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
    public class CTests_WatchTestDirectories : Actor {
        [Singleton] AppSettings settings;
        [Singleton] CTests_Parse parser;
        [Singleton] CTests_WatchReferencedFiles fileWatcher;

        protected override TimeSpan RunDelay => new TimeSpan(0, 0, 0, 0, 50);

        private Dictionary<string, DateTimeOffset> files = new Dictionary<string, DateTimeOffset>();
        private Dictionary<string, FileSystemWatcher> watchers = new Dictionary<string, FileSystemWatcher>();

        protected override async Task OnRun(ActorUtil util) {
            await Task.FromResult(0);
            var userSettings = settings.UserSettings;
            var paths = userSettings.TestDirectories;
            if (paths == null || !paths.Any()) {
                fileWatcher.AllDirectoriesToWatch.Enqueue(ImmutableList.Create<string>());        
            }
            foreach (var path in paths) {
                try {
                    if (!Directory.Exists(path)) {
                        util.Log.RealTime($"Path not found: {path}");
                        continue;
                    }
                    if (!watchers.ContainsKey(path)) {
                        var watcher = new FileSystemWatcher(path);
                        foreach (var pattern in settings.UserSettings.TestRootPatterns) {
                            watcher.Filters.Add(pattern);
                        }
                        watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.FileName;

                        void enqueue(object sender, FileSystemEventArgs e) {
                            switch (e) {
                                case RenamedEventArgs re:
                                    parser.RootSourceFileDetected.Enqueue(re.OldFullPath);
                                    parser.RootSourceFileDetected.Enqueue(re.FullPath);
                                    break;
                                case FileSystemEventArgs fe:
                                    parser.RootSourceFileDetected.Enqueue(fe.FullPath);
                                    break;
                            }
                        }
                        watcher.Changed += enqueue;
                        watcher.Created += enqueue;
                        watcher.Deleted += enqueue;
                        watcher.Renamed += enqueue;

                        watcher.EnableRaisingEvents = true;
                        watchers.Add(path, watcher);

                        //Search for the files for the first time:
                        foreach (var pattern in userSettings.TestRootPatterns) {
                            try {
                                var initialRootFiles = Directory.GetFiles(path, pattern);
                                parser.RootSourceFileDetected.EnqueueRange(initialRootFiles);
                            }
                            catch (Exception ex) {
                                util.Log.RealTime($"Failed to process root test pattern {pattern} in directory {path}", ex);
                            }
                        }
                    }
                }
                catch (Exception ex) {
                    util.Log.RealTime($"Failed to process root test directory {path}", ex);
                }
            }

            var watcherList = watchers.ToList();
            foreach (var watcher in watcherList) {
                if (!paths.Contains(watcher.Key)) {
                    watchers.Remove(watcher.Key);
                    watcher.Value.Dispose();
                }
            }
        }
    }
}
