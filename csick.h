#ifndef _test_h_
#define _test_h_

#include <errno.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdarg.h>
#include <signal.h>
#include <assert.h>

typedef enum {
    tr_Success = 0,
    tr_Failed = 65000,
    tr_BadLineNumber = 65001,
    tr_AssertFailed = 65002,
} TestResult;

extern void _test_exitWithMessage(TestResult exitCode, char *msg, ...) {
  va_list args;
  va_start(args, msg);
  vfprintf(stderr, msg, args);
  va_end(args);
  exit(exitCode);
}

extern int _test_getLineToRun(int argc, char ** argv){
    if(argc < 2){
        _test_exitWithMessage(
            tr_BadLineNumber,
            "You must pass in the line number that the test starts on.");
    }
    int lineToRun = atoi(argv[1]);
    if(lineToRun <= 0){
        _test_exitWithMessage(tr_BadLineNumber, "Test line to run was less than or equal to 0: %d", lineToRun);
    }
    return lineToRun;
}

extern int _test_hasDebugger(int argc, char ** argv){
    const char expectedArg[] = "--debug";
    for(int i = 0; i < argc; i++){
        if(strcmp(expectedArg, argv[i]) == 0){
            return 1;
        }
    }
    return 0;
}

extern int _test_getZero(){
    //Helps avoid a warning:
    return 0;
}

extern int _test_useIntVariable(int i){
    return i;
}

extern char * _test_useCharPointer(char * c){
    return c;
}

#define START_TESTS int main(int argc, char ** argv) { \
    int _lineToRun = _test_getLineToRun(argc, argv); \
    int _hasDebugger = _test_hasDebugger(argc, argv); \
    _test_useIntVariable(_hasDebugger); \
    char * _selectedTestName = "No Test Selected"; \
    _test_useCharPointer(_selectedTestName);

#define START_TEST(testName) do { if(_lineToRun == __LINE__){ \
        _selectedTestName = testName;

#define CS_ASSERT(expression) do { if(!(expression)){ int line = __LINE__; \
            if(_hasDebugger){ _test_useIntVariable(1 / _test_getZero());  } \
            _test_exitWithMessage(line, "Assert Failed: '%s' Line %d", _selectedTestName, line); \
        } } while(0);

#define END_TEST \
        return tr_Success; \
    } } while(0);

#define END_TESTS \
    _test_exitWithMessage(tr_BadLineNumber, "No test was found on line %d.", _lineToRun); \
}

#endif //_test_h_