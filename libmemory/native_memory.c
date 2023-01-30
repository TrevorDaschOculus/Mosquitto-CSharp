#include <stdio.h>
#include <stdlib.h>

#define MEMORY_IMPLEMENTATION
#include "native_memory.h"

void* native_malloc(size_t size) 
{
	return malloc(size);
}

void* native_calloc(size_t count, size_t size)
{
	return calloc(count, size);
}

void native_free(void* ptr)
{
	free(ptr);
}