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
                }
            };
        }

        private List<Web_RootSourceFile> getRootSourceFiles(Func<CTests_AvailableTestFile, bool> predicate, bool includeOutput) {
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
                    tests = x.Actors_GetAll().Select(y => getWebTestFromTest(y, false)).ToList(),
                };
            }).OrderBy(x => x.path).ToList();
            return ret;
        }

        [HttpGet]
        [Route("api/v1/RootSourceFile")]
        public List<Web_RootSourceFile> RootSourceFileList() {
            return getRootSourceFiles(x => true, false);
        }

        [HttpGet]
        [Route("api/v1/RootSourceFile/{pathHash}")]
        public Web_RootSourceFile RootSourceFileList(string pathHash) {
            return getRootSourceFiles(x => x.SourceFile.PathHash == pathHash, true).First();
        }

        [HttpGet]
        [Route("api/v1/RootSourceFile/{pathHash}/{testNumber}")]
        public Web_CTest RootSourceFileList(string pathHash, int testNumber) {
            var testFile = testFiles.Actors_First(x => x.SourceFile.PathHash == pathHash);
            var test = testFile.Actors_First(x => x.Id == testNumber);
            return getWebTestFromTest(test, true);
        }
    }
}
