#include <stdlib.h>
#include <string.h>
#include "./buffer.h"

#define BUFFER_SUCCESS 0
#define BUFFER_OUTOFMEMORY -1
#define BUFFER_WASNULL -2
#define BUFFER_DATAWASNULL -3

extern Buffer * buffer_new(void) {
  Buffer * buffer = malloc(sizeof(Buffer));
  if(buffer == NULL){
    return NULL;
  }

  int initialSize = 4096;
  buffer->index = 0;
  buffer->capacity = initialSize;
  buffer->data = malloc(sizeof(char) * initialSize);
  if (buffer->data == NULL) {
    free(buffer);
    return NULL;
  }
  return buffer;
}

extern int buffer_grow(Buffer *buffer) {
  if (buffer == NULL) return BUFFER_WASNULL;
  if (buffer->data == NULL) return BUFFER_DATAWASNULL;

  char *newMem = malloc(sizeof(char) * buffer->capacity * 2);
  memcpy(newMem, buffer, sizeof(char) * buffer->capacity);
  free(buffer->data);
  buffer->data = newMem;
  buffer->capacity *= 2;
  return BUFFER_SUCCESS;
}

extern int buffer_free(Buffer ** pBuffer) {
  if (pBuffer == NULL) return BUFFER_WASNULL;

  Buffer * buffer = *pBuffer;
  if (buffer == NULL) return BUFFER_WASNULL;

  if (buffer->data == NULL) return BUFFER_DATAWASNULL;

  free(buffer->data);
  buffer->data = NULL;
  free(buffer);
  *pBuffer = NULL;

  return BUFFER_SUCCESS;
}