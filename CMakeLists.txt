cmake_minimum_required(VERSION 3.20)
project(KCD2SaveGameEditor LANGUAGES C CXX)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

# Export compile commands for IDE tooling
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

# Public headers
include_directories(
    ${CMAKE_CURRENT_SOURCE_DIR}/include
    ${CMAKE_CURRENT_SOURCE_DIR}/src/core
)

# Core Shared Library
add_library(KCD2SaveCore SHARED
    src/core/crypto/md5.h
    src/core/crypto/md5.cpp
    src/core/compression/miniz.h
    src/core/compression/miniz.c
    src/core/compression/miniz_tdef.c
    src/core/compression/miniz_tinfl.c
    src/core/compression/miniz_zip.c
    src/core/container/WhsContainer.hpp
    src/core/container/WhsContainer.cpp
    src/core/model/SaveSession.hpp
    src/core/model/SaveSession.cpp
    src/core/api/kcd2_save_api.cpp
)

target_compile_definitions(KCD2SaveCore PRIVATE KCD2_CORE_EXPORTS)

# CLI Test Tool
add_executable(kcd2_cli
    src/cli/main.cpp
)

target_link_libraries(kcd2_cli PRIVATE KCD2SaveCore)

# Set output directories
set_target_properties(KCD2SaveCore PROPERTIES
    RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin"
    LIBRARY_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin"
    ARCHIVE_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/lib"
)

set_target_properties(kcd2_cli PROPERTIES
    RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin"
)
