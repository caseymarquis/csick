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
    public class CTests_AvailableTest : Actor {
        [FlexibleParent] CTests_AvailableTestFile parentFile;
        [Singleton] Signalr_SendUpdates sendUpdates;
        [Singleton] AppSettings settings;

        protected override TimeSpan RunDelay => new TimeSpan(0, 0, 0, 0, 50);

        public readonly MessageQueue<CTestCommand> Commands = new MessageQueue<CTestCommand>();

        public CTest Test => parentFile.SourceFile.Tests.FirstOrDefault(x => x.TestNumber == this.Id);

        private readonly Atom<RunStatus> runStatus = new Atom<RunStatus>(RunStatus.WaitingOnParent);
        public RunStatus RunStatus => runStatus.Value;

        private readonly Atom<TestResult> testResult = new Atom<TestResult>();
        public TestResult TestResult => testResult.Value;

        DateTimeOffset lastKnownParentCompileTime;
        DateTimeOffset testStarted;
        Process testProcess;

        private object lockStringBuilders = new object();
        StringBuilder sbStdOut = new StringBuilder();
        StringBuilder sbStdErr = new StringBuilder();

        public string StandardOut {
            get { lock (lockStringBuilders) return sbStdOut.ToString(); }
        }
        public string StandardError {
            get { lock (lockStringBuilders) return sbStdErr.ToString(); }
        }

        protected override async Task OnRun(ActorUtil util) {
            var cmds = Commands.DequeueAll();

            var statusWas = this.runStatus.Value;

            var compileResult = parentFile.CompileResult;
            var parentHasCompiled = compileResult.Finished && compileResult.Success;
            var parentWasRecompiled = parentHasCompiled && compileResult.TimeStopped != lastKnownParentCompileTime;
            lastKnownParentCompileTime = compileResult.TimeStopped;

            var shouldPause = cmds.Any(x => x == CTestCommand.Cancel);
            var shouldResumeOrForce = !shouldPause && cmds.Any(x => x == CTestCommand.Run);
            switch (statusWas) {
                case RunStatus.WaitingOnParent:
                    if (shouldPause) {
                        runStatus.Value = RunStatus.Paused;
                    }
                    else if (parentWasRecompiled || (shouldResumeOrForce && parentHasCompiled)) {
                        runStatus.Value = RunStatus.Scheduled;
                    }
                    break;
                case RunStatus.Scheduled:
                    if (shouldPause) {
                        runStatus.Value = RunStatus.Paused;
                    }
                    else if (!parentHasCompiled) {
                        runStatus.Value = RunStatus.WaitingOnParent;
                    }
                    else {
                        try {
                            if (startTest(out var failedResult)) {
                                runStatus.Value = RunStatus.Running;
                            }
                            else {
                                this.testResult.Value = failedResult;
                                runStatus.Value = RunStatus.WaitingOnParent;
                            }
                        }
                        catch (Exception ex) {
                            util.Log.Error(ex);
                            this.testResult.Value = new TestResult(error: ex.Message, testStarted, util.Now);
                            runStatus.Value = RunStatus.WaitingOnParent;
                        }
                    }
                    break;
                case RunStatus.Running:
                    if (shouldPause) {
                        testResult.Value = cancelTest();
                        runStatus.Value = RunStatus.Paused;
                    }
                    else {
                        var doneTesting = testProcess.HasExited;
                        await readFromStream(testProcess.StandardOutput, sbStdOut);
                        await readFromStream(testProcess.StandardError, sbStdErr);
                        if (doneTesting) {
                            var exitCode = testProcess.ExitCode;
                            if (exitCode != 0) {
                                this.testResult.Value = new TestResult(
                                    finished: true,
                                    success: false,
                                    exitCode: exitCode,
                                    testProcess.StartTime,
                                    testProcess.ExitTime,
                                    StandardError);
                                this.runStatus.Value = RunStatus.WaitingOnParent;
                            }
                            else {
                                this.testResult.Value = new TestResult(finished: true, success: true, testProcess.ExitCode, testProcess.StartTime, testProcess.ExitTime, StandardOut);
                                this.runStatus.Value = RunStatus.WaitingOnParent;
                            }
                        }
                    }
                    break;
                case RunStatus.Paused:
                    if (shouldResumeOrForce) {
                        runStatus.Value = parentHasCompiled? RunStatus.Scheduled : RunStatus.WaitingOnParent;
                    }
                    break;
            }

            if (statusWas != RunStatus) {
                triggerUpdate();
            }

            await Task.FromResult(0);

            bool startTest(out TestResult failedResult) {
                lock (lockStringBuilders) {
                    sbStdOut.Clear();
                    sbStdErr.Clear();
                }
                testStarted = util.Now;
                try {
                    //TODO: Out file is effectively hardcoded to ./bin/{fileName}{optionalOSExtension}
                    //Changing this would be a giant pain.
                    var originalPath = parentFile.SourceFile.FilePath;
                    var executablePath = Path.Combine(Path.GetDirectoryName(originalPath), "bin", Path.GetFileNameWithoutExtension(originalPath));
                    var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                    if (isWindows) {
                        executablePath += ".exe";
                    }
                    var psi = new ProcessStartInfo {
                        FileName = executablePath,
                        CreateNoWindow = true,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        WorkingDirectory = Path.GetDirectoryName(executablePath),
                        Arguments = string.Join(' ', Test.TestNumber.ToString())
                    };
                    testProcess = Process.Start(psi);
                    failedResult = new TestResult(notDone: true);
                    return true;
                }
                catch (Exception ex) {
                    failedResult = new TestResult(error: $"Failed to start test: {ex.Message}", testStarted, util.Now);
                    return false;
                }
            }

            TestResult cancelTest() {
                try {
                    if (!testProcess.HasExited) {
                        testProcess.Kill(true); //TODO: What if this hangs forever?
                    }
                }
                catch (Exception ex) {
                    util.Log.Error("Failed to cancel test.", ex);
                }
                return new TestResult("Test Canceled", testStarted, util.Now);
            }

            async Task readFromStream(StreamReader stream, StringBuilder to) {
                if (testProcess == null) {
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
                sendUpdates.Send_UpdateTest(parentFile.SourceFile.FileName ?? "", this.Id);
            }
        }
    }
}
