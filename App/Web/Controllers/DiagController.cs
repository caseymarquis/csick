using CSick.Actors._CTests;
using CSick.Web.Controllers.Helpers;
using KC.Actin;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CSick.Web.Controllers {
    [InjectDependencies]
    public class DiagController : Controller {
        [Singleton] AppSettings settings;

        [HttpGet]
        [Route("api/v1/Diag/Directories")]
        public async Task<ImmutableList<string>> GetDirectoryPaths() {
            var ret = settings.UserSettings.TestDirectories
                .Select(x => {
                    try {
                        return Path.GetFullPath(x);
                    }
                    catch {
                        return $"Invalid Path: {x}";
                    };
                }).ToImmutableList();
            return await Task.FromResult(ret);
        }
    }
}
