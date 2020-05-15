#include <stdlib.h>
#include "./buffer.h"

#define BUFFER_SUCCESS 0
#define BUFFER_OUTOFMEMORY -1
#define BUFFER_WASNULL -2
#define BUFFER_DATAWASNULL -3

extern int buffer_init(Buffer *buffer) {
  if (buffer == NULL) return BUFFER_WASNULL;

  int initialSize = 4096;
  buffer->index = 0;
  buffer->capacity = initialSize;
  buffer->data = (char *)malloc(sizeof(char) * initialSize);
  if (buffer->data == NULL) {
    return BUFFER_OUTOFMEMORY;
  }
  return BUFFER_SUCCESS;
}

extern int buffer_grow(Buffer *buffer) {
  if (buffer == NULL) return BUFFER_WASNULL;
  if (buffer->data == NULL) return BUFFER_DATAWASNULL;

  char *newMem = (char *)malloc(sizeof(char) * buffer->capacity);
  memcpy(newMem, buffer, sizeof(char) * buffer->capacity);
  free(buffer->data);
  buffer->data = newMem;
  return BUFFER_SUCCESS;
}

extern int buffer_free(Buffer *buffer) {
  if (buffer == NULL) return BUFFER_WASNULL;
  if (buffer->data == NULL) return BUFFER_DATAWASNULL;

  free(buffer->data);
  buffer->data = NULL;
  return BUFFER_SUCCESS;
}