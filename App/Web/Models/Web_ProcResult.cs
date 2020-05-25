using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSick.Web.Models {
    public class Web_ProcResult {
        public bool finished;
        public bool gracefulExit;
        public bool success => finished && gracefulExit && exitCode == 0;
        public int exitCode;
        public DateTimeOffset timeStarted;
        public DateTimeOffset timeStopped;
        public string output;
    }
}
