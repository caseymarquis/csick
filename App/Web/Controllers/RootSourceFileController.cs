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

        /// <summary>
        /// Returns a list of the existing users within the system.
        /// </summary>
        [HttpGet]
        [Route("api/v1/RootSourceFile")]
        public List<Web_RootSourceFile> RootSourceFileList(string token) {
            var rootFileActors = testFiles.Actors_GetAll();
            var ret = rootFileActors.Select(x => {
                var sourceFile = x.SourceFile;
                var compileResult = x.CompileResult;
                return new Web_RootSourceFile {
                    fileName = sourceFile.FileName,
                    path = sourceFile.FilePath,
                    compileStatus = x.CompileStatus.ToString(),
                    compileResult = new Web_CompileResult {
                        finished = compileResult.Finished,
                        output = compileResult.Output, //TODO: Consider making this null and only sending when this file is targeted.
                        timeStarted = compileResult.TimeStarted,
                        timeStopped = compileResult.TimeStopped,
                        success = compileResult.Success,
                    },
                    tests = x.Actors_GetAll().Select(y => {
                        var test = y.Test;
                        var testResult = y.TestResult;
                        return new Web_CTest {
                            name = test.Name,
                            lineNumber = test.LineNumber,
                            testNumber = test.TestNumber,
                            runStatus = y.RunStatus.ToString(),
                            testResult = new Web_TestResult {
                                exitCode = testResult.ExitCode,
                                finished = testResult.Finished,
                                output = testResult.Output, //TODO: Consider making this null and only sending when this test is targeted.
                                timeStarted = testResult.TimeStarted,
                                timeStopped = testResult.TimeStopped,
                                success = testResult.Success,
                            }
                        };
                    }).ToList(),
                };
            }).OrderBy(x => x.path).ToList();
            return ret;
        }
    }
}
