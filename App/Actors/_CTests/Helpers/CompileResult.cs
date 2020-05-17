using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace CSick.Actors._CTests.Helpers {
    public readonly struct CompileResult {
        public readonly bool Finished;
        public readonly bool Success;
        public readonly DateTimeOffset TimeStarted;
        public readonly DateTimeOffset TimeFinished;
        public readonly string Output;

        public CompileResult(bool notDone) {
            this.Finished = false;
            this.Success = false;
            this.TimeStarted = DateTimeOffset.MinValue;
            this.TimeFinished = DateTimeOffset.MinValue;
            this.Output = "";
        }

        public CompileResult(string singleError, DateTimeOffset timeStarted, DateTimeOffset timeFinished) {
            this.Finished = true;
            this.Success = false;
            this.TimeStarted = timeStarted;
            this.TimeFinished = timeFinished;
            this.Output = singleError ?? "";
        }

        public CompileResult(bool finished, bool success, DateTimeOffset timeStarted, DateTimeOffset timeFinished, string output) {
            this.Finished = finished;
            this.Success = success;
            this.TimeStarted = timeStarted;
            this.TimeFinished = timeFinished;
            this.Output = output ?? "";
        }
    }
}
