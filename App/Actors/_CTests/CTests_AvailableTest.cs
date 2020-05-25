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
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSick.Actors._CTests {
    [Instance]
    public class CTests_AvailableTest : Actor {
        [FlexibleParent] CTests_AvailableTestFile parentFile;
        [Singleton] Signalr_SendUpdates sendUpdates;
        [Instance] ProcessRunner processRunner;

        protected override TimeSpan RunDelay => new TimeSpan(0, 0, 0, 0, 50);

        public readonly MessageQueue<CTestCommand> Commands = new MessageQueue<CTestCommand>();

        public CTest Test => parentFile.SourceFile.Tests.FirstOrDefault(x => x.TestNumber == this.Id);

        private readonly Atom<RunStatus> runStatusAtom = new Atom<RunStatus>(RunStatus.WaitingOnParent);
        public RunStatus RunStatus => runStatusAtom.Value;

        DateTimeOffset lastKnownExecutableVersion;
        DateTimeOffset lastWaitStarted;

        Atom<Guid> processReceiptAtom = new Atom<Guid>();
        Atom<ProcHandle> procHandleAtom = new Atom<ProcHandle>(null);
        public ProcHandle ProcHandle => procHandleAtom.Value;

        Atom<ProcResult> lastCompletedRunResultAtom = new Atom<ProcResult>();
        public ProcResult LastCompletedRunResult => lastCompletedRunResultAtom.Value;

        protected override async Task OnRun(ActorUtil util) {
            var cmds = Commands.DequeueAll();

            var parentHasCompiled = parentFile.CompileStatus == CompileStatus.Compiled;
            var parentExecutableVersion = parentFile.ExecutableVersion;
            var parentWasRecompiled = parentHasCompiled && parentExecutableVersion != lastKnownExecutableVersion;
            if (parentHasCompiled) {
                lastKnownExecutableVersion = parentExecutableVersion;
            }

            var shouldPause = cmds.Any(x => x == CTestCommand.Cancel);
            var shouldResumeOrForce = !shouldPause && cmds.Any(x => x == CTestCommand.Run);

            var statusWas = this.runStatusAtom.Value;
            this.runStatusAtom.Value = step();

            if (statusWas != RunStatus) {
                triggerUpdate();
            }

            await Task.FromResult(0);
            return;

            RunStatus step() {
                switch (statusWas) {
                    case RunStatus.WaitingOnParent:
                    case RunStatus.TimedOut:
                        if (shouldPause) {
                            return RunStatus.Paused;
                        }
                        else if (parentWasRecompiled || (shouldResumeOrForce && parentHasCompiled)) {
                            return RunStatus.Scheduled;
                        }
                        else {
                            return statusWas;
                        }
                    case RunStatus.Scheduled:
                        if (shouldPause) {
                            return RunStatus.Paused;
                        }
                        else if (!parentHasCompiled) {
                            return RunStatus.WaitingOnParent;
                        }
                        else {
                            this.ProcHandle?.Cancel();
                            this.procHandleAtom.Value = null;
                            var originalPath = parentFile.SourceFile.FilePath;
                            var executablePath = Path.Combine(Path.GetDirectoryName(originalPath), "bin", Path.GetFileNameWithoutExtension(originalPath));
                            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                            if (isWindows) {
                                executablePath += ".exe";
                            }
                            processReceiptAtom.Value = processRunner.StartProcess(new ProcStartInfo(
                                path: executablePath,
                                workingDirectory: Path.GetDirectoryName(executablePath),
                                arguments: new string[] { Test.LineNumber.ToString() },
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
                            return RunStatus.WaitingOnProcessStart;
                        }
                    case RunStatus.WaitingOnProcessStart:
                        if (shouldPause) {
                            this.processReceiptAtom.Value = Guid.Empty;
                            return RunStatus.Paused;
                        }
                        else if (ProcHandle != null) {
                            startWait();
                            return RunStatus.Running;
                        }
                        else if (haveWaited(new TimeSpan(0, 0, 5))) {
                            ProcHandle?.Cancel();
                            return RunStatus.TimedOut;
                        }
                        else {
                            return RunStatus.WaitingOnProcessStart;
                        }
                    case RunStatus.Running:
                        if (shouldPause) {
                            ProcHandle?.Cancel();
                            return RunStatus.Paused;
                        }
                        else {
                            var ph = this.ProcHandle;
                            if (haveWaited(new TimeSpan(0, 0, 5))) {
                                return RunStatus.TimedOut;
                            }
                            else if (ph.Status != ProcessStatus.Finished) {
                                return RunStatus.Running;
                            }
                            else {
                                lastCompletedRunResultAtom.Value = ph.Result;
                                return RunStatus.WaitingOnParent;
                            }
                        }
                    case RunStatus.Paused:
                        if (shouldResumeOrForce) {
                            return parentHasCompiled ? RunStatus.Scheduled : RunStatus.WaitingOnParent;
                        }
                        else {
                            return RunStatus.Paused;
                        }
                    default:
                        throw new NotImplementedException(statusWas.ToString("g"));
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
                sendUpdates.Send_UpdateTest(parentFile.SourceFile.FilePath ?? "", this.Id);
            }
        }
    }
}
