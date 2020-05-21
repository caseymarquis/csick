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
            var testResult = availableTest.TestResult;
            return new Web_CTest {
                name = test.Name,
                lineNumber = test.LineNumber,
                testNumber = test.TestNumber,
                runStatus = availableTest.RunStatus.ToString(),
                testResult = new Web_TestResult {
                    exitCode = testResult.ExitCode,
                    finished = testResult.Finished,
                    output = includeOutput? testResult.Output : null,
                    timeStarted = testResult.TimeStarted,
                    timeStopped = testResult.TimeStopped,
                    success = testResult.Success,
                },
            };
        }

        private List<Web_RootSourceFile> getRootSourceFiles(Func<CTests_AvailableTestFile, bool> predicate, bool includeOutput, int? withTestNumber) {
            var rootFileActors = testFiles.Actors_Where(predicate);
            var ret = rootFileActors.Select(x => {
                var sourceFile = x.SourceFile;
                var compileResult = x.CompileResult;
                return new Web_RootSourceFile {
                    fileName = sourceFile.FileName,
                    path = sourceFile.FilePath,
                    pathHash = sourceFile.PathHash,
                    compileStatus = x.CompileStatus.ToString(),
                    compileResult = new Web_CompileResult {
                        finished = compileResult.Finished,
                        output = includeOutput? compileResult.Output : null,
                        timeStarted = compileResult.TimeStarted,
                        timeStopped = compileResult.TimeStopped,
                        success = compileResult.Success,
                    },
                    tests = x.Actors_GetAll()
                        .Select(y =>
                            getWebTestFromTest(
                                availableTest: y,
                                includeOutput: withTestNumber != null && y.Test.TestNumber == withTestNumber))
                        .ToList(),
                };
            }).OrderBy(x => x.path).ToList();
            return ret;
        }

        [HttpGet]
        [Route("api/v1/RootSourceFile")]
        public List<Web_RootSourceFile> RootSourceFileList() {
            return getRootSourceFiles(x => true, false, null);
        }

        [HttpGet]
        [Route("api/v1/RootSourceFile/{pathHash}")]
        public Web_RootSourceFile RootSourceFileList(string pathHash) {
            return getRootSourceFiles(x => x.SourceFile.PathHash == pathHash, true, null).First();
        }

        [HttpGet]
        [Route("api/v1/RootSourceFile/{pathHash}/{testNumber}")]
        public async Task<Web_CTest> RootSourceFileList(string pathHash, int testNumber) {
            var testFile = getRootSourceFiles(x => x.SourceFile.PathHash == pathHash, false, testNumber).First();
            var test = testFile.tests.First(x => x.testNumber == testNumber);
            testFile.tests = null;
            test.parent = testFile;
            try {
                test.lines = (await System.IO.File.ReadAllLinesAsync(testFile.path))
                    .Select(x => string.IsNullOrEmpty(x)? " " : x)
                    .ToArray();
            }
            catch(Exception ex) {
                test.lines = new string[] { $"Failed to get text: {ex.Message}" };
            }
            return test;
        }
    }
}
