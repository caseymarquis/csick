using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace CSick.Actors.ProcessRunnerNS {
    public readonly struct ProcStartInfo {
        public ProcStartInfo(string path, string workingDirectory, IEnumerable<string> arguments, TimeSpan maxRunTime, Action<ProcHandle> accept) {
            this.Id = Guid.NewGuid();
            this.Path = path;
            this.WorkingDirectory = workingDirectory;
            this.Arguments = arguments?.ToImmutableList() ?? ImmutableList.Create<string>();
            this.MaxRunTime = maxRunTime;
            this.__Accept__ = accept;
        }

        public readonly Guid Id;
        public readonly string Path;
        public readonly string WorkingDirectory;
        public readonly ImmutableList<string> Arguments;
        public readonly TimeSpan MaxRunTime;
        public readonly Action<ProcHandle> __Accept__;
    }
}
