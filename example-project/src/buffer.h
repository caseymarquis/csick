#ifndef _buffer_h_
#define _buffer_h_

typedef struct {
    char * data;
    int index;
    int capacity;
} Buffer;

extern Buffer * buffer_new(void);
extern int buffer_grow(Buffer * buffer);
extern int buffer_free(Buffer ** buffer);

#endif //_buffer_h_