using CSick.Actors._CTests.Helpers;
using CSick.Actors.ProcessRunnerNS;
using CSick.Actors.ProcessRunnerNS.Helpers;
using CSick.Actors.Signalr;
using KC.Actin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSick.Actors._CTests {
    [Instance]
    public class CTests_AvailableTestFile : Scene<CTests_AvailableTest, Role, int, Role<string>, string> {
        [FlexibleParent] CTests_AvailableTestFiles parent;
        [Singleton] AppSettings settings;
        [Singleton] Signalr_SendUpdates sendUpdates;
        [Instance] ProcessRunner processRunner;

        protected override TimeSpan RunDelay => new TimeSpan(0, 0, 0, 0, 50);

        Atom<CTestSourceFile> sourceFileAtom = new Atom<CTestSourceFile>();
        public CTestSourceFile SourceFile => sourceFileAtom.Value;

        Atom<ProcResult> lastCompileResultAtom = new Atom<ProcResult>();
        public ProcResult LastCompileResult => lastCompileResultAtom.Value;

        readonly Atom<CompileStatus> compileStatusAtom = new Atom<CompileStatus>(Helpers.CompileStatus.Modified);
        public CompileStatus CompileStatus => compileStatusAtom.Value;

        Atom<DateTimeOffset> executableVersionAtom = new Atom<DateTimeOffset>();
        public DateTimeOffset ExecutableVersion => executableVersionAtom.Value;
        DateTimeOffset lastWaitStarted = DateTimeOffset.MinValue;

        Atom<Guid> processReceiptAtom = new Atom<Guid>();
        Atom<ProcHandle> procHandleAtom = new Atom<ProcHandle>(null);
        public ProcHandle ProcHandle => procHandleAtom.Value;

        protected override async Task<IEnumerable<Role>> CastActors(ActorUtil util, Dictionary<int, CTests_AvailableTest> myActors) {
            var mySourceFile = parent.RootSourceFiles.FirstOrDefault(x => x.FilePath == this.Id);
            if (mySourceFile.FilePath == null) {
                this.Dispose();
                triggerUpdate();
                return null;
            }

            if (sourceFileAtom.Value.ParseTime != mySourceFile.ParseTime) {
                sourceFileAtom.Value = mySourceFile;
                triggerUpdate();
            }

            var compileStatusWas = this.CompileStatus;
            this.compileStatusAtom.Value = step();

            if (compileStatusWas != CompileStatus) {
                triggerUpdate();
            }

            await Task.FromResult(0);
            return mySourceFile.Tests.Select(x => new Role {
                Id = x.TestNumber,
            });

            CompileStatus step() {
                switch (compileStatusWas) {
                    case CompileStatus.Modified:
                        executableVersionAtom.Value = mySourceFile.ParseTime;
                        var us = settings.UserSettings;
                        try {
                            Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(mySourceFile.FilePath), "bin"));
                        }
                        catch { }
                        this.ProcHandle?.Cancel();
                        this.procHandleAtom.Value = null;
                        processReceiptAtom.Value = processRunner.StartProcess(new ProcStartInfo(
                            path: us.CompilerPath,
                            workingDirectory: Path.GetDirectoryName(mySourceFile.FilePath),
                            arguments: us.GetProcessedCompileArguments(mySourceFile.FilePath),
                            maxRunTime: new TimeSpan(0, 0, 10),
                            accept: newHandle => {
                                if (newHandle.Id == processReceiptAtom.Value) {
                                    this.procHandleAtom.Value = newHandle;
                                }
                                else {
                                    newHandle.Cancel();
                                }
                            }
                        ));
                        startWait();
                        return CompileStatus.WaitingOnProcessStart;
                    case CompileStatus.WaitingOnProcessStart:
                        if (ProcHandle != null) {
                            startWait();
                            return CompileStatus.Compiling;
                        }
                        else if (haveWaited(new TimeSpan(0, 0, 5))) {
                            ProcHandle?.Cancel();
                            return CompileStatus.TimedOut;
                        }
                        else {
                            return CompileStatus.WaitingOnProcessStart;
                        }
                    case CompileStatus.Compiling:
                        var ph = this.ProcHandle;
                        if (haveWaited(new TimeSpan(0, 0, 5))) {
                            return CompileStatus.TimedOut;
                        }
                        else if (ph.Status != ProcessStatus.Finished) {
                            return CompileStatus.Compiling;
                        }
                        else {
                            var result = ph.Result;
                            lastCompileResultAtom.Value = result;
                            if (result.GracefulExit && result.ExitCode == 0) {
                                return CompileStatus.Compiled;
                            }
                            else {
                                return CompileStatus.Failed; 
                            }
                        }
                    case CompileStatus.Failed:
                    case CompileStatus.TimedOut:
                    case CompileStatus.Compiled:
                        if (executableVersionAtom.Value != mySourceFile.ParseTime) {
                            return CompileStatus.Modified;
                        }
                        else {
                            return compileStatusWas;
                        }
                    default:
                        throw new NotImplementedException(compileStatusWas.ToString("g"));
                }
            }

            void startWait() {
                lastWaitStarted = util.Now;
            }

            bool haveWaited(TimeSpan moreThanThis) {
                var timeSpentWaiting = util.Now - lastWaitStarted;
                return timeSpentWaiting > moreThanThis;
            }

            void triggerUpdate() {
                sendUpdates.Send_UpdateTestsFile(mySourceFile.FilePath ?? "");
            }
        }
    }
}
