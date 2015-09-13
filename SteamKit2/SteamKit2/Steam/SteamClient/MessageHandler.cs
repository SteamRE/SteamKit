/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SteamKit2
{
    [AttributeUsage(AttributeTargets.Class)]
    class SteamCallbackAttribute : Attribute
    {
        public EMsg EMsg { get; set; }

        public SteamCallbackAttribute( EMsg eMsg )
        {
            this.EMsg = eMsg;
        }
    }

    class MessageHandler
    {
        class CallbackInfo
        {
            public EMsg NetworkMessage { get; set; }
            public Type CallbackType { get; set; }
        }


        SteamClient client;

        Dictionary<Type, Func<IPacketMsg, CallbackMsg>> activatorCache = new Dictionary<Type, Func<IPacketMsg, CallbackMsg>>();
        List<CallbackInfo> callbackList;


        public MessageHandler( SteamClient client )
        {
            this.client = client;

            callbackList = typeof( Internal.CMClient ).Assembly
                .GetTypes()
                .Where( t => t.HasAttribute<SteamCallbackAttribute>() )
                .Select( t =>
                {
                    SteamCallbackAttribute attrib = t.GetAttributes<SteamCallbackAttribute>()[0];

                    return new CallbackInfo { CallbackType = t, NetworkMessage = attrib.EMsg };
                } )
                .ToList();
        }


        public void RouteToCallback( IPacketMsg packetMsg )
        {
            Type[] callbackTypes = FindCallbacksForEMsg( packetMsg.MsgType );

            foreach ( var callbackType in callbackTypes )
            {
                CallbackMsg callbackObj = Create( callbackType, packetMsg );

                // post to the callback queue
                client.PostCallback( callbackObj );
            }
        }


        Type[] FindCallbacksForEMsg( EMsg msgType )
        {
            return callbackList
                .Where( c => c.NetworkMessage == msgType )
                .Select( c => c.CallbackType )
                .ToArray();
        }

        CallbackMsg Create( Type callbackType, IPacketMsg packetMsg )
        {
            Func<IPacketMsg, CallbackMsg> activator;

            if ( !activatorCache.TryGetValue( callbackType, out activator ) )
            {
                // if we don't have an activator cached, we need to compile one

                var callbackCtor = callbackType.GetConstructor( BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof( IPacketMsg ) }, null );

                if ( callbackCtor == null )
                {
                    throw new InvalidOperationException( string.Format( "No IPacketMsg constructor exists for a callback msg of type {0}", callbackType ) );
                }

                var paramExpr = Expression.Parameter( typeof( IPacketMsg ), "packetMsg" );
                var newExpr = Expression.New( callbackCtor, paramExpr );

                activator = Expression.Lambda<Func<IPacketMsg, CallbackMsg>>( newExpr, paramExpr ).Compile();

                // cache it off
                activatorCache[callbackType] = activator;
            }

            return activator( packetMsg );
        }
    }
}
