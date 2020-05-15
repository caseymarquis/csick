using CSick.Actors;
using KC.Actin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CSick {
    public class UserSettings {
        public UserSettings() { }

        public UserSettings(AppSettings parent) {
            LogDirPath = Path.Combine(parent.DataDirPath, "logs");
            AppUrl = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? @"http://0.0.0.0:5000/";

            var solutionDir = Util.GetSolutionDirectory();
            TestDirectories = new List<string> {
                string.IsNullOrWhiteSpace(solutionDir)? "" : Path.GetFullPath(Path.Combine(solutionDir, "..")),
            }.ToImmutableList();
            TestRootPatterns = new List<string> {
                "*.c",
            }.ToImmutableList();
        }

        public string LogDirPath;
        public string AppUrl;
        public ImmutableList<string> TestDirectories;
        public ImmutableList<string> TestRootPatterns;
    }

    public class AppSettings {
        public readonly string DataDirPath = Path.GetFullPath(Path.Combine(Util.GetSolutionDirectory(), "..", "csick-data"));
        public string ConfigPath => Path.Combine(DataDirPath, "config.txt");
        public bool Exists => File.Exists(ConfigPath);

        private Atom<UserSettings> userSettings = new Atom<UserSettings>();
        public UserSettings UserSettings => userSettings.Value;

        public bool TryCreateOnDisk(out string error) {
            try {
                if (this.userSettings.Value == null) {
                    this.userSettings.Value = new UserSettings(this);
                }
                Directory.CreateDirectory(DataDirPath);
                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(this.UserSettings, Formatting.Indented));
                error = null;
                return true;
            }
            catch (Exception ex) {
                error = ex.Message;
                return false;
            }
        }

        public bool TryLoadFromDisk(out string error) {
            try {
                Directory.CreateDirectory(DataDirPath);
                if (!File.Exists(ConfigPath)) {
                    error = $"File does not exist - {ConfigPath}";
                    return false;
                }
                string allText = null;
                try {
                    allText = File.ReadAllText(ConfigPath);
                }
                catch (Exception ex) {
                    error = $"Could not access file {ConfigPath} {ex.Message}";
                    return false;
                }

                try {
                    this.userSettings.Value = JsonConvert.DeserializeObject<UserSettings>(allText);
                }
                catch (Exception ex) {
                    error = $"Could not parse file {ConfigPath} {ex.Message}";
                    return false;
                }

                error = null;
                return true;
            }
            catch (Exception ex) {
                error = ex.Message;
                return false;
            }
        }
    }
}
