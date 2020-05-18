using CSick.Actors._CTests.Helpers;
using KC.Actin;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CSick.Actors._CTests {
    [Singleton]
    public class CTests_Parse : Actor {
        [Singleton] CTests_WatchReferencedFiles fileWatcher;
        [Singleton] CTests_AvailableTestFiles activeTestFiles;

        protected override TimeSpan RunDelay => new TimeSpan(0, 0, 0, 0, 50);

        public MessageQueue<string> RootSourceFileDetected = new MessageQueue<string>();
        public MessageQueue<string> AnySourceFileDetectedOrChanged = new MessageQueue<string>();

        ImmutableList<CTestSourceFile> parsedRoots = ImmutableList.Create<CTestSourceFile>();

        protected override async Task OnRun(ActorUtil util) {
            HashSet<string> rootCandidates = new HashSet<string>();
            HashSet<string> changeCandidates = new HashSet<string>();
            {
                if (RootSourceFileDetected.TryDequeueAll(out var reportedRootSourceFiles)) {
                    foreach (var path in reportedRootSourceFiles) {
                        rootCandidates.Add(path);
                        changeCandidates.Add(path);
                    }
                }
                if (AnySourceFileDetectedOrChanged.TryDequeueAll(out var reportedChangedSourceFiles)) {
                    foreach (var path in reportedChangedSourceFiles) {
                        changeCandidates.Add(path);
                    }
                }
            }

            if (!(rootCandidates.Any() || changeCandidates.Any())) {
                return;
            }

            var currentParseTime = util.Now;
            var activeRootSourceFiles = this.parsedRoots;
            {
                var newRootPaths = rootCandidates.Where(candidatePath => !activeRootSourceFiles.Any(file => file.FilePath == candidatePath)).ToArray();
                foreach (var rootPath in newRootPaths) {
                    var errorMessages = new Atom<ImmutableList<string>>(ImmutableList.Create<string>());
                    var result = await CTestSourceFile.Create(rootPath, currentParseTime, errorMessages);
                    activeRootSourceFiles = activeRootSourceFiles.Add(result);
                    foreach (var message in errorMessages.Value) {
                        util.Log.Error(message);
                    }
                }
            }

            {
                var rootsCopy = activeRootSourceFiles;
                foreach (var root in rootsCopy) {
                    if (root.ParseTime == currentParseTime) {
                        //We already recompiled this round.
                        continue;
                    }
                    if (root.ReferencesPaths(changeCandidates)) {
                        var errorMessages = new Atom<ImmutableList<string>>(ImmutableList.Create<string>());
                        var result = await CTestSourceFile.Create(root.FilePath, currentParseTime, errorMessages);
                        activeRootSourceFiles = activeRootSourceFiles.Replace(root, result);
                        foreach (var message in errorMessages.Value) {
                            util.Log.Error(message);
                        }
                    }
                }
            }

            //Remove any missing root files:
            activeRootSourceFiles = activeRootSourceFiles.Where(x => x.Exists).ToImmutableList();
            this.parsedRoots = activeRootSourceFiles; //<== If anything external wants to look at these

            activeTestFiles.LatestRootFiles.Enqueue(activeRootSourceFiles);

            var allUniqueFiles = new HashSet<string>();
            foreach (var root in activeRootSourceFiles) {
                var leaves = root.Leaves();
                foreach (var leaf in leaves) {
                    foreach (var path in leaf.Lineage) {
                        allUniqueFiles.Add(path);
                    }
                }
            }

            fileWatcher.AllDirectoriesToWatch.Enqueue(allUniqueFiles.Select(x => Path.GetDirectoryName(x)).Distinct().ToImmutableList());
        }
    }
}
