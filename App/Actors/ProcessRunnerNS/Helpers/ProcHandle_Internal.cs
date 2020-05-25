using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSick.Actors.ProcessRunnerNS.Helpers {
    /// <summary>
    /// This class is shared across threads.
    /// you must lock on all reads/writes.
    /// Another layer of indirection could force this, but it's not worth the hassle,
    /// since this class is already hidden from normal use inside of ProcHandle.
    /// </summary>
    public class ProcHandle_Internal {

        public ProcHandle_Internal(ProcStartInfo procStartInfo, Guid id) {
            this.StartInfo = procStartInfo;
            this.Id = id;
        }

        public readonly Guid Id;
        public readonly ProcStartInfo StartInfo;
        public ProcessStatus Status;
        public ProcResult Result;
        public StringBuilder StdOut = new StringBuilder();
        public StringBuilder StdErr = new StringBuilder();
        public Thread ProcessMonitorThread;

        public void SetStatusAndResult(ProcessStatus status, ProcResult result) {
            if (this.Status != ProcessStatus.Finished) {
                this.Status = status;
                this.Result = result;
            }
        }
    }
}
