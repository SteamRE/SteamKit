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
    /// A server list provider that uses a file to persist the server list using protobuf
    /// </summary>
    public class FileStorageServerListProvider : ServerListProvider
    {
        [ProtoContract]
        class ServerListProto
        {
            [ProtoMember(1)]
            public String ipAddress { get; set; }
            [ProtoMember(2)]
            public int port { get; set; }
        }

        private string filename;

        /// <summary>
        /// Initialize a new instance of FileStorageServerListProvider
        /// </summary>
        public FileStorageServerListProvider(string filename)
        {
            this.filename = filename;
        }

        /// <summary>
        /// Read the stored list of servers from the file
        /// </summary>
        /// <returns>List of servers if persisted, otherwise an empty list</returns>
        public async Task<ICollection<IPEndPoint>> FetchServerList()
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (FileStream fileStream = File.OpenRead(filename))
                    {
                        await fileStream.CopyToAsync(ms);
                        ms.Position = 0;

                        return ProtoBuf.Serializer.DeserializeItems<ServerListProto>(ms, PrefixStyle.Base128, 1).Select(item => new IPEndPoint(IPAddress.Parse(item.ipAddress), item.port)).ToList();
                    }
                }
            }
            catch (IOException ex)
            {
                DebugLog.WriteLine("FileStorageServerListProvider", "Failed to read file {0}: {1}", filename, ex.Message);
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

                    using (FileStream fileStream = File.OpenWrite(filename))
                    {
                        await ms.CopyToAsync(fileStream);
                    }
                }
            }
            catch (IOException ex)
            {
                DebugLog.WriteLine("FileStorageServerListProvider", "Failed to write file {0}: {1}", filename, ex.Message);
            }
        }

    }
}
