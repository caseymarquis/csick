using CSick.Actors._CTests.Helpers;
using CSick.Actors.ProcessRunnerNS.Helpers;
using KC.Actin;
using Microsoft.AspNetCore.SignalR.Protocol;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSick.Actors.ProcessRunnerNS {
    [Instance]
    public class ProcessRunner : Actor {

        protected override TimeSpan RunDelay => new TimeSpan(0, 0, 0, 0, 50);

        MessageQueue<(Guid, ProcStartInfo)> startQueue = new MessageQueue<(Guid, ProcStartInfo)>();
        public Guid StartProcess(ProcStartInfo startInfo) {
            var receipt = Guid.NewGuid();
            this.startQueue.Enqueue((receipt, startInfo));
            return receipt;
        }

        DateTimeOffset runStarted;
        private ProcHandle m_currentProcess;
        protected override async Task OnRun(ActorUtil util) {
            var haveNewProcessToStart = startQueue.TryDequeueAll(out var msgs);
            if (haveNewProcessToStart) {
                this.m_currentProcess?.Cancel();
                var (id, startInfo) = msgs.Last();
                this.m_currentProcess = new ProcHandle(startInfo, id);
                startInfo.__Accept__(this.m_currentProcess);
            }

            var workingProcess = m_currentProcess;
            if (m_currentProcess == null) {
                return;
            }

            var newStatus = step(out var result);
            workingProcess.EditWithLock(x => {
                x.SetStatusAndResult(newStatus, result);
            });

            await Task.FromResult(0);
            return;

            ProcessStatus step(out ProcResult result) {
                var statusWas = workingProcess.Status;

                switch (statusWas) {
                    case ProcessStatus.NotStarted:
                        startWait();
                        startTheProcess(workingProcess, runStarted);
                        result = new ProcResult(notDone: true);
                        return ProcessStatus.Running;
                    case ProcessStatus.Running:
                        if (haveWaited(workingProcess.StartInfo.MaxRunTime)) {
                            doKill(runStarted, null);
                            result = new ProcResult($"Timed out after {workingProcess.StartInfo.MaxRunTime.TotalSeconds} seconds.", runStarted, util.Now);
                            return ProcessStatus.Finished;
                        }
                        result = new ProcResult(notDone: true);
                        return ProcessStatus.Running;
                    case ProcessStatus.Canceled:
                        doKill(runStarted, null);
                        result = new ProcResult("Canceled", runStarted, util.Now);
                        return ProcessStatus.Finished;
                    case ProcessStatus.Finished:
                        this.m_currentProcess = null;
                        result = workingProcess.ReadWithLock(x => x.Result); //This is superfluous, but seems like the most correct option.
                        return ProcessStatus.Finished;
                    default:
                        throw new NotImplementedException(workingProcess.Status.ToString("g"));
                }

                void startWait() {
                    runStarted = util.Now;
                }

                bool haveWaited(TimeSpan moreThanThis) {
                    var timeSpentWaiting = util.Now - runStarted;
                    return timeSpentWaiting > moreThanThis;
                }
            }

            bool startTheProcess(ProcHandle workingProcess, DateTimeOffset started) {
                try {
                    actuallyStartUpTheProcess();
                    startTheMonitoringThread();
                    return true;
                }
                catch (Exception ex) {
                    try {
                        workingProcess.WithProcess(new TimeSpan(0, 0, 5), proc => {
                            proc?.Dispose();
                        });
                    }
                    catch { }
                    workingProcess.EditWithLock(x => {
                        x.SetStatusAndResult(ProcessStatus.Finished, new ProcResult(ex.Message, started, DateTimeOffset.Now));
                    });
                    return false;
                }

                void actuallyStartUpTheProcess() {
                    var startInfo = workingProcess.StartInfo;
                    var psi = new System.Diagnostics.ProcessStartInfo {
                        FileName = startInfo.Path,
                        CreateNoWindow = true,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        WorkingDirectory = startInfo.WorkingDirectory ?? "./",
                        Arguments = string.Join(' ', startInfo.Arguments)
                    };
                    workingProcess.StartProcess(psi);
                }

                void startTheMonitoringThread() {
                    //Once the process is started, we MUST only touch it
                    //in this thread. Otherwise if the external process blocks,
                    //we will block and be unable to run doKill();
                    var t = new Thread(() => {
                        try {
                            while (ProcessStatus.Running == workingProcess.ReadWithLock(x => x.Status)) {
                                try {
                                    workingProcess.WithProcess(new TimeSpan(0, 0, 5), proc => {
                                        if (proc.HasExited) {
                                            readOutput();
                                            var exitCode = proc.ExitCode;
                                            var started = proc.StartTime;
                                            var ended = proc.ExitTime;
                                            workingProcess.EditWithLock(x => {
                                                var stdOut = x.StdOut.ToString();
                                                var stdErr = x.StdErr.ToString();

                                                var output = "";
                                                if (!string.IsNullOrWhiteSpace(stdOut)) {
                                                    output += "Std Out: " + Environment.NewLine + stdOut;
                                                }
                                                if (!string.IsNullOrWhiteSpace(stdErr)) {
                                                    if (!string.IsNullOrWhiteSpace(output)) {
                                                        output += Environment.NewLine;
                                                        output += Environment.NewLine;
                                                    }
                                                    output += "Std Err: " + Environment.NewLine + stdErr;
                                                }

                                                x.SetStatusAndResult(ProcessStatus.Finished,
                                                    new ProcResult(true, true, exitCode, started, ended, output));
                                            });
                                        }

                                        void readOutput() {
                                            if (readFromStream(proc.StandardOutput, out var stdSb)) {
                                                workingProcess.EditWithLock(x => {
                                                    x.StdOut.Append(stdSb);
                                                });
                                            }
                                            if (readFromStream(proc.StandardError, out var stdErr)) {
                                                workingProcess.EditWithLock(x => {
                                                    x.StdErr.Append(stdErr);
                                                });
                                            }
                                        }
                                    });

                                    bool readFromStream(StreamReader stream, out StringBuilder sb) {
                                        var buffer = new char[256];
                                        var readSomething = false;
                                        sb = null;
                                        for (; ; ) {
                                            var amountRead = stream.Read(buffer, 0, buffer.Length);
                                            if (amountRead == 0) {
                                                break; //Exit as soon as there's nothing to read
                                            }
                                            else {
                                                readSomething = true;
                                                if (sb == null) {
                                                    sb = new StringBuilder();
                                                }
                                                sb.Append(buffer, 0, amountRead);
                                            }
                                        }
                                        return readSomething;
                                    }
                                }
                                catch (Exception ex) {
                                    doKill(started, ex);
                                }
                            }
                        }
                        catch (Exception ex) {
                            doKill(started, ex);
                        }
                    });
                    t.IsBackground = true;
                    workingProcess.EditWithLock(x => {
                        x.ProcessMonitorThread = t;
                    });
                    t.Start();
                }
            }

            void doKill(DateTimeOffset started, Exception ex = null) {
                //We cancel in a separate thread just in case the cancel locks up.
                var t = new Thread(() => {
                    try {
                        var timeStarted = started;
                        var timeEnded = DateTimeOffset.Now;
                        var exitCode = -9999;
                        try {
                            if (!workingProcess.TryWithProcess(new TimeSpan(0, 0, 5), proc => {
                                if (proc?.HasExited == false) {
                                    proc.Kill();
                                    proc.WaitForExit(3000); //We are blocking inside the lock. Perhaps there's a better option, but maybe not.
                                }
                                try {
                                    if (proc?.HasExited == true) {
                                        timeStarted = proc.StartTime;
                                        timeEnded = proc.ExitTime;
                                        exitCode = proc.ExitCode;
                                    }
                                }
                                catch { }
                            })) {
                                workingProcess.KillProcessWithNoLock();
                            };
                        }
                        catch { }
                        
                        workingProcess.EditWithLock(x => {
                            x.SetStatusAndResult(ProcessStatus.Finished, new ProcResult(true, false, exitCode, timeStarted, timeEnded, ex?.Message ?? "Canceled"));
                        });
                    }
                    catch {
                        workingProcess.EditWithLock(x => {
                            x.SetStatusAndResult(ProcessStatus.Finished, new ProcResult("Unknown Failure", DateTimeOffset.MinValue, DateTimeOffset.MinValue));
                        });
                    }
                });
                t.IsBackground = true;
                t.Start();

                //If the kill thread timed out, then ensure the finished status gets out:
                Task.Run(async () => {
                    try {
                        await Task.Delay(5000);
                        if (workingProcess.Status != ProcessStatus.Finished) {
                            workingProcess.EditWithLock(x => {
                                x.SetStatusAndResult(ProcessStatus.Finished, new ProcResult("Kill timed out.", DateTimeOffset.MinValue, DateTimeOffset.MinValue));
                            });
                        }
                        try {
                            if (t.IsAlive) {
                                t.Abort();
                            }
                        }
                        catch { }
                        try {
                            var processThread = workingProcess.ReadWithLock(x => x.ProcessMonitorThread);
                            if (processThread.IsAlive) {
                                processThread.Abort();
                            }
                        }
                        catch { }
                    }
                    catch { }
                });
            }
        }
    }

}
