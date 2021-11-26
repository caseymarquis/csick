# CSick

CSick is a testing framework for C. It's sort of like webpack-dev-server, but
for running tests on C files.

![Sample Testing Image](https://raw.githubusercontent.com/caseymarquis/csick/master/docs/sample.png)

## What does it do?
1. Watches all .c files in a specified directory.
2. Parses those files to find tests, and reads their includes to trace their dependencies.
3. When a root test file or a dependency file is modified, CSick recompiles all related tests and runs them again.
4. The results of this are displayed on a local webpage.

The net result is that as you modify and change your files, test binaries are automatically created and run to see if your new changes worked.

## Getting Started

### Download and Install
0. Install .NET core and NodeJS.
1. Create a directory called 'test' in your existing project.
2. Open a terminal in this folder.
3. `git clone --depth 1 https://github.com/caseymarquis/csick.git`
4. `./csick/install.bat`
4. `./run-tests.bat`
10. For example tests, see 'csick/example-project/test'
11. When csick is running, you can debug specific tests from within VS Code by selecting a test and hitting F5.
This requires a bit of configuration however. You can copy the config files from 'csick/example-project/.vscode'
to get started. It is assumed that gcc and gdb will be available in your environment.

```c
#include "./csick/csick.h"

START_TESTS

START_TEST("Assert Should Succeed")
    ASSERT(1 == 1);
END_TEST

START_TEST("Assert Should Fail")
    ASSERT(1 == 0);
END_TEST

END_TESTS
```
## Disclaimer

CSick was written and used for the author's personal projects. It's pretty feature sparse, and will likely only have features added as needed by the author. Pull requests would be welcome, but you're unlikely to request a feature and receive it.
