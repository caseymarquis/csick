using CSick.Actors._CTests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSick.Web.Models {
    public class Web_RootSourceFile {
        public string fileName;
        public string path;
        public string pathHash;
        public string compileStatus;
        public string[] lines;

        public Web_CompileResult compileResult;        

        public List<Web_CTest> tests;
    }

    public class Web_CompileResult {
        public bool finished;
        public bool success;
        public DateTimeOffset timeStarted;
        public DateTimeOffset timeStopped;
        public string output;
    }
}
