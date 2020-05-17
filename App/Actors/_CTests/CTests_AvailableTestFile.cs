using CSick.Actors._CTests.Helpers;
using KC.Actin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace CSick.Actors._CTests {
    [Instance]
    public class CTests_AvailableTestFile : Scene<CTests_AvailableTest, Role, int, Role<string>, string> {
        [FlexibleParent] CTests_AvailableTestFiles parent;

        Atom<CTestSourceFile> sourceFile = new Atom<CTestSourceFile>();
        public CTestSourceFile SourceFile => sourceFile.Value;

        private readonly Atom<CompileStatus> compileStatus = new Atom<CompileStatus>(Helpers.CompileStatus.Modified);
        public CompileStatus CompileStatus => compileStatus.Value;

        protected override async Task<IEnumerable<Role>> CastActors(ActorUtil util, Dictionary<int, CTests_AvailableTest> myActors) {
            var mySourceFile = parent.RootSourceFiles.FirstOrDefault(x => x.FilePath == this.Id);
            if (mySourceFile.FilePath == null) {
                this.Dispose();
                return null;
            }
            sourceFile.Value = mySourceFile;

            var result = mySourceFile.Tests.Select(x => new Role {
                Id = x.TestNumber,
            });
            return await Task.FromResult(result);
        }
    }
}
