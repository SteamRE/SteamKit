## NetHook2

NetHook2 is a windows DLL that is injected into the address space of a running Steam.exe process in order to hook into the networking routines of the Steam client. After hooking, NetHook2 will proceed to dump any network messages sent to and received from the Steam server that the client is connected to.

These messages are dumped to file, and can be analyzed further with NetHookAnalyzer, a hex editor, or your own purpose-built tools.

## Compiling

#### Building

1. Execute `SetupDependencies.cmd` to automatically acquire and build the zlib and protobuf headers and libraries. You will need CMake for this.
2. Build `NetHook2.sln` with Visual Studio 2019.
3. Behold: a fresh new `NetHook2.dll` is born into this world. You can place this DLL wherever you like, or leave where you built it. You'll need its full file path later when injecting.

#### Updating steammessages_base

1. Download `protoc` for the same version as specified in `SetupDependencies`.
2. Run `.\protoc.exe .\steammessages_base.proto --cpp_out=build`

## Usage

NetHook is capable of self injecting and ejecting from running instances of Steam, so there's no requirement to use a separate loader such as winject.

#### Downloading pre-built binary

As SteamKit2 releases can lag behind Steam changes, the current master branch may contain fixes to NetHook that are not released yet.

You can download latest available build from [GitHub Actions here](https://github.com/SteamRE/SteamKit/actions).  
Click on the latest commit and scroll all the way down to Artifacts section.
There, you can download NetHook and NetHookAnalyzer.

#### To begin dumping network packets

1. Ensure Steam is running. Additionally, make sure you're prepared for the possibility for Steam to crash. Using NetHook2 isn't an exact science, and sometimes things break.
2. Execute the following in an _elevated_ command prompt: `rundll32 "<Path To NetHook2.dll>",Inject`

If all goes well, you should see a console window appear with output similar to the following:

```
CCrypto::SymmetricEncryptWithIV = 0x384b84c0
CCrypto::SymmetricDecrypt = 0x384b8290
pGetMessageList = 0x3843c030
pInfos = 0x38874dc0
numMessages = 502
Dumped emsg list! (502 messages)
Detoured SymmetricEncryptWithIV!
Detoured SymmetricDecrypt!
```

If instead you see a failure message or your Steam process crashes, it's possible that an update to Steam may have broken one of NetHook2's binary signature scans and they will require updating. If you're on the beta branch of Steam, try using the non-beta version and vice versa. Additionally check to see if there's any recent NetHook2 related commits on our `steamclient-beta` branch.

If nothing seems to work, feel free to file an issue or hop [on IRC](https://github.com/SteamRE/SteamKit/wiki#contact) and let us know!

Provided everything successfully injected and hooked, NetHook2 will now begin dumping every message to file. You can locate the dumps in your Steam install, under the `nethook` directory. The directories are numbered with the unix time that dumping began.

#### To stop dumping packets

Simply execute `rundll32 "<Path To NetHook2.dll>",Eject`. The console window will disappear and NetHook2 will eject itself from the running Steam instance.

#### Injecting into other executables

NetHook2 supports injecting into any other process that makes use of Steam's networking library. In particular, you can inject NetHook2 into `steamcmd.exe`, and `srcds.exe` to dump traffic from those processes.

To do so, simply provide the ID or the name of the process on the command line when injecting. Ex: `rundll32 "<Path To NetHook2.dll>",Inject 1234` or `rundll32 "<Path To NetHook2.dll>",Inject srcds.exe`. When ejecting, be sure to provide the same process ID or name in the command as well.

#### Viewing the dumped packets

Packet dumps are written to `nethook/<timestamp>` folder inside of your Steam installation.  
`<timestamp>` indicates the time NetHook was injected.

Open `NetHookAnalyzer2.exe` and then File->Open, it should automatically default to the latest folder created by NetHook.
