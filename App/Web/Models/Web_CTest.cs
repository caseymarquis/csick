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

        public Web_ProcResult testResult;
        public Web_RootSourceFile parent;
    }
}
