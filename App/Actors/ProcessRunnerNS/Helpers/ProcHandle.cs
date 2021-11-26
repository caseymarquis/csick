using CSick.Actors.ProcessRunnerNS.Helpers;
using KC.Actin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSick.Actors.ProcessRunnerNS {
    /// <summary>
    /// The implementation of this class is hacky. I'm pretty sure it can be removed
    /// by turning the ProcessRunner into a scene which owns disposable subprocesses.
    /// </summary>
    public class ProcHandle {
        private object lockMe = new object();
        private ProcHandle_Internal data;

        public ProcHandle(ProcStartInfo startInfo, Guid id) {
            lock (lockMe) {
                this.data = new ProcHandle_Internal(startInfo, id);
            }
        }

        public ProcessStatus Status => ReadWithLock(x => x.Status);
        public ProcStartInfo StartInfo => ReadWithLock(x => x.StartInfo);
        public Guid Id => ReadWithLock(x => x.Id);
        public ProcResult Result => ReadWithLock(x => x.Result);

        private Process proc;
        private SemaphoreSlim lockProc = new SemaphoreSlim(1, 1);

        public void EditWithLock(Action<ProcHandle_Internal> doEdit) {
            lock (lockMe) {
                doEdit(data);
            }
        }

        public T ReadWithLock<T>(Func<ProcHandle_Internal, T> read) {
            lock (lockMe) {
                return read(data);
            }
        }

        /// <summary>
        /// Couldn't find a conclusive answer on whether Diagnostics.Process is thread safe.
        /// As such, we're cautiously locking when we access the underlying process.
        /// </summary>
        public bool TryWithProcess(TimeSpan timeout, Action<Process> withProc) {
            if (lockProc.Wait(timeout)) {
                try {
                    withProc(proc); 
                }
                finally {
                    lockProc.Release();
                }
                return true;
            }
            return false;
        }

        public void WithProcess(TimeSpan timeout, Action<Process> withProc) {
            if (!TryWithProcess(timeout, theProc => {
                withProc(theProc);
            })) {
                throw new TimeoutException("Failed to access process.");
            };
        }

        public void StartProcess(ProcessStartInfo psi) {
            //Should never actually have to lock, since this gets called before everything else:
            WithProcess(new TimeSpan(0, 0, 5), _ => {
                this.proc = Process.Start(psi);
            });      
        }

        public void Cancel() {
            lock (lockMe) {
                data.SetStatusAndResult(ProcessStatus.Canceled, new ProcResult(notDone: true));
            }
        }

        public void KillProcessWithNoLock() {
            proc?.Kill(true);
        }
    }
}
