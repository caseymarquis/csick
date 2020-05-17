using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSick.Actors._CTests.Helpers {
    [Flags]
    public enum CTestCommand {
        Cancel = 1,
        Run = 2,
    }
}
