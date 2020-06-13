using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using KC.Actin;
using System.Diagnostics;
using KC.Actin.ActorUtilNS;
using System.Runtime.InteropServices.ComTypes;
using CSick.Actors;

namespace CSick {
    public class App {

        //This is static so we can inject dependencies into
        //our web controllers.
        public static Director Director = new Director();

        public async Task Run() {
            var solutionDir = Util.GetSolutionDirectory();
            Directory.SetCurrentDirectory(solutionDir);

            var config = new AppSettings();
            await Util.WaitForThreadAsync(new TimeSpan(0, 0, 5), null, () => {
                if (!config.Exists) {
                    Console.WriteLine($"Creating initial config file: {config.ConfigPath}");
                    if (!config.TryCreateOnDisk(out var createError)) {
                        Console.WriteLine($"Failed to create initial config file: {createError}");
                        return;
                    }
                }
                if (!config.TryLoadFromDisk(out var loadError)) {
                    Console.WriteLine($"Config file could not be loaded: {loadError}");
                    return;
                }
            });
            config.UserSettings.FillInEmptySettings(config);

            Director.AddSingletonDependency(config);
            await Director.Run(configure: async directorConfig => {
                directorConfig.Set_DirectorName("Main Director");
                directorConfig.Set_AssembliesToCheckForDependencies(typeof(WebSite_Run).Assembly);
                directorConfig.Set_StandardLogOutputFolder(config.UserSettings.LogDirPath);

                Console.WriteLine("CSick Starting...");
                var sw = new Stopwatch();
                sw.Start();
                directorConfig.Run_AfterStart(async util => {
                    Console.WriteLine($"CSick started in {sw.ElapsedMilliseconds}ms.");
                    await Task.FromResult(0);
                });

                //directorConfig.Set_RuntimeLog<DebugLogger>();
                await Task.FromResult(0);
            });
        }

        public void Dispose() {
            Director.Dispose();
        }
    }

    [Singleton]
    public class DebugLogger : IActinLogger {
        public void Log(ActinLog log) {
            Console.WriteLine(log.ToString());
        }
    }
}
