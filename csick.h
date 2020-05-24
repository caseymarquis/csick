#ifndef _test_h_
#define _test_h_

#include <errno.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdarg.h>

typedef enum {
    tr_Success = 0,
    tr_Failed = 65000,
    tr_BadLineNumber = 65001,
    tr_AssertFailed = 65002,
} TestResult;

extern int test_exitWithMessage(TestResult exitCode, char *msg, ...) {
  va_list args;
  va_start(args, msg);
  vfprintf(stderr, msg, args);
  va_end(args);
  exit(exitCode);
  return 1;
}

extern int _test_getLineToRun(int argc, char ** argv){
    if(argc < 2){
        test_exitWithMessage(
            tr_BadLineNumber,
            "You must pass in the line number that the test starts on.");
    }
    int lineToRun = atoi(argv[1]);
    if(lineToRun <= 0){
        test_exitWithMessage(tr_BadLineNumber, "Test line to run was less than or equal to 0: %d", lineToRun);
    }
    return lineToRun;
}

#define START_TESTS int main(int argc, char ** argv) { \
    int _lineToRun = _test_getLineToRun(argc, argv); \
    char * _selectedTestName = "No Test Selected";

#define START_TEST(testName) if(_lineToRun == __LINE__){ \
        _selectedTestName = testName;

#define CS_ASSERT(expression) if(!(expression)){ int line = __LINE__; \
            test_exitWithMessage(line, "Assert Failed: '%s' Line %d", _selectedTestName, line); \
        }

#define END_TEST \
        return tr_Success; \
    }

#define END_TESTS \
    test_exitWithMessage(tr_BadLineNumber, "No test was found on line %d.", _lineToRun); \
}

#endif //_test_h_