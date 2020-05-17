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
                return new Web_RootSourceFile {
                    fileName = sourceFile.FileName,
                    path = sourceFile.FilePath,
                    compileStatus = x.CompileStatus.ToString(),
                    tests = x.Actors_GetAll().Select(y => {
                        var test = y.Test;
                        return new Web_CTest {
                            name = test.Name,
                            lineNumber = test.LineNumber,
                            testNumber = test.TestNumber,
                            runStatus = y.RunStatus.ToString(),
                        };
                    }).ToList(),
                };
            }).OrderBy(x => x.path).ToList();
            return ret;
        }
    }
}
