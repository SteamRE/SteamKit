using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SteamKit2.Discovery
{
    /// <summary>
    /// A server list provider that uses IsolatedStorage to persist the server list
    /// </summary>
    public class IsolatedStorageServerListProvider : ServerListProvider
    {
        private static string FileName = "serverlist.json";

        [ProtoContract]
        class ServerListProto
        {
            [ProtoMember(1)]
            public String ipAddress { get; set; }
            [ProtoMember(2)]
            public int port { get; set; }
        }

        private IsolatedStorageFile isolatedStorage;

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
        public async Task<ICollection<IPEndPoint>> FetchServerList()
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (IsolatedStorageFileStream fileStream = isolatedStorage.OpenFile(FileName, FileMode.Open, FileAccess.Read))
                    {
                        await fileStream.CopyToAsync(ms);
                        ms.Position = 0;

                        return ProtoBuf.Serializer.DeserializeItems<ServerListProto>(ms, PrefixStyle.Base128, 1).Select(item => new IPEndPoint(IPAddress.Parse(item.ipAddress), item.port)).ToList();
                    }
                }
            }
            catch (IOException ex)
            {
                DebugLog.WriteLine("IsolatedStorageServerListProvider", "Failed to read file {0}: {1}", FileName, ex.Message);
                return new List<IPEndPoint>();
            }
        }

        /// <summary>
        /// Writes the supplied list of servers to persistent storage
        /// </summary>
        /// <param name="endpoints">List of server endpoints</param>
        /// <returns>Awaitable task for write completion</returns>
        public async Task UpdateServerList(IEnumerable<IPEndPoint> endpoints)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    ProtoBuf.Serializer.Serialize(ms, endpoints.Select(ep => new ServerListProto() { ipAddress = ep.Address.ToString(), port = ep.Port }));
                    ms.Position = 0;

                    using (IsolatedStorageFileStream fileStream = isolatedStorage.OpenFile("serverlist.json", FileMode.Create))
                    {
                        await ms.CopyToAsync(fileStream);
                    }
                }
            }
            catch (IOException ex)
            {
                DebugLog.WriteLine("IsolatedStorageServerListProvider", "Failed to write file {0}: {1}", FileName, ex.Message);
            }
        }

    }
}
