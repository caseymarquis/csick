#ifndef _buffer_h_
#define _buffer_h_

typedef struct {
    char * data;
    int index;
    int capacity;
} Buffer;

extern int buffer_new(Buffer * buffer);
extern int buffer_grow(Buffer * buffer);
extern int buffer_free(Buffer * buffer);

#endif //_buffer_h_