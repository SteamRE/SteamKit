/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace SteamKit3
{
    /// <summary>
    /// This attribute must be applied to all <see cref="ClientHandler">ClientHandlers</see> that wish to be registered with a <see cref="SteamClient"/>.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, Inherited = false, AllowMultiple = false )]
    public sealed class HandlerAttribute : Attribute
    {
    }

    /// <summary>
    /// This class implements the base requirements every message handler should inherit from.
    /// </summary>
    public abstract class ClientHandler
    {
        /// <summary>
        /// Gets the <see cref="SteamClient"/> associated with this handler.
        /// </summary>
        protected SteamClient Client { get; private set; }
        /// <summary>
        /// Gets the <see cref="JobMgr"/> associated with this handler.
        /// </summary>
        protected JobMgr JobMgr { get { return Client.JobMgr; } }


        /// <summary>
        /// Initializes a new instance of the <see cref="ClientHandler"/> class.
        /// </summary>
        public ClientHandler()
        {
        }


        internal void Setup( SteamClient client )
        {
            Contract.Requires( client != null );

            Client = client;
        }
    }
}
