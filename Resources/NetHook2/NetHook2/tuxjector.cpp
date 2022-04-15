//
// somewhat universal 32bit Linux ptrace based lib injector
//

#include <dlfcn.h>
#include <link.h>
#include <stdio.h>
#include <stdlib.h>
#include <cstring>
#include <sys/ptrace.h>
#include <sys/wait.h>
#include <sys/user.h>
#include <limits>
#include <errno.h>
#include <fstream>
#include <memory>

//50        push    eax ; dlopen() mode
//53        push    ebx ; lib path
//FF D2     call    edx ; targets dlopen() address
//CC        int3        ;
const uint8_t payload[] = { 0x50, 0x53, 0xFF, 0xD2, 0xCC };

// a helper function for writing data into target process mem map
// using PTRACE_POKETEXT call word by word padded with 0x00
// will return offsetBase advanced by len + pad
// will return -1 if PTRACE_POKETEXT failed
size_t PtraceWriteData(int pid, size_t offsetBase, void *data, size_t len)
{
    for(int i = 0; i < len; )
    {
        size_t value = 0;
        size_t dataLen = len - i > sizeof(size_t) ? sizeof(size_t) : len - i;
        std::memcpy((char*)&value, (char*)data+i, dataLen);
        if(ptrace(PTRACE_POKETEXT, pid, offsetBase, value) == -1)
        {
            return -1;
        }
        offsetBase += sizeof(size_t);
        i += dataLen;
    }
    return offsetBase;
}

// a helper function for reading data from target process mem map
// word by word using PTRACE_PEEKTEXT call
// will return false if PTRACE_PEEKTEXT failed
// data size must be multiple of sizeof(size_t)
bool PtraceReadData(int pid, size_t offsetBase, void* data, size_t len)
{
    if(len % sizeof(size_t))
    {
        return false;
    }

    for(size_t i = 0; i < len;)
    {
        long out = ptrace(PTRACE_PEEKTEXT, pid, offsetBase + i, 0);
        if(out == -1 && !errno)
        {
            return false;
        }
        std::memcpy((char*)data + i, (char*)&out, sizeof(size_t));
        i += sizeof(size_t);
    }

    return true;
}

static int dl_iter_cb(struct dl_phdr_info *info, size_t size, void *data)
{
    size_t *base = (size_t*)data;
    if(std::string(info->dlpi_name).find("libdl") != std::string::npos)
    {
        *base = info->dlpi_addr;
    }
    return 0;
}

int main(int argc, char* argv[])
{
    if(argc < 3)
    {
        printf("Usage: %s LIB_PATH PID\n", argv[0]);
        return 0;
    }

    std::string libPath(argv[1]);
    std::string path  = "/proc/";
                path += argv[2];
                path += "/maps";
    int pid = std::atoi(argv[2]);

    size_t libdlBaseTarget = -1;
    size_t targetExecMapBase = -1;

    std::ifstream ifs(path, std::ios_base::in);
    if(!ifs.good())
    {
        printf("Could not read target maps. Wrong PID?\n");
        return -1;
    }

    std::string line;
    while(std::getline(ifs, line))
    {
        size_t base, end;
        char flags[4] = {0};
        sscanf(line.c_str(), "%x-%x %c%c%c%c", &base, &end, &flags[0], &flags[1], &flags[2], &flags[3]);

        if(targetExecMapBase == -1)
        {
           if(end - base > sizeof(payload) + 255)
           {
              targetExecMapBase = base;
           }
        }

        if(line.find("libdl") != std::string::npos)
        {
           libdlBaseTarget = base;
           break;
        }
    }

    if(targetExecMapBase == -1)
    {
        printf("No map suitable for payload injection found in target process\n");
        return -1;
    }
    printf("Found map suitable for payload @ 0x%x\n", targetExecMapBase);

    void* libdl = (link_map*)dlopen("libdl.so", RTLD_LAZY);
    size_t dlopenAddr = (size_t)dlsym(libdl, "dlopen");
    size_t libdlBase = -1;
    dl_iterate_phdr(dl_iter_cb, &libdlBase);
    if(libdlBase == -1)
    {
        printf("Could not get libdl base addess\n");
        return -1;
    }
    size_t dlopenOffset = dlopenAddr - libdlBase;
    printf("dlopen() offset 0x%x\n", dlopenOffset);

    if(libdlBaseTarget == -1)
    {
        printf("libdl not found in target process!\n");
        return -1;
    }
    printf("Found libdl in target process @ 0x%x\n", libdlBaseTarget);

    if(ptrace(PTRACE_ATTACH, pid, nullptr, nullptr) == -1)
    {
        perror("Could not attach to target");
        return -1;
    }
    printf("Attached to target process\n");

    int status = 0;
    user_regs_struct regs, regsOld;
    size_t libPathSz = (libPath.size() + 1) % sizeof(size_t) == 0 ?
        libPath.size() + 1 : ((libPath.size() + 1) / sizeof(size_t) + 1) * sizeof(size_t);
    size_t payloadSz = sizeof(payload) % sizeof(size_t) == 0 ?
        sizeof(payload) : (sizeof(payload) / sizeof(size_t) + 1) * sizeof(size_t);

    size_t scSize = libPathSz + payloadSz;
    std::unique_ptr<char[]> textBackup(new char[scSize]);

    if(waitpid(pid, &status, 0) == -1)
    {
        perror("waitpid() failed\n");
        return -1;
    }

    if(WIFSTOPPED(status) && WSTOPSIG(status) == SIGSTOP )
    {
        printf("Got SIGSTOP from target\n");

        if(PtraceReadData(pid, targetExecMapBase, textBackup.get(), scSize))
        {
            printf("Backed up target data\n");
        }
        else
        {
            printf("Could not backup target!\n");
            return -1;
        }

        if(ptrace(PTRACE_GETREGS, pid, nullptr, &regs) != -1)
        {
            std::memcpy(&regsOld, &regs, sizeof(regs));
            printf("Saved target regs\n");

            size_t plBase = PtraceWriteData(pid,targetExecMapBase, (void*)libPath.c_str(), libPath.size()+1);
            if(plBase != -1)
            {
                printf("Wrote lib path into process memory\n");
                if(PtraceWriteData(pid, plBase, (void*)payload, sizeof(payload)) != -1)
                {
                    printf("Wrote payload\n");
                }
            }

            regs.eip = plBase;
            regs.eax = RTLD_LAZY;
            regs.ebx = targetExecMapBase;
            regs.edx = libdlBaseTarget + dlopenOffset;

            if(ptrace(PTRACE_SETREGS, pid, nullptr, &regs) != -1)
            {
                printf("Set target registers\n");
            }

            if(ptrace(PTRACE_CONT, pid, nullptr, nullptr) != -1)
            {
                printf("Resumed target\n");
            }
        }
    }

    // wait for dlopen() to return and hit our breakpoint
    if(waitpid(pid, &status, 0) == -1)
    {
        perror("waitpid() failed\n");
        return -1;
    }

    // restore target state if our breakpoint triggered
    // or KILL the target if we get any other signal
    if(WIFSTOPPED(status) && WSTOPSIG(status) == SIGTRAP)
    {
        printf("Got SIGTRAP from payload\n");

        ptrace(PTRACE_GETREGS, pid, nullptr, &regs);
        // check if dlopen call result is not null
        if(regs.eax != 0)
        {
            printf("Library injected\n");
        }

        // restore target state and resume
        if(PtraceWriteData(pid, targetExecMapBase, textBackup.get(), scSize))
        {
            printf("Restored target data\n");
        }
        if(ptrace(PTRACE_SETREGS, pid, nullptr, &regsOld) != -1)
        {
            printf("Resotred old regs\n");
        }
        if(ptrace(PTRACE_CONT, pid, nullptr, nullptr) != -1)
        {
            printf("Resumed target\n");
        }
    }
    else
    {
        printf("Got unexpected signal from target ( %d )\n", status);
        printf("Terminating process\n");
        // "This operation is deprecated; do not use it!" says ptrace(2)
        // will use it anyway...
        // If we're here target state is corrupted and most likely
        // will crash if we resume it
        ptrace(PTRACE_KILL, pid, nullptr, nullptr);
    }

    ptrace(PTRACE_DETACH, pid, nullptr, nullptr);

    return 0;

}

