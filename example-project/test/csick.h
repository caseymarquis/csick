#ifndef _test_h_
#define _test_h_
#include <errno.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdarg.h>

typedef enum {
    tr_Success = 0,
    tr_Failed = 9091,
    tr_BadTestNumber = 9092,
    tr_AssertFailed = 9093,
} TestResult;

extern int test_exitWithMessage(TestResult exitCode, char *msg, ...) {
  va_list args;
  va_start(args, msg);
  vfprintf(stderr, msg, args);
  va_end(args);
  exit(exitCode);
  return 1;
}

extern int _test_getTestToRun(int argc, char ** argv){
    if(argc < 2){
        test_exitWithMessage(
            tr_BadTestNumber,
            "No argument was passed to the test specifying which test to run.");
    }

    int testToRun = atoi(argv[1]);
    if(testToRun <= 0){
        test_exitWithMessage(tr_BadTestNumber, "Test to run was less than or equal to 0: %d", testToRun);
    }
    return testToRun;
}

#define START_TESTS int main(int argc, char ** argv) { \
    int _testToRun = _test_getTestToRun(argc, argv); \
    int _testCounter = 0;
    char * _selectedTestName = "No Test Selected";

#define START_TEST(testName) \
    if((++_testCounter) == _testToRun){ \
        _selectedTestName = testName;

#define ASSERT(expression) \
        if(!(expression)){ \
            test_exitWithMessage(tr_AssertFailed, "Assert Failed: '%s' Line %d", _selectedTestName, __LINE__ - 2); \
        }

#define END_TEST \
        return tr_Success; \
    }

#define END_TESTS \
    test_exitWithMessage(tr_BadTestNumber, "Test to run was not found: %d", _testToRun); \
}
#endif