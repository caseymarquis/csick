using KC.Actin;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CSick.Actors._CTests {
    [Singleton]
    public class CTests_WatchReferencedFiles : Actor {
        [Singleton] CTests_ParseDependencies parseDependencies;

        public MessageQueue<ImmutableList<string>> AllDirectoriesToWatch = new MessageQueue<ImmutableList<string>>();

        Dictionary<string, FileSystemWatcher> fileWatchers = new Dictionary<string, FileSystemWatcher>();
        protected override async Task OnRun(ActorUtil util) {
            if (!AllDirectoriesToWatch.TryDequeueAll(out var messages)) {
                return;
            }
            var newPathsToWatch = messages.Last(); //Last message wins. 

            var toDispose = fileWatchers.Where(pair => !newPathsToWatch.Contains(pair.Key)).ToList();

            try {
                var toAdd = newPathsToWatch.Where(newPath => !fileWatchers.ContainsKey(newPath)).ToList();
                foreach (var pathToAdd in toAdd) {
                    if (!File.Exists(pathToAdd)) {
                        continue;
                    }
                    var watcher = new FileSystemWatcher(pathToAdd);
                    watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.FileName;

                    void enqueue(object sender, FileSystemEventArgs e) {
                        switch (e) {
                            case RenamedEventArgs re:
                                parseDependencies.AnySourceFileDetectedOrChanged.Enqueue(re.FullPath);
                                parseDependencies.AnySourceFileDetectedOrChanged.Enqueue(re.OldFullPath);
                                break;
                            case FileSystemEventArgs fe:
                                parseDependencies.AnySourceFileDetectedOrChanged.Enqueue(fe.FullPath);
                                break;
                        }
                    }
                    watcher.Changed += enqueue;
                    watcher.Created += enqueue;
                    watcher.Deleted += enqueue;
                    watcher.Renamed += enqueue;

                    fileWatchers[pathToAdd] = watcher;
                    watcher.EnableRaisingEvents = true;
                }
            }
            finally {
                foreach (var d in toDispose) {
                    try { d.Value.Dispose(); }
                    catch { }
                }
            }
            await Task.FromResult(0);
        }
    }
}
