# CSick

CSick is a testing framework for c. Its sort of like webpack-dev-server, but
for running tests on c files.

![Sample Testing Image](https://raw.githubusercontent.com/caseymarquis/csick/master/docs/sample.png)

## What does it do?
1. Watches all .c files in a specified directory.
2. Parses those files to find tests, and reads their project includes to trace their dependencies.
3. When a root test file or a dependency file is modified, CSick recompiles all related tests and runs them again.
4. The results of this are displayed on a local webpage.

The net result is that as you modify and change your files, test binaries are automatically recompiled to see if your new changes worked.

## Getting Started

### Download and Install
0. Install .NET core and NodeJS.
1. Create a directory called 'test' in your existing project.
2. Open a terminal in this folder.
3. `git clone --depth 1 https://github.com/caseymarquis/csick.git`
4. `cd ./csick/App/wwwdev`
4. `npm install`
5. `node ./node_modules/webpack/bin/webpack.js --progress --content-base ../wwwroot`
6. `cd ../../../`

### Run
7. `dotnet run --project ./csick/App/App.csproj`
8. Navigate to `localhost:5000`
9. Add test files in the test folder.

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

CSick was written and used for the author's personal projects. It's pretty feature sparse, and will likely only have features added as needed by the author. Pull requests would be welcome, but you're unlikely to request a feature an receive it.
