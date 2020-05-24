//#include "../../../csick/csick.h"

START_TESTS

START_TEST("Assert Should Succeed")
    CS_ASSERT(1 == 1);
END_TEST

START_TEST("Assert Should Fail")
    CS_ASSERT(1 == 0);
END_TEST

END_TESTS