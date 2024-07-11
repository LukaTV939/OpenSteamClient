#pragma once

#if defined(_MSC_VER)
    #define EXPORT extern "C" __declspec(dllexport)
#elif defined(__GNUC__)
    #define EXPORT extern "C" __attribute__((visibility("default")))
#else
    //  do nothing and hope for the best?
    #define EXPORT
    #pragma warning Unknown dynamic link import/export semantics.
#endif