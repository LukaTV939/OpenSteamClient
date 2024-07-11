cmake_minimum_required(VERSION 3.25)
project(mainexe LANGUAGES CXX) 

set(CMAKE_CXX_STANDARD 20)

include("cmake/idesupport.cmake")

set(NetHostOutputPath "" CACHE STRING "")

if (BUILD_ARCH STREQUAL "x64" OR DEFINED IDE_BUILD)
    file(GLOB_RECURSE mainexe_sources CONFIGURE_DEPENDS "src/*.cpp")

    if (BUILD_PLATFORM_TARGET STREQUAL "linux")
        set(CMAKE_EXE_LINKER_FLAGS "-Wl,-export-dynamic")
    endif()

    if (BUILD_PLATFORM_TARGET STREQUAL "win")
        # This singular line bloats the exe to 12MB...
        set(CMAKE_EXE_LINKER_FLAGS "-static-libgcc -static-libstdc++ -static")

        
        # set(CMAKE_CXX_STANDARD_LIBRARIES "${CMAKE_CXX_STANDARD_LIBRARIES}") # -lwsock32 -lws2_32
        #set(CMAKE_CXX_FLAGS "-static-libgcc -static-libstdc++ -static-pthreads -Wl,-Bstatic -lpthread -Wl,-Bdynamic")
        #set(CMAKE_EXE_LINKER_FLAGS "-Wl,-Bstatic -lpthread -Wl,-Bdynamic ${CMAKE_EXE_LINKER_FLAGS}")
        SET(EXTRA_OPTIONS WIN32)
    endif()
    
    add_executable(OpenSteamClient ${EXTRA_OPTIONS} ${mainexe_sources})
    set_property(TARGET OpenSteamClient PROPERTY CMAKE_EXECUTABLE_ENABLE_EXPORTS true)
    
    if (BUILD_PLATFORM_TARGET STREQUAL "linux")
        execute_process(COMMAND objdump -s -j .valvesig ${CMAKE_BINARY_DIR}/CMakeFiles/OpenSteamClient.dir/src/main.cpp.o RESULT_VARIABLE ret)
        if(ret EQUAL "1")
            # Segment doesn't exist
            add_custom_command(
                TARGET OpenSteamClient 
                PRE_LINK
                COMMAND objcopy --add-section .valvesig=${PROJECT_SOURCE_DIR}/dummysig --set-section-flags .valvesig=readonly ${CMAKE_BINARY_DIR}/CMakeFiles/OpenSteamClient.dir/src/main.cpp.o ${CMAKE_BINARY_DIR}/CMakeFiles/OpenSteamClient.dir/src/main.cpp.o)
        endif()
    endif()

    if (BUILD_PLATFORM_TARGET STREQUAL "win")
        target_link_options(OpenSteamClient PRIVATE -municode)
    endif()

    set(LNH_BASE_DIR ${NetHostOutputPath}/${BUILD_PLATFORM_TARGET}-${BUILD_ARCH})
    if (BUILD_PLATFORM_TARGET STREQUAL "win")
        # Can't statically link libnethost.lib as it's built for VS, not MingW. Do this trickery to get the .dll how we want it instead.
        configure_file(${LNH_BASE_DIR}/nethost.dll ${CMAKE_RUNTIME_OUTPUT_DIRECTORY}/nethost.dll COPYONLY)
        set(LIBRARIES_TO_LINK ${LNH_BASE_DIR}/nethost.dll)
        # set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -D__MSVCRT_VERSION__=0x900")
        
        # Set resource file to avoid "POSIX WinThreads for Windows"
        target_sources(OpenSteamClient PRIVATE ${PROJECT_SOURCE_DIR}/OpenSteamClient.rc)
    else()
        set(LIBRARIES_TO_LINK ${LNH_BASE_DIR}/libnethost.a)
    endif()
  
    target_include_directories(OpenSteamClient PUBLIC ${PROJECT_SOURCE_DIR}/include)
    target_link_libraries(OpenSteamClient ${LIBRARIES_TO_LINK})
endif()

if (NOT DEFINED IDE_BUILD)
    # Save build timestamp to file
    string(TIMESTAMP UNIX_TIMESTAMP "%s")
    file(WRITE ${NATIVE_OUTPUT_FOLDER}/../build_timestamp ${UNIX_TIMESTAMP})
endif()
