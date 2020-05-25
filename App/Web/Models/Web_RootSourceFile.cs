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

        public Web_ProcResult compileResult;

        public List<Web_CTest> tests;

        public string[] lines;
    }
}
