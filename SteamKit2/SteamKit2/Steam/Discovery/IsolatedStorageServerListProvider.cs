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
        public Task<IEnumerable<IPEndPoint>> FetchServerListAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    using (IsolatedStorageFileStream fileStream = isolatedStorage.OpenFile(FileName, FileMode.Open, FileAccess.Read))
                    {
                        return ProtoBuf.Serializer.DeserializeItems<BasicServerListProto>(fileStream, PrefixStyle.Base128, 1)
                            .Select(item => new IPEndPoint(IPAddress.Parse(item.ipAddress), item.port))
                            .ToList();
                    }
                }
                catch (IOException ex)
                {
                    DebugLog.WriteLine("IsolatedStorageServerListProvider", "Failed to read file {0}: {1}", FileName, ex.Message);
                    return Enumerable.Empty<IPEndPoint>();
                }
            });
        }

        /// <summary>
        /// Writes the supplied list of servers to persistent storage
        /// </summary>
        /// <param name="endpoints">List of server endpoints</param>
        /// <returns>Awaitable task for write completion</returns>
        public Task UpdateServerListAsync(IEnumerable<IPEndPoint> endpoints)
        {
            return Task.Run(() =>
            {
                try
                {
                    using (IsolatedStorageFileStream fileStream = isolatedStorage.OpenFile(FileName, FileMode.Create))
                    {
                        ProtoBuf.Serializer.Serialize(fileStream,
                            endpoints.Select(ep => new BasicServerListProto { ipAddress = ep.Address.ToString(), port = ep.Port }));
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
