cmake_minimum_required(VERSION 3.15)

project(tuxhook2 LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 11)
set(CMAKE_CXX_STANDARD_REQUIRED true)

# steam client is still a 32bit app
if(UNIX)
    set(CMAKE_C_FLAGS -m32)
    set(CMAKE_CXX_FLAGS -m32)
elseif(WIN32)
	set(CMAKE_C_FLAGS "/MD")
	set(CMAKE_CXX_FLAGS "/MD")
	
	find_library(PSAPI psapi)
	if(PSAPI MATCHES "PSAPI-NOTFOUND")
		message(FATAL_ERROR "Psapi not found!")
	endif()
endif()

set(CMAKE_POSITION_INDEPENDENT_CODE true)

set(PROTOBUF_VERSION 3.15.6)
set(FUNCHOOK_VERSION 1.1.0)

set(TuxHook_ROOT ${CMAKE_CURRENT_LIST_DIR})
set(TuxHook_SRC ${CMAKE_CURRENT_LIST_DIR}/TuxHook)

include(ExternalProject)
ExternalProject_Add(protobuf
    DOWNLOAD_DIR ${TuxHook_ROOT}/deps/downloads
    SOURCE_DIR ${TuxHook_ROOT}/deps/srcs/protobuf
    SOURCE_SUBDIR ./cmake
    BINARY_DIR ${TuxHook_ROOT}/deps/builds/protobuf
    INSTALL_DIR ${TuxHook_ROOT}/include/protobuf
    CMAKE_ARGS
        -DCMAKE_BUILD_TYPE=Release
        -Dprotobuf_BUILD_TESTS=false
        -Dprotobuf_MSVC_STATIC_RUNTIME=false
        -DCMAKE_INSTALL_PREFIX=<INSTALL_DIR>
        -DCMAKE_C_FLAGS=${CMAKE_C_FLAGS}
        -DCMAKE_CXX_FLAGS=${CMAKE_CXX_FLAGS}
        -DCMAKE_POSITION_INDEPENDENT_CODE=true
    URL https://github.com/protocolbuffers/protobuf/releases/download/v${PROTOBUF_VERSION}/protobuf-cpp-${PROTOBUF_VERSION}.zip
    EXCLUDE_FROM_ALL true
)
if(UNIX AND NOT APPLE)
    set(protobuf_LIB ${TuxHook_ROOT}/include/protobuf/lib/libprotobuf.a)
elseif(WIN32)
    set(protobuf_LIB ${TuxHook_ROOT}/include/protobuf/lib/libprotobuf.lib)
endif()
set(protobuf_INCLUDE ${TuxHook_ROOT}/include/protobuf/include)


# funchook build seem to be messy with MSVC
set(FUNCHOOK_C_FLAGS "${CMAKE_C_FLAGS}")
if(WIN32)
        # somebody forgot to use standard defined _WIN32 flag?
        # fine can also define WIN32
        set(FUNCHOOK_C_FLAGS " -DWIN32=1 ${FUNCHOOK_C_FLAGS}")
endif()
ExternalProject_Add(funchook
    DOWNLOAD_DIR ${TuxHook_ROOT}/deps/downloads
    SOURCE_DIR ${TuxHook_ROOT}/deps/srcs/funchook
    BINARY_DIR ${TuxHook_ROOT}/deps/builds/funchook
    INSTALL_DIR ${TuxHook_ROOT}/include/funchook
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
    set(funchook_LIB ${TuxHook_ROOT}/include/funchook/lib/libfunchook.a)
    set(funchook_DISTORM_LIB ${TuxHook_ROOT}/deps/builds/funchook/libdistorm.a)
elseif(WIN32)
    set(funchook_LIB ${TuxHook_ROOT}/include/funchook/lib/funchook.lib)
	set(funchook_DISTORM_LIB ${TuxHook_ROOT}/deps/builds/funchook/Release/distorm.lib)
endif()
set(funchook_INCLUDE ${TuxHook_ROOT}/include/funchook/include)

ExternalProject_Add(zlib
    DOWNLOAD_DIR ${TuxHook_ROOT}/deps/downloads
    SOURCE_DIR ${TuxHook_ROOT}/deps/srcs/zlib
    BINARY_DIR ${TuxHook_ROOT}/deps/builds/zlib
    INSTALL_DIR ${TuxHook_ROOT}/include/zlib
    CMAKE_ARGS
        -DCMAKE_BUILD_TYPE=Release
        -DBUILD_SHARED_LIBS=false
        -DCMAKE_INSTALL_PREFIX=<INSTALL_DIR>
        -DCMAKE_C_FLAGS=${CMAKE_C_FLAGS}
        -DCMAKE_POSITION_INDEPENDENT_CODE=true
    URL http://zlib.net/zlib1212.zip
    EXCLUDE_FROM_ALL true
)
if(UNIX AND NOT APPLE)
    set(zlib_LIB ${TuxHook_ROOT}/include/zlib/lib/libz.a)
elseif(WIN32)
    set(zlib_LIB ${TuxHook_ROOT}/include/zlib/lib/zlibstatic.lib)
endif()
set(zlib_INCLUDE ${TuxHook_ROOT}/include/zlib/include)

add_custom_target(deps DEPENDS zlib funchook protobuf)
set_target_properties(deps PROPERTIES EXCLUDE_FROM_ALL true)

set(LIBSRC
    ${TuxHook_SRC}/tuxhook.cpp
    ${TuxHook_SRC}/clientmodule.h
    ${TuxHook_SRC}/clientmodule.cpp
    ${TuxHook_SRC}/utils.h
    ${TuxHook_SRC}/csimpledetour.cpp
    ${TuxHook_SRC}/csimpledetour.h
    ${TuxHook_SRC}/binaryreader.cpp
    ${TuxHook_SRC}/binaryreader.h
    ${TuxHook_SRC}/crypto.h
    ${TuxHook_SRC}/net.h
    ${TuxHook_SRC}/signscan.h
    ${TuxHook_SRC}/signscan.cpp
    ${TuxHook_SRC}/zip.cpp
    ${TuxHook_SRC}/zip.h
    ${TuxHook_SRC}/version.h
    ${TuxHook_SRC}/steammessages_base.pb.cc
    ${TuxHook_SRC}/logger.cpp
    ${TuxHook_SRC}/logger.h
)
if(UNIX AND NOT APPLE)
    list(APPEND LIBSRC
        ${TuxHook_SRC}/utils.cpp
        ${TuxHook_SRC}/crypto.cpp
        ${TuxHook_SRC}/net.cpp
    )
elseif(WIN32)
    list(APPEND LIBSRC
		${TuxHook_SRC}/nh2_string.h
		${TuxHook_SRC}/string.cpp
	    ${TuxHook_SRC}/sedebug.h
		${TuxHook_SRC}/sedebug.cpp
        ${TuxHook_SRC}/utils_win32.cpp
        ${TuxHook_SRC}/crypto_win32.cpp
        ${TuxHook_SRC}/net_win32.cpp
		${TuxHook_SRC}/injector.cpp
    )
endif()

add_library(tuxhook2 SHARED ${LIBSRC})
set_property(TARGET tuxhook2 PROPERTY POSITION_INDEPENDENT_CODE true)
target_include_directories(tuxhook2 PUBLIC ${zlib_INCLUDE} ${protobuf_INCLUDE} ${funchook_INCLUDE})
target_link_libraries(tuxhook2 ${CMAKE_DL_LIBS} ${zlib_LIB} ${protobuf_LIB} ${funchook_LIB} ${funchook_DISTORM_LIB})
if(WIN32)
	target_link_libraries(tuxhook2 ${PSAPI})
endif()

if(UNIX AND NOT APPLE)
    add_custom_command(TARGET tuxhook2 POST_BUILD COMMAND ${CMAKE_COMMAND} -E copy ${CMAKE_CURRENT_LIST_DIR}/inject.sh ${CMAKE_CURRENT_BINARY_DIR}/inject.sh)
    # tiny payload lib
    add_library(tuxhookldr SHARED ${CMAKE_CURRENT_LIST_DIR}/TuxHook/tuxhookldr.cpp)
    set_property(TARGET tuxhookldr PROPERTY POSITION_INDEPENDENT_CODE true)
    target_link_libraries(tuxhookldr ${CMAKE_DL_LIBS})
endif()
