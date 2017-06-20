#if NET46
using ProtoBuf;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SteamKit2.Discovery
{
    /// <summary>
    /// A server list provider that uses IsolatedStorage to persist the server list
    /// </summary>
    public class IsolatedStorageServerListProvider : IServerListProvider
    {
        private const string FileName = "serverlist.protobuf";

        IsolatedStorageFile isolatedStorage;

        /// <summary>
        /// Initialize a new instance of IsolatedStorageServerListProvider using <see cref="IsolatedStorageFile.GetUserStoreForAssembly"/>
        /// </summary>
        public IsolatedStorageServerListProvider()
        {
            isolatedStorage = IsolatedStorageFile.GetUserStoreForAssembly();
        }

        /// <summary>
        /// Read the stored list of servers from IsolatedStore
        /// </summary>
        /// <returns>List of servers if persisted, otherwise an empty list</returns>
        public Task<IEnumerable<CMServerRecord>> FetchServerListAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    using (IsolatedStorageFileStream fileStream = isolatedStorage.OpenFile(FileName, FileMode.Open, FileAccess.Read))
                    {
                        return Serializer.DeserializeItems<BasicServerListProto>(fileStream, PrefixStyle.Base128, 1)
                            .Select(item =>
                            {
                                if (item.websocket)
                                {
                                    return CMServerRecord.WebSocketServer(item.address + ":" + item.port);
                                }
                                else
                                {
                                    return CMServerRecord.SocketServer(new IPEndPoint(IPAddress.Parse(item.address), item.port));
                                }
                            })
                            .ToList();
                    }
                }
                catch (IOException ex)
                {
                    DebugLog.WriteLine("IsolatedStorageServerListProvider", "Failed to read file {0}: {1}", FileName, ex.Message);
                    return Enumerable.Empty<CMServerRecord>();
                }
            });
        }

        /// <summary>
        /// Writes the supplied list of servers to persistent storage
        /// </summary>
        /// <param name="endpoints">List of server endpoints</param>
        /// <returns>Awaitable task for write completion</returns>
        public Task UpdateServerListAsync(IEnumerable<CMServerRecord> endpoints)
        {
            return Task.Run(() =>
            {
                try
                {
                    using (IsolatedStorageFileStream fileStream = isolatedStorage.OpenFile(FileName, FileMode.Create))
                    {
                        Serializer.Serialize(fileStream,
                            endpoints.Select(ep =>
                            {
                                if (ep.ServerType == CMServerType.WebSocket)
                                {
                                    return new BasicServerListProto
                                    {
                                        address = ep.GetHostname(),
                                        port = ep.GetPort(),
                                        websocket = true
                                    };
                                }
                                else
                                {
                                    return new BasicServerListProto
                                    {
                                        address = ep.GetIPAddress().ToString(),
                                        port = ep.GetPort(),
                                        websocket = false
                                    };
                                }
                            }));
                        fileStream.SetLength(fileStream.Position);
                    }
                }
                catch (IOException ex)
                {
                    DebugLog.WriteLine("IsolatedStorageServerListProvider", "Failed to write file {0}: {1}", FileName, ex.Message);
                }
            });
        }
    }
}
#endif
