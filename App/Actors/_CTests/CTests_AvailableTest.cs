using CSick.Actors._CTests.Helpers;
using KC.Actin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace CSick.Actors._CTests {
    [Instance]
    public class CTests_AvailableTest : Actor {
        [FlexibleParent] CTests_AvailableTestFile parentFile;

        protected override TimeSpan RunDelay => new TimeSpan(0, 0, 0, 0, 50);

        public readonly MessageQueue<CTestCommand> Commands = new MessageQueue<CTestCommand>();

        public CTest Test => parentFile.SourceFile.Tests.FirstOrDefault(x => x.TestNumber == this.Id);

        private readonly Atom<RunStatus> runStatus = new Atom<RunStatus>(RunStatus.NotRun);
        public RunStatus RunStatus => runStatus.Value;

        protected override async Task OnRun(ActorUtil util) {
            if (Commands.Any(x => x == CTestCommand.Cancel)) {
                //TODO: Cancel everything
                Commands.DequeueAll();
            }
            await Task.FromResult(0);
        }
    }
}
