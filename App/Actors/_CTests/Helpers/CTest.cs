using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSick.Actors._CTests.Helpers {
    public readonly struct CTest {
        public readonly int TestNumber;
        public readonly int LineNumber;
        public readonly string Name;

        public CTest(int testNumber, int lineNumber, string name) {
            this.TestNumber = testNumber;
            this.LineNumber = lineNumber;
            this.Name = name;
        }
    }
}
