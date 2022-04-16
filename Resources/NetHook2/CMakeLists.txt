cmake_minimum_required(VERSION 3.15)

project(NetHook2 LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 11)
set(CMAKE_CXX_STANDARD_REQUIRED true)

# deps versions
set(PROTOBUF_VERSION "3.15.6")
set(FUNCHOOK_VERSION "1.1.0")
set(ZLIB_VERSION "1.2.12")

# steam client is still a 32bit app
if(UNIX)
    set(CMAKE_C_FLAGS -m32)
    set(CMAKE_CXX_FLAGS -m32)
elseif(WIN32)
        set(CMAKE_C_FLAGS "/MD /EHsc")
        set(CMAKE_CXX_FLAGS "/MD /EHsc")
	
	find_library(PSAPI psapi)
	if(PSAPI MATCHES "PSAPI-NOTFOUND")
		message(FATAL_ERROR "Psapi not found!")
	endif()
endif()

set(CMAKE_POSITION_INDEPENDENT_CODE true)

set(NetHook2_ROOT ${CMAKE_CURRENT_LIST_DIR})
set(NetHook2_SRC ${CMAKE_CURRENT_LIST_DIR}/NetHook2)

include(ExternalProject)
ExternalProject_Add(protobuf
    DOWNLOAD_DIR ${NetHook2_ROOT}/deps/downloads
    SOURCE_DIR ${NetHook2_ROOT}/deps/srcs/protobuf
    SOURCE_SUBDIR ./cmake
    BINARY_DIR ${NetHook2_ROOT}/deps/builds/protobuf
    INSTALL_DIR ${NetHook2_ROOT}/include/protobuf
    CMAKE_ARGS
        -DCMAKE_BUILD_TYPE=Release
        -Dprotobuf_BUILD_TESTS=false
        -Dprotobuf_BUILD_PROTOC_BINARIES=false
        -Dprotobuf_BUILD_LIBPROTOC=false
        -Dprotobuf_MSVC_STATIC_RUNTIME=false
        -DCMAKE_INSTALL_PREFIX=<INSTALL_DIR>
        -DCMAKE_C_FLAGS=${CMAKE_C_FLAGS}
        -DCMAKE_CXX_FLAGS=${CMAKE_CXX_FLAGS}
        -DCMAKE_POSITION_INDEPENDENT_CODE=true
    URL https://github.com/protocolbuffers/protobuf/releases/download/v${PROTOBUF_VERSION}/protobuf-cpp-${PROTOBUF_VERSION}.zip
    EXCLUDE_FROM_ALL true
)
if(UNIX AND NOT APPLE)
    set(protobuf_LIB ${NetHook2_ROOT}/include/protobuf/lib/libprotobuf.a)
elseif(WIN32)
    set(protobuf_LIB ${NetHook2_ROOT}/include/protobuf/lib/libprotobuf.lib)
endif()
set(protobuf_INCLUDE ${NetHook2_ROOT}/include/protobuf/include)


# funchook build seem to be messy with MSVC
set(FUNCHOOK_C_FLAGS "${CMAKE_C_FLAGS}")
if(WIN32)
        # it's supposecd to be compiled with mingw, so
        # WIN32 macro is undefined when building with MSVC
        set(FUNCHOOK_C_FLAGS " -DWIN32=1 ${FUNCHOOK_C_FLAGS}")
endif()
ExternalProject_Add(funchook
    DOWNLOAD_DIR ${NetHook2_ROOT}/deps/downloads
    SOURCE_DIR ${NetHook2_ROOT}/deps/srcs/funchook
    BINARY_DIR ${NetHook2_ROOT}/deps/builds/funchook
    INSTALL_DIR ${NetHook2_ROOT}/include/funchook
    CMAKE_ARGS
        -DCMAKE_BUILD_TYPE=Release
        -DFUNCHOOK_BUILD_STATIC=true
        -DFUNCHOOK_BUILD_TESTS=false
        -DFUNCHOOK_BUILD_SHARED=false
        -DCMAKE_INSTALL_PREFIX=<INSTALL_DIR>
        -DCMAKE_C_FLAGS="${FUNCHOOK_C_FLAGS}"
        -DCMAKE_POSITION_INDEPENDENT_CODE=true
    URL https://github.com/kubo/funchook/releases/download/v${FUNCHOOK_VERSION}/funchook-${FUNCHOOK_VERSION}.zip
    EXCLUDE_FROM_ALL true
)
# libdistorm is not copied to funchook libs during shared build install
# might just grub a copy from build artifacts ...
if(UNIX AND NOT APPLE)
    set(funchook_LIB ${NetHook2_ROOT}/include/funchook/lib/libfunchook.a)
    set(funchook_DISTORM_LIB ${NetHook2_ROOT}/deps/builds/funchook/libdistorm.a)
elseif(WIN32)
    set(funchook_LIB ${NetHook2_ROOT}/include/funchook/lib/funchook.lib)
        set(funchook_DISTORM_LIB ${NetHook2_ROOT}/deps/builds/funchook/Release/distorm.lib)
endif()
set(funchook_INCLUDE ${NetHook2_ROOT}/include/funchook/include)

string(REPLACE "." "" ZLIB_VERSION_ ${ZLIB_VERSION})
ExternalProject_Add(zlib
    DOWNLOAD_DIR ${NetHook2_ROOT}/deps/downloads
    SOURCE_DIR ${NetHook2_ROOT}/deps/srcs/zlib
    BINARY_DIR ${NetHook2_ROOT}/deps/builds/zlib
    INSTALL_DIR ${NetHook2_ROOT}/include/zlib
    CMAKE_ARGS
        -DCMAKE_BUILD_TYPE=Release
        -DBUILD_SHARED_LIBS=false
        -DCMAKE_INSTALL_PREFIX=<INSTALL_DIR>
        -DCMAKE_C_FLAGS=${CMAKE_C_FLAGS}
        -DCMAKE_POSITION_INDEPENDENT_CODE=true
    URL https://zlib.net/zlib${ZLIB_VERSION_}.zip
    EXCLUDE_FROM_ALL true
)
if(UNIX AND NOT APPLE)
    set(zlib_LIB ${NetHook2_ROOT}/include/zlib/lib/libz.a)
elseif(WIN32)
    set(zlib_LIB ${NetHook2_ROOT}/include/zlib/lib/zlibstatic.lib)
endif()
set(zlib_INCLUDE ${NetHook2_ROOT}/include/zlib/include)

add_custom_target(deps DEPENDS zlib funchook protobuf)
set_target_properties(deps PROPERTIES EXCLUDE_FROM_ALL true)

set(LIBSRC
    ${NetHook2_SRC}/nethook.cpp
    ${NetHook2_SRC}/clientmodule.h
    ${NetHook2_SRC}/clientmodule.cpp
    ${NetHook2_SRC}/utils.h
    ${NetHook2_SRC}/csimpledetour.cpp
    ${NetHook2_SRC}/csimpledetour.h
    ${NetHook2_SRC}/binaryreader.cpp
    ${NetHook2_SRC}/binaryreader.h
    ${NetHook2_SRC}/crypto.h
    ${NetHook2_SRC}/net.h
    ${NetHook2_SRC}/signscan.h
    ${NetHook2_SRC}/signscan.cpp
    ${NetHook2_SRC}/zip.cpp
    ${NetHook2_SRC}/zip.h
    ${NetHook2_SRC}/version.cpp
    ${NetHook2_SRC}/version.h
    ${NetHook2_SRC}/steammessages_base.pb.cc
    ${NetHook2_SRC}/logger.cpp
    ${NetHook2_SRC}/logger.h
)
if(UNIX AND NOT APPLE)
    list(APPEND LIBSRC
        ${NetHook2_SRC}/utils_linux_x86.cpp
        ${NetHook2_SRC}/crypto_linux_x86.cpp
        ${NetHook2_SRC}/net_linux_x86.cpp
    )
elseif(WIN32)
    list(APPEND LIBSRC
        ${NetHook2_SRC}/utils_win32.cpp
        ${NetHook2_SRC}/crypto_win32.cpp
        ${NetHook2_SRC}/net_win32.cpp
        ${NetHook2_SRC}/nh2_string.h
        ${NetHook2_SRC}/string.cpp
        ${NetHook2_SRC}/sedebug.h
        ${NetHook2_SRC}/sedebug.cpp
        ${NetHook2_SRC}/injector.cpp
    )
endif()
set_source_files_properties(${NetHook2_SRC}/version.cpp PROPERTIES GENERATED true)

add_library(NetHook2 SHARED ${LIBSRC})
set_property(TARGET NetHook2 PROPERTY POSITION_INDEPENDENT_CODE true)
target_include_directories(NetHook2 PUBLIC ${zlib_INCLUDE} ${protobuf_INCLUDE} ${funchook_INCLUDE})
target_link_libraries(NetHook2 ${CMAKE_DL_LIBS} ${zlib_LIB} ${protobuf_LIB} ${funchook_LIB} ${funchook_DISTORM_LIB})
if(WIN32)
        target_link_libraries(NetHook2 ${PSAPI})
        add_custom_command(TARGET NetHook2 PRE_BUILD COMMAND powershell.exe -ExecutionPolicy Unrestricted ${NetHook2_ROOT}/GenerateVersionInfo.ps1 -srcRoot \"${NetHook2_SRC}\")
endif()

if(UNIX AND NOT APPLE)
    add_custom_command(OUTPUT ${NetHook2_SRC}/version.cpp COMMAND ${NetHook2_ROOT}/GenerateVersionInfo.sh ${NetHook2_SRC})
    add_custom_command(TARGET NetHook2 POST_BUILD COMMAND ${CMAKE_COMMAND} -E copy ${CMAKE_CURRENT_LIST_DIR}/inject.sh ${CMAKE_CURRENT_BINARY_DIR}/inject.sh)

    # tiny payload lib
    add_library(tuxhookldr SHARED ${NetHook2_SRC}/tuxhookldr.cpp)
    set_property(TARGET tuxhookldr PROPERTY POSITION_INDEPENDENT_CODE true)
    target_link_libraries(tuxhookldr ${CMAKE_DL_LIBS})
    # custom injector
    add_executable(tuxjector  ${NetHook2_SRC}/tuxjector.cpp)
    target_link_libraries(tuxjector ${CMAKE_DL_LIBS})
endif()
