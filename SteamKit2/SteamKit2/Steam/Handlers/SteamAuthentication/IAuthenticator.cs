using System.Threading.Tasks;

namespace SteamKit2
{
    /// <summary>
    /// 
    /// </summary>
    public interface IAuthenticator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<string> ProvideDeviceCode();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public Task<string> ProvideEmailCode(string email);
    }
}
