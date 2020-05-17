using CSick.Actors._CTests.Helpers;
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

        Atom<CTestSourceFile> sourceFile = new Atom<CTestSourceFile>();
        public CTestSourceFile SourceFile => sourceFile.Value;

        readonly Atom<CompileStatus> compileStatus = new Atom<CompileStatus>(Helpers.CompileStatus.Modified);
        public CompileStatus CompileStatus => compileStatus.Value;

        readonly Atom<CompileResult> compileResult = new Atom<CompileResult>(new CompileResult(notDone: true));

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
                return null;
            }
            sourceFile.Value = mySourceFile;

            var result = mySourceFile.Tests.Select(x => new Role {
                Id = x.TestNumber,
            });

            var status = CompileStatus;
            switch (status) {
                case CompileStatus.Modified:
                    try {
                        compileStatus.Value = startCompile() ? CompileStatus.Compiling : CompileStatus.Failed;
                    }
                    catch (Exception ex) {
                        util.Log.Error(ex);
                        this.compileResult.Value = new CompileResult(singleError: ex.Message, compileStarted, util.Now);
                        compileStatus.Value = CompileStatus.Failed;
                    }
                    break;
                case CompileStatus.Compiling:
                    var doneCompiling = compileProcess.HasExited;
                    await readFromStream(compileProcess.StandardOutput, sbStdOut);
                    await readFromStream(compileProcess.StandardError, sbStdErr);
                    if (doneCompiling) {
                        var exitCode = compileProcess.ExitCode;
                        if (exitCode != 0) {
                            this.compileResult.Value = new CompileResult(
                                singleError: $"Exit Code: {exitCode}:{Environment.NewLine}{StandardError}",
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

            bool startCompile() {
                lock (lockStringBuilders) {
                    sbStdOut.Clear();
                    sbStdErr.Clear();
                }
                sourceVersion = mySourceFile.ParseTime;
                compileStarted = util.Now;
                compileResult.Value = new CompileResult(notDone: true);
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
                        //HACK: Only annoying if you change the default out location...
                        Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(mySourceFile.FilePath), "bin"));
                    }
                    catch { }
                    compileProcess = Process.Start(psi);
                    return true;
                }
                catch (Exception ex) {
                    compileResult.Value = new CompileResult(singleError: $"Failed to start compiler: {ex.Message}", compileStarted, util.Now);
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
                    lock (lockStringBuilders) {
                        to.Append(buffer, 0, amountRead);
                    }
                }
            }

            return await Task.FromResult(result);
        }
    }
}
