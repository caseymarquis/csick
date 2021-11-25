using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CSick.Utility {
    public class Install {
        public async Task Run() {
            Console.WriteLine($"Working Dir: {new DirectoryInfo("./").FullName}");
            var result = await Util.RunProcess("./npm-install.bat", "", ".");
            Console.WriteLine("Running npm install...");
            if (!result.Success) {
                Console.WriteLine(result.ToString());
                throw new ApplicationException("Unable to run 'npm install'.");
            }
            Console.WriteLine("Compiling site with webpack...");
            result = await Util.RunProcess("node",
                "./node_modules/webpack/bin/webpack.js --colors --display-error-details --content-base ../wwwroot",
                "./App/wwwdev");
            if (!result.Success) {
                Console.WriteLine(result.ToString());
                throw new ApplicationException("Unable to run './compile.bat'.");
            }
            copy("./example.gitignore", "../.gitignore");
            copy("./example.run-tests.bat", "../run-tests.bat");

            Console.Clear();
            Console.WriteLine("Success. You may now start csick by running 'run-tests.bat' in your test directory. Press any key to exit.");
            Console.ReadKey();

            void copy(string from, string to) {
                if (new FileInfo(to).Exists) {
                    return; 
                }
                File.Copy(from, to);
            }
        }
    }
}
