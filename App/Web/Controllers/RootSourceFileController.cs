using CSick.Actors._CTests;
using CSick.Actors._CTests.Helpers;
using CSick.Web.Controllers.Helpers;
using CSick.Web.Models;
using KC.Actin;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace CSick.Web.Controllers {
    [InjectDependencies]
    public class RootSourceFileController : Controller {
        [Singleton] CTests_AvailableTestFiles testFiles;

        private Web_CTest getWebTestFromTest(CTests_AvailableTest availableTest, bool includeOutput) {
            var test = availableTest.Test;
            var testResult = availableTest.LastCompletedRunResult;
            return new Web_CTest {
                name = test.Name,
                lineNumber = test.LineNumber,
                testNumber = test.TestNumber,
                runStatus = availableTest.RunStatus.ToString(),
                testResult = new Web_ProcResult {
                    exitCode = testResult.ExitCode,
                    finished = testResult.Finished,
                    output = includeOutput? testResult.Output : null,
                    timeStarted = testResult.TimeStarted,
                    timeStopped = testResult.TimeStopped,
                    gracefulExit = testResult.GracefulExit,
                },
            };
        }

        private async Task<List<Web_RootSourceFile>> getRootSourceFiles(Func<CTests_AvailableTestFile, bool> predicate, bool includeOutput, bool includeFileContent, int? withTestNumber) {
            var rootFileActors = testFiles.Actors_Where(predicate);
            var ret = rootFileActors.Select(x => {
                var sourceFile = x.SourceFile;
                var compileResult = x.LastCompileResult;
                return new Web_RootSourceFile {
                    fileName = sourceFile.FileName,
                    path = sourceFile.FilePath,
                    pathHash = sourceFile.PathHash,
                    compileStatus = x.CompileStatus.ToString(),
                    compileResult = new Web_ProcResult {
                        finished = compileResult.Finished,
                        output = includeOutput? compileResult.Output : null,
                        timeStarted = compileResult.TimeStarted,
                        timeStopped = compileResult.TimeStopped,
                        gracefulExit = compileResult.GracefulExit,
                    },
                    tests = x.Actors_GetAll()
                        .Select(y =>
                            getWebTestFromTest(
                                availableTest: y,
                                includeOutput: withTestNumber != null && y.Test.TestNumber == withTestNumber))
                        .ToList(),
                };
            }).OrderBy(x => x.path).ToList();

            if (includeFileContent) {
                foreach (var result in ret) {
                    try {
                        result.lines = (await System.IO.File.ReadAllLinesAsync(result.path))
                            .Select(x => string.IsNullOrEmpty(x) ? " " : x)
                            .ToArray();
                    }
                    catch (Exception ex) {
                        result.lines = new string[] { $"Failed to get text: {ex.Message}" };
                    }
                }
            }

            return ret;
        }

        [HttpGet]
        [Route("api/v1/RootSourceFile")]
        public async Task<List<Web_RootSourceFile>> RootSourceFileList() {
            return await getRootSourceFiles(x => true,
                includeOutput: false,
                includeFileContent: false,
                withTestNumber: null);
        }

        [HttpGet]
        [Route("api/v1/RootSourceFile/{pathHash}")]
        public async Task<Web_RootSourceFile> RootSourceFileList(string pathHash) {
            return (await getRootSourceFiles(x => x.SourceFile.PathHash == pathHash,
                includeOutput: true,
                includeFileContent: true,
                withTestNumber: null))
                .First();
        }

        [HttpGet]
        [Route("api/v1/RootSourceFile/{pathHash}/{testNumber}")]
        public async Task<Web_CTest> RootSourceFileList(string pathHash, int testNumber) {
            var testFile = (await getRootSourceFiles(x => x.SourceFile.PathHash == pathHash,
                includeOutput: false,
                includeFileContent: true,
                withTestNumber: testNumber))
                .First();
            var test = testFile.tests.First(x => x.testNumber == testNumber);
            testFile.tests = null;
            test.parent = testFile;
            
            return test;
        }
    }
}
