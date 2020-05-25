using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSick.Actors._CTests.Helpers {
    public enum CompileStatus {
        Modified,
        WaitingOnProcessStart,
        Compiling,
        Failed,
        TimedOut,
        Compiled,
    }
}
