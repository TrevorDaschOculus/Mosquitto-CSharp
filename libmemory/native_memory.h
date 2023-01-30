#ifndef NATIVE_MEMORY_H
#define NATIVE_MEMORY_H

#include <stdint.h>

#if defined(WIN32)
#  ifdef MEMORY_IMPLEMENTATION
#    define MEMORY_API __declspec(dllexport)
#  else
#    define MEMORY_API __declspec(dllimport)
#  endif
#else
#  define MEMORY_API extern
#endif

MEMORY_API void* native_malloc(size_t size);

MEMORY_API void* native_calloc(size_t count, size_t size);

MEMORY_API void native_free(void* ptr);

#endif