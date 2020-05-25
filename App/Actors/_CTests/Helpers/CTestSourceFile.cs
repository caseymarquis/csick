using KC.Actin;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CSick.Actors._CTests.Helpers {
    public readonly struct CTestSourceFile {
        public readonly ImmutableList<string> Lineage;
        public readonly ImmutableList<CTestSourceFile> Children;
        public readonly ImmutableList<CTest> Tests;

        public string FilePath => Lineage?.Last();
        public string FileName => Path.GetFileName(FilePath);
        public string PathHash => Util.GetMd5(FilePath);
        public readonly DateTimeOffset ParseTime;

        private CTestSourceFile(string path, ImmutableList<string> lineage, DateTimeOffset parseTime, ImmutableList<CTestSourceFile> children, ImmutableList<CTest> tests) {
            Lineage = lineage;
            ParseTime = parseTime;
            Children = children;
            Tests = tests;
        }

        public static async Task<CTestSourceFile> Create(string path, DateTimeOffset parseTime, Atom<ImmutableList<string>> errorMessagesAtom) {
            var lineage = new string[] { path }.ToImmutableList();
            path = Path.GetFullPath(path);
            return await createInternal(path, lineage, parseTime, errorMessagesAtom);
        }

        private static string getFileNotFoundMessage(ImmutableList<string> lineage) {
            var depth = 0;
            var sb = new StringBuilder();
            sb.AppendLine("File does not exist:");
            foreach (var lineagePath in lineage.Reverse()) {
                depth++;
                sb.Append(' ', depth*2);
                sb.Append(lineagePath);
            }
            return sb.ToString();
        }

        static async Task<CTestSourceFile> createInternal(string path, ImmutableList<string> lineage, DateTimeOffset parseTime, Atom<ImmutableList<string>> errorMessagesAtom) {
            //Lineage should already include the current path when this function is called.
            string[] absolutePathedDependencies = Array.Empty<string>();
            var tests = ImmutableList.Create<CTest>();
            try {
                if (!File.Exists(path)) {
                    errorMessagesAtom.Value = errorMessagesAtom.Value.Add(getFileNotFoundMessage(lineage));
                }
                else {
                    var directory = Path.GetDirectoryName(path);
                    var text = await File.ReadAllTextAsync(path);
                    if (lineage.Count == 1) {
                        //root file:
                        tests = GetTestsFromText(text);
                    }
                    var childPaths = GetChildPathsFromText(text);
                    absolutePathedDependencies = childPaths.Select(x => Path.GetFullPath(Path.Combine(directory, x))).Distinct().ToArray();
                }
            }
            catch(Exception ex) {
                errorMessagesAtom.Value = errorMessagesAtom.Value.Add($"Failed to read file: '{path}' with error {ex.Message}"); 
            }

            var children = ImmutableList.Create<CTestSourceFile>();
            foreach (var absolutePath in absolutePathedDependencies) {
                if (lineage.Contains(absolutePath)) {
                    continue;
                }
                children = children.Add(await createInternal(absolutePath, lineage.Add(absolutePath), parseTime, errorMessagesAtom));
            }

            return new CTestSourceFile(path, lineage, parseTime, children, tests);
        }

        private static Regex dependenciesRegex = new Regex(@"^(\s.)*\#include\s+\""(.*)\""", RegexOptions.Compiled | RegexOptions.Multiline);
        public static string[] GetChildPathsFromText(string text) {
            var matches = dependenciesRegex.Matches(text);
            if (matches.Any()) {
                return matches.Select(x => x.Groups[2].Value).ToArray();
            }
            return Array.Empty<string>();
        }

        private static Regex ctestRegex = new Regex(@"START_TEST\(\s*\""(.*)\""\s*\)", RegexOptions.Compiled | RegexOptions.Multiline);
        public static ImmutableList<CTest> GetTestsFromText(string text) {
            var result = ImmutableList.Create<CTest>();
            var testNumber = 0;

            string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            for (var lineNumber = 0; lineNumber < lines.Length; lineNumber++) {
                var line = lines[lineNumber];
                if (ctestRegex.IsMatch(line)) {
                    var match = ctestRegex.Match(line);
                    result = result.Add(new CTest(++testNumber, lineNumber + 1, match.Groups[1].Value));
                }
            }
            return result;
        }

        public bool ReferencesPaths(HashSet<string> changeCandidates) {
            return Leaves().Any(leaf => leaf.Lineage.Any(absolutePath => changeCandidates.Contains(absolutePath)));
        }

        public ImmutableList<CTestSourceFile> Leaves() {
            var leaves = ImmutableList.Create<CTestSourceFile>();
            void addIfLeaf(CTestSourceFile file) {
                if (!file.Children.Any()) {
                    leaves = leaves.Add(file);
                }
                else {
                    foreach (var child in file.Children) {
                        addIfLeaf(child);
                    }
                }
            }
            addIfLeaf(this);
            return leaves;
        }

        public bool Exists {
            get {
                try {
                    return File.Exists(FilePath);
                }
                catch {
                    //What could we possibly do?
                    return false;
                }
            }
        }

        public override string ToString() {
            return FileName;
        }
    }
}
