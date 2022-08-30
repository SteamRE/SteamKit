using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SteamKit2
{
    public interface IAuthenticator
    {
        public Task<string> ProvideDeviceCode();
        public Task<string> ProvideEmailCode(string email);
    }
}
