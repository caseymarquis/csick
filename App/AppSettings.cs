﻿using CSick.Actors;
using KC.Actin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace CSick {
    public class UserSettings {
        public UserSettings() { }

        public UserSettings(AppSettings parent) {
            FillInEmptySettings(parent);
        }

        public void FillInEmptySettings(AppSettings parent) {
            orEquals(ref LogDirPath, Path.Combine(parent.DataDirPath, "logs"));
            orEquals(ref AppUrl, Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? @"http://0.0.0.0:5000/");
            orEquals(ref CompilerPath, "gcc");
            MaxCompileSeconds = MaxCompileSeconds <= 0 ? 30 : MaxCompileSeconds;
            MaxTestRunSeconds = MaxTestRunSeconds <= 0 ? 30 : MaxTestRunSeconds;

            var solutionDir = Util.GetSolutionDirectory();
            orEqualsList(ref TestDirectories, new List<string> {
                string.IsNullOrWhiteSpace(solutionDir)? "" : "../",
            });
            orEqualsList(ref TestRootPatterns, new List<string> {
                "*.c",
            });
            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            orEqualsList(ref CompileArguments, new List<string> {
                "-g",
                "-Werror",
                "-Wall",
                "-Wextra",
                "{fileName}{fileExt}",
                "-o",
                isWindows? "./bin/{fileName}.exe" : "./bin/{fileName}",
            });

            void orEquals(ref string self, string defaultValue) {
                if (string.IsNullOrWhiteSpace(self)) {
                    self = defaultValue;
                }
            }

            void orEqualsList<T>(ref ImmutableList<T> self, IEnumerable<T> defaultValue) {
                if (self == null || self.IsEmpty) {
                    self = defaultValue.ToImmutableList();
                }
            }
        }

        public string LogDirPath;
        public string AppUrl;
        public ImmutableList<string> TestDirectories;
        public ImmutableList<string> TestRootPatterns;
        public ImmutableList<string> CompileArguments;
        public string CompilerPath;
        public int MaxCompileSeconds;
        public int MaxTestRunSeconds;

        public ImmutableList<string> GetProcessedCompileArguments(string filePath) {
            var nameExt = Path.GetFileName(filePath);
            var name = Path.GetFileNameWithoutExtension(nameExt);
            var ext = Path.GetExtension(nameExt);
            return CompileArguments.Select(x => x.Replace("{fileName}", name).Replace("{fileExt}", ext)).ToImmutableList();
        }
    }

    public class AppSettings {
        public readonly string DataDirPath = "../csick-data";
        public string ConfigPath => Path.Combine(DataDirPath, "config.json");
        public string ConfigBackupPath => Path.Combine(DataDirPath, "config.old.schema.json");
        public string GitignorePath => Path.Combine(DataDirPath, ".gitignore");
        public bool Exists => File.Exists(ConfigPath);

        private Atom<UserSettings> userSettingsAtom = new Atom<UserSettings>();
        public UserSettings UserSettings => userSettingsAtom.Value;

        public bool TryCreateOnDisk(out string error, bool force = false) {
            try {
                if (this.userSettingsAtom.Value == null) {
                    this.userSettingsAtom.Value = new UserSettings(this);
                }
                Directory.CreateDirectory(DataDirPath);
                if (force && File.Exists(ConfigPath)) {
                    File.Move(ConfigPath, ConfigBackupPath, overwrite: true);
                }
                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(this.UserSettings, Formatting.Indented));
                {
                    if (!File.Exists(GitignorePath)) {
                        File.WriteAllText(GitignorePath, "logs");
                    }
                }
                error = null;
                return true;
            }
            catch (Exception ex) {
                error = ex.Message;
                return false;
            }
        }

        public bool TryWriteToDiskIfDifferent(out string error) {
            try {
                Directory.CreateDirectory(DataDirPath);
                if (!File.Exists(ConfigPath)) {
                    error = $"File does not exist - {ConfigPath}";
                    return false;
                }
                string textOnDisk = null;
                try {
                    textOnDisk = File.ReadAllText(ConfigPath);
                }
                catch (Exception ex) {
                    error = $"Could not access file {ConfigPath} {ex.Message}";
                    return false;
                }
                var textInMemory = JsonConvert.SerializeObject(this.UserSettings, Formatting.Indented);
                if (textOnDisk != textInMemory) {
                    return TryCreateOnDisk(out error, force: true);
                }
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
                    this.userSettingsAtom.Value = JsonConvert.DeserializeObject<UserSettings>(allText);
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
