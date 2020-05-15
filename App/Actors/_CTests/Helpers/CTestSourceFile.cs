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
    public class CTestSourceFile {
        public readonly ImmutableList<string> Lineage;
        public readonly ImmutableList<CTestSourceFile> Children;

        public string FilePath => Lineage.Last();
        public string FileName => Path.GetFileName(FilePath); 
        public readonly DateTimeOffset ParseTime;

        private CTestSourceFile(string path, ImmutableList<string> lineage, DateTimeOffset parseTime, ImmutableList<CTestSourceFile> children) {
            Lineage = lineage;
            ParseTime = parseTime;
            Children = children;
        }

        public static async Task<CTestSourceFile> Create(string path, DateTimeOffset parseTime, Atom<ImmutableList<string>> errorMessages) {
            var lineage = new string[] { path }.ToImmutableList();
            path = Path.GetFullPath(path);
            return await createInternal(path, lineage, parseTime, errorMessages);
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

        static async Task<CTestSourceFile> createInternal(string path, ImmutableList<string> lineage, DateTimeOffset parseTime, Atom<ImmutableList<string>> errorMessages) {
            //Lineage should already include the current path when this function is called.
            string[] absolutePathedDependencies = Array.Empty<string>();
            try {
                if (!File.Exists(path)) {
                    errorMessages.Value = errorMessages.Value.Add(getFileNotFoundMessage(lineage));
                }
                else {
                    var directory = Path.GetDirectoryName(path);
                    var text = await File.ReadAllTextAsync(path);
                    var childPaths = GetChildPathsFromText(text);
                    absolutePathedDependencies = childPaths.Select(x => Path.GetFullPath(Path.Combine(directory, x))).Distinct().ToArray();
                }
            }
            catch(Exception ex) {
                errorMessages.Value = errorMessages.Value.Add($"Failed to read file: '{path}' with error {ex.Message}"); 
            }

            var children = ImmutableList.Create<CTestSourceFile>();
            foreach (var absolutePath in absolutePathedDependencies) {
                if (lineage.Contains(absolutePath)) {
                    continue;
                }
                children = children.Add(await createInternal(absolutePath, lineage.Add(absolutePath), parseTime, errorMessages));
            }
            return new CTestSourceFile(path, lineage, parseTime, children);
        }

        private static Regex dependenciesRegex = new Regex(@"^(\s.)*\#include\s+\""(.*)\""", RegexOptions.Compiled | RegexOptions.Multiline);
        public static string[] GetChildPathsFromText(string text) {
            var matches = dependenciesRegex.Matches(text);
            if (matches.Any()) {
                return matches.Select(x => x.Groups[2].Value).ToArray();
            }
            return Array.Empty<string>();
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
