#include "../src/buffer.h"
#include "../src/buffer.c"
#include <string.h>

//Normally, you clone csick into your project test directory,
//so you would reference ./csick/csick.h
#include "../../../csick/csick.h"

START_TESTS

START_TEST("buffer_new")
    Buffer * buffer = NULL;
    int result = buffer_new(buffer);
    CS_ASSERT(result == 0);
    CS_ASSERT(buffer != NULL);
    CS_ASSERT(buffer->capacity > 0);
    CS_ASSERT(buffer->index == 0);
END_TEST

START_TEST("buffer_grow")
    Buffer * buffer = NULL;
    int result = buffer_new(buffer);
    CS_ASSERT(result == 0);
    const char * s = "This is a string";
    strcpy(buffer->data, s);
    buffer->index += strlen(s);

    int capacityWas = buffer->capacity;
    result = buffer_grow(buffer);
    CS_ASSERT(result == 0);
    CS_ASSERT(memcmp(buffer->data, s, strlen(s)));
    CS_ASSERT(buffer->capacity > capacityWas);
END_TEST

START_TEST("buffer_free")
    Buffer * buffer = NULL;
    int result = buffer_new(buffer);
    CS_ASSERT(result == 0);

    result = buffer_free(buffer);
    CS_ASSERT(result == 0);
    CS_ASSERT(buffer == NULL);
END_TEST

END_TESTS