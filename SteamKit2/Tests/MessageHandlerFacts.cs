using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using SteamKit2;
using SteamKit2.Internal;
using Xunit;

namespace Tests
{
    public class MessageHandlerFacts
    {
        [Fact]
        public void AllSteamCallbacksHaveCorrectPacketMsgConstructor()
        {
            var steamCallbackTypes = typeof( CMClient ).Assembly
                .GetTypes()
                .Where( t => t.HasAttribute<SteamCallbackAttribute>() );

            foreach ( var type in steamCallbackTypes )
            {
                ConstructorInfo ctor = type.GetConstructor( BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof( IPacketMsg ) }, null );

                Assert.NotNull( ctor );
            }

        }

        [Fact]
        public void AllPacketMsgConstructorsHaveSteamCallbackAttribute()
        {
            var callbackTypes = typeof( CMClient ).Assembly
                .GetTypes()
                .Where( t => t.IsSubclassOf( typeof( CallbackMsg ) ) )
                .Where( t =>
                {
                    ConstructorInfo ctor = t.GetConstructor( BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof( IPacketMsg ) }, null );

                    return ctor != null;
                } );

            foreach ( var type in callbackTypes )
            {
                Assert.True( type.HasAttribute<SteamCallbackAttribute>() );
            }
        }
    }

    static class TypeUtils
    {
        public static T[] GetAttributes<T>( this Type type, bool inherit = false )
            where T : Attribute
        {
            return type.GetCustomAttributes( typeof( T ), inherit ) as T[];
        }
        public static bool HasAttribute<T>( this Type type, bool inherit = false )
            where T : Attribute
        {
            return type.GetAttributes<T>().Length > 0;
        }
    }
}
