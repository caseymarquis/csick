using CSick.Actors._CTests.Helpers;
using KC.Actin;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace CSick.Actors._CTests {
    [Singleton]
    public class CTests_AvailableTestFiles : Scene<CTests_AvailableTestFile, Role<string>, string> {
        public readonly MessageQueue<ImmutableList<CTestSourceFile>> LatestRootFiles = new MessageQueue<ImmutableList<CTestSourceFile>>();

        private readonly Atom<ImmutableList<CTestSourceFile>> lastKnownActiveRoots = new Atom<ImmutableList<CTestSourceFile>>(ImmutableList.Create<CTestSourceFile>());

        public ImmutableList<CTestSourceFile> RootSourceFiles => lastKnownActiveRoots.Value;

        protected override async Task<IEnumerable<Role<string>>> CastActors(ActorUtil util, Dictionary<string, CTests_AvailableTestFile> myActors) {
            if (LatestRootFiles.TryDequeueAll(out var msg)) {
                this.lastKnownActiveRoots.Value = msg.Last();
            }

            //Create a child test file for each root source file:
            var result = this.lastKnownActiveRoots.Value.Select(x => new Role<string> {
                Id = x.FilePath,
            });
            return await Task.FromResult(result);
        }
    }
}
