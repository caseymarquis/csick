using CSick.Actors._CTests.Helpers;
using CSick.Actors.Signalr;
using KC.Actin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace CSick.Actors._CTests {
    [Instance]
    public class CTests_AvailableTestFile : Scene<CTests_AvailableTest, Role, int, Role<string>, string> {
        [FlexibleParent] CTests_AvailableTestFiles parent;
        [Singleton] AppSettings settings;
        [Singleton] Signalr_SendUpdates sendUpdates;

        protected override TimeSpan RunDelay => new TimeSpan(0, 0, 0, 0, 50);

        Atom<CTestSourceFile> sourceFile = new Atom<CTestSourceFile>();
        public CTestSourceFile SourceFile => sourceFile.Value;

        readonly Atom<CompileStatus> compileStatus = new Atom<CompileStatus>(Helpers.CompileStatus.Modified);
        public CompileStatus CompileStatus => compileStatus.Value;

        readonly Atom<CompileResult> compileResult = new Atom<CompileResult>(new CompileResult(notDone: true));
        public CompileResult CompileResult => compileResult.Value;

        DateTimeOffset sourceVersion = DateTimeOffset.MinValue;
        DateTimeOffset compileStarted;
        Process compileProcess;

        private object lockStringBuilders = new object();
        StringBuilder sbStdOut = new StringBuilder();
        StringBuilder sbStdErr = new StringBuilder();

        public string StandardOut {
            get { lock (lockStringBuilders) return sbStdOut.ToString(); }
        }
        public string StandardError {
            get { lock (lockStringBuilders) return sbStdErr.ToString(); }
        }

        protected override async Task<IEnumerable<Role>> CastActors(ActorUtil util, Dictionary<int, CTests_AvailableTest> myActors) {
            var mySourceFile = parent.RootSourceFiles.FirstOrDefault(x => x.FilePath == this.Id);
            if (mySourceFile.FilePath == null) {
                this.Dispose();
                triggerUpdate();
                return null;
            }

            if (sourceFile.Value.ParseTime != mySourceFile.ParseTime) {
                sourceFile.Value = mySourceFile;
                triggerUpdate();
            }
            var compileStatusWas = this.CompileStatus;

            var result = mySourceFile.Tests.Select(x => new Role {
                Id = x.TestNumber,
            });

            var status = CompileStatus;
            switch (status) {
                case CompileStatus.Modified:
                    compileResult.Value = new CompileResult(notDone: true);
                    try {
                        if (startCompile(out var failedResult)) {
                            compileStatus.Value = CompileStatus.Compiling;
                        }
                        else {
                            compileResult.Value = failedResult;
                            compileStatus.Value = CompileStatus.Failed;
                        }
                    }
                    catch (Exception ex) {
                        util.Log.Error(ex);
                        this.compileResult.Value = new CompileResult(error: ex.Message, compileStarted, util.Now);
                        compileStatus.Value = CompileStatus.Failed;
                    }
                    break;
                case CompileStatus.Compiling:
                    //TODO: What if a file is modified during compile?
                    //Right now we have to wait. If the compile is stuck, that's bad.
                    var doneCompiling = compileProcess.HasExited;
                    await readFromStream(compileProcess.StandardOutput, sbStdOut);
                    await readFromStream(compileProcess.StandardError, sbStdErr);
                    if (doneCompiling) {
                        var exitCode = compileProcess.ExitCode;
                        if (exitCode != 0) {
                            this.compileResult.Value = new CompileResult(
                                error: $"Exit Code: {exitCode}:{Environment.NewLine}{StandardError}",
                                compileProcess.StartTime,
                                compileProcess.ExitTime);

                            this.compileStatus.Value = CompileStatus.Failed;
                        }
                        else {
                            this.compileResult.Value = new CompileResult(finished: true, success: true, compileProcess.StartTime, compileProcess.ExitTime, StandardOut);
                            this.compileStatus.Value = CompileStatus.Compiled;
                        }
                    }
                    break;
                case CompileStatus.Failed:
                case CompileStatus.Compiled:
                    if (sourceVersion != mySourceFile.ParseTime) {
                        compileStatus.Value = CompileStatus.Modified;
                    }
                    break;
                default:
                    throw new NotImplementedException(status.ToString("g"));
            }

            if (compileStatusWas != CompileStatus) {
                triggerUpdate();
            }

            bool startCompile(out CompileResult failedResult) {
                lock (lockStringBuilders) {
                    sbStdOut.Clear();
                    sbStdErr.Clear();
                }
                sourceVersion = mySourceFile.ParseTime;
                compileStarted = util.Now;
                var us = settings.UserSettings;
                try {
                    var psi = new ProcessStartInfo {
                        FileName = us.CompilerPath,
                        CreateNoWindow = true,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        WorkingDirectory = Path.GetDirectoryName(mySourceFile.FilePath),
                        Arguments = string.Join(' ', us.GetProcessedCompileArguments(mySourceFile.FilePath))
                    };
                    try {
                        //TODO: Out file is effectively hardcoded to ./bin/{fileName}{optionalOSExtension}
                        //Changing this would be a giant pain.
                        Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(mySourceFile.FilePath), "bin"));
                    }
                    catch { }
                    compileProcess = Process.Start(psi);
                    failedResult = new CompileResult(notDone: true);
                    return true;
                }
                catch (Exception ex) {
                    failedResult = new CompileResult(error: $"Failed to start compiler: {ex.Message}", compileStarted, util.Now);
                    return false;
                }
            }

            async Task readFromStream(StreamReader stream, StringBuilder to) {
                if (compileProcess == null) {
                    return;
                }
                var buffer = new char[256];
                while (stream.Peek() > 0) {
                    var amountRead = await stream.ReadBlockAsync(buffer, 0, buffer.Length);
                    if (amountRead == 0) {
                        break; //Just in case
                    }
                    else {
                        triggerUpdate();
                    }
                    lock (lockStringBuilders) {
                        to.Append(buffer, 0, amountRead);
                    }
                }
            }

            void triggerUpdate() {
                sendUpdates.Send_UpdateTestsFile(mySourceFile.FileName ?? "");
            }

            return await Task.FromResult(result);
        }
    }
}
