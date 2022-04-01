#include <iostream>
#include <link.h>
#include <dlfcn.h>

std::string GetDirectoryFromPath(const char* path)
{
    std::string strPath(path);
    size_t delim = strPath.find_last_of("/");
    return strPath.substr(0, delim);
}

void thldr_Init()
{
    Dl_info info;
    if(dladdr((void*)thldr_Init, &info))
    {
        printf("Loading main lib... ");
        std::string th2_path = GetDirectoryFromPath(info.dli_fname) + "/libtuxhook2.so";
        void* handle = dlopen(th2_path.c_str(), RTLD_LAZY);
        if (!handle) {
            printf("FAIL:\n");
            printf("    %s\n", dlerror());
        }
        else
        {
            dlerror();
            void (*th2_init)();
            th2_init = (void (*)())dlsym(handle, "th2_Init");
            if(!th2_init)
            {
                printf("FAIL:\n");
                printf("    %s\n", dlerror());
            }
            else
            {
                printf("OK\n");
                th2_init();
            }
        }
    }
}

__attribute__ ((constructor)) void injected()
{
    std::printf("Loader injected\n");
    thldr_Init();
}
