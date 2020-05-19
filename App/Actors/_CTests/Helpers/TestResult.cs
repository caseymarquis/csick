using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSick.Actors._CTests.Helpers {
    public readonly struct TestResult {
        public readonly bool Finished;
        public readonly bool Success;
        public readonly int ExitCode;
        public readonly DateTimeOffset TimeStarted;
        public readonly DateTimeOffset TimeStopped;
        public readonly string Output;

        public TestResult(bool notDone) {
            this.Finished = false;
            this.Success = false;
            this.ExitCode = 0;
            this.TimeStarted = DateTimeOffset.MinValue;
            this.TimeStopped = DateTimeOffset.MinValue;
            this.Output = "";
        }

        public TestResult(string error, DateTimeOffset timeStarted, DateTimeOffset timeFinished, int exitCode = 0) {
            this.Finished = false;
            this.Success = false;
            this.ExitCode = exitCode;
            this.TimeStarted = timeStarted;
            this.TimeStopped = timeFinished;
            this.Output = error;
        }

        public TestResult(bool finished, bool success, int exitCode, DateTimeOffset timeStarted, DateTimeOffset timeFinished, string output) {
            this.Finished = finished;
            this.Success = success;
            this.ExitCode = exitCode;
            this.TimeStarted = timeStarted;
            this.TimeStopped = timeFinished;
            this.Output = output ?? "";
        }
    }
}
