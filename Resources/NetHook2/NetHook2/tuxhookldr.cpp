#include <iostream>
#include <link.h>
#include <dlfcn.h>
#include <csignal>
#include <pthread.h>
#include <unistd.h>


std::string GetDirectoryFromPath(const char* path)
{
    std::string strPath(path);
    return strPath.substr(0, strPath.find_last_of("/"));
}

void* thldr_Init(void* arg)
{
    usleep(1000); // wait a second to let injector restore target state

    printf("Loader injected\n");
    Dl_info info;
    if(dladdr((void*)thldr_Init, &info))
    {
        printf("Loading main lib... ");
        std::string th2_path = GetDirectoryFromPath(info.dli_fname) + "/libNetHook2.so";
        void* handle = dlopen(th2_path.c_str(), RTLD_LAZY);
        if (!handle) {
            printf("FAIL:\n");
            printf("    %s\n", dlerror());
        }
        else
        {
            dlerror();
            void (*th2_init)();
            th2_init = (void (*)())dlsym(handle, "nh2_Init");
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
    else
    {
        printf("FAIL\n");
    }

    return 0;
}

__attribute__ ((constructor)) void injected()
{
    static pthread_t threadWorker = 0;
    pthread_create(&threadWorker, NULL, &thldr_Init, nullptr);
}
