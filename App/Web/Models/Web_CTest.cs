using CSick.Actors._CTests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSick.Web.Models {
    public class Web_CTest {
        public string name;
        public int lineNumber;
        public int testNumber;
        public string runStatus;

        public Web_TestResult testResult;
        public Web_RootSourceFile parent;
    }

    public class Web_TestResult {
        public bool finished;
        public bool success;
        public int exitCode;
        public DateTimeOffset timeStarted;
        public DateTimeOffset timeStopped;
        public string output;
    }
}
