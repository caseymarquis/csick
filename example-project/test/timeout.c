#include "../../../csick/csick.h"
#ifdef _WIN32
#include <windows.h>
#else
#include <unistd.h>
#define Sleep(x) usleep((x)*1000)
#endif

START_TESTS

START_TEST("Should Time Out")
    Sleep(100000);
    CS_ASSERT(1 == 1);
END_TEST

END_TESTS