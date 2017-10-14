using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ProtoBuf;

namespace SteamKit2.Discovery
{
    /// <summary>
    /// A server list provider that uses a file to persist the server list using protobuf
    /// </summary>
    public class FileStorageServerListProvider : IServerListProvider
    {
        readonly string filename;

        /// <summary>
        /// Initialize a new instance of FileStorageServerListProvider
        /// </summary>
        public FileStorageServerListProvider(string filename)
        {
            this.filename = filename ?? throw new ArgumentNullException(nameof(filename));
        }

        /// <summary>
        /// Read the stored list of servers from the file
        /// </summary>
        /// <returns>List of servers if persisted, otherwise an empty list</returns>
        public Task<IEnumerable<ServerRecord>> FetchServerListAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    using (FileStream fileStream = File.OpenRead(filename))
                    {
                        return Serializer.DeserializeItems<BasicServerListProto>(fileStream, PrefixStyle.Base128, 1)
                            .Select(item =>
                            {
                                return ServerRecord.CreateServer(item.Address, item.Port, item.Protocols);
                            })
                            .ToList();
                    }
                }
                catch (IOException ex)
                {
                    DebugLog.WriteLine("FileStorageServerListProvider", "Failed to read file {0}: {1}", filename, ex.Message);
                    return Enumerable.Empty<ServerRecord>();
                }
            });
        }

        /// <summary>
        /// Writes the supplied list of servers to persistent storage
        /// </summary>
        /// <param name="endpoints">List of server endpoints</param>
        /// <returns>Awaitable task for write completion</returns>
        public Task UpdateServerListAsync(IEnumerable<ServerRecord> endpoints)
        {
            if (endpoints == null)
            {
                throw new ArgumentNullException(nameof(endpoints));
            }

            return Task.Run(() =>
            {
                try
                {
                    using (var fileStream = File.OpenWrite(filename))
                    {
                        Serializer.Serialize(fileStream,
                            endpoints.Select(ep =>
                            {
                                return new BasicServerListProto
                                {
                                    Address = ep.GetHost(),
                                    Port = ep.GetPort(),
                                    Protocols = ep.ProtocolTypes
                                };
                            }));
                        fileStream.SetLength(fileStream.Position);
                    }
                }
                catch (IOException ex)
                {
                    DebugLog.WriteLine("FileStorageServerListProvider", "Failed to write file {0}: {1}", filename, ex.Message);
                }
            });
        }
    }
}
