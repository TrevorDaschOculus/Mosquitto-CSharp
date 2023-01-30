cmake_minimum_required(VERSION 2.8.12...3.20)

project(memory)

include_directories(${PROJECT_SOURCE_DIR}/include)

set(INCLUDE_FILES native_memory.h)
set(SOURCE_FILES native_memory.c)

add_library(libmemory SHARED ${SOURCE_FILES})

set_target_properties(libmemory PROPERTIES
	OUTPUT_NAME memory
	VERSION 1
	SOVERSION 1
)

install(TARGETS libmemory
	RUNTIME DESTINATION "${CMAKE_INSTALL_BINDIR}"
	ARCHIVE DESTINATION "${CMAKE_INSTALL_LIBDIR}"
	LIBRARY DESTINATION "${CMAKE_INSTALL_LIBDIR}")
