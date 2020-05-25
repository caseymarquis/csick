using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSick.Actors.ProcessRunnerNS {
    public readonly struct ProcResult {
        public readonly bool Finished;
        public readonly bool GracefulExit;
        public readonly int ExitCode;
        public readonly DateTimeOffset TimeStarted;
        public readonly DateTimeOffset TimeStopped;
        public readonly string Output;

        public ProcResult(bool notDone) {
            this.Finished = false;
            this.GracefulExit = false;
            this.ExitCode = 0;
            this.TimeStarted = DateTimeOffset.MinValue;
            this.TimeStopped = DateTimeOffset.MinValue;
            this.Output = "";
        }

        public ProcResult(string error, DateTimeOffset timeStarted, DateTimeOffset timeFinished) {
            this.Finished = false;
            this.GracefulExit = false;
            this.ExitCode = -9876;
            this.TimeStarted = timeStarted;
            this.TimeStopped = timeFinished;
            this.Output = error;
        }

        public ProcResult(bool finished, bool gracefulExit, int exitCode, DateTimeOffset timeStarted, DateTimeOffset timeFinished, string output) {
            this.Finished = finished;
            this.GracefulExit = gracefulExit;
            this.ExitCode = exitCode;
            this.TimeStarted = timeStarted;
            this.TimeStopped = timeFinished;
            this.Output = output ?? "";
        }
    }
}
