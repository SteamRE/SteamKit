/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using SteamKit2.Internal;

namespace SteamKit2.Authentication
{
    /// <summary>
    /// Represents the details required to authenticate on Steam.
    /// </summary>
    public sealed class AuthSessionDetails
    {
        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        /// <value>The username.</value>
        public string? Username { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>The password.</value>
        public string? Password { get; set; }

        /// <summary>
        /// Gets or sets the device name (or user agent).
        /// </summary>
        /// <value>The device name.</value>
        public string? DeviceFriendlyName { get; set; } = $"{Environment.MachineName} (SteamKit2)";

        /// <summary>
        /// Gets or sets the platform type that the login will be performed for.
        /// </summary>
        public EAuthTokenPlatformType PlatformType { get; set; } = EAuthTokenPlatformType.k_EAuthTokenPlatformType_SteamClient;

        /// <summary>
        /// Gets or sets the client operating system type.
        /// </summary>
        /// <value>The client operating system type.</value>
        public EOSType ClientOSType { get; set; } = Utils.GetOSType();

        /// <summary>
        /// Gets or sets the session persistence.
        /// </summary>
        /// <value>The persistence.</value>
        public bool IsPersistentSession { get; set; } = false;

        /// <summary>
        /// Gets or sets the website id that the login will be performed for.
        /// Known values are "Unknown", "Client", "Mobile", "Website", "Store", "Community", "Partner", "SteamStats".
        /// </summary>
        /// <value>The website id.</value>
        public string? WebsiteID { get; set; } = "Client";

        /// <summary>
        /// Steam guard data for client login. Provide <see cref="AuthPollResult.NewGuardData"/> if available.
        /// </summary>
        /// <value>The guard data.</value>
        public string? GuardData { get; set; }

        /// <summary>
        /// Authenticator object which will be used to handle 2-factor authentication if necessary.
        /// Use <see cref="UserConsoleAuthenticator"/> for a default implementation.
        /// </summary>
        /// <value>The authenticator object.</value>
        public IAuthenticator? Authenticator { get; set; }
    }
}
