using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SteamKit2;

namespace NetHookAnalyzer2
{
	static class EMsgExtensions
	{
		public static string GetGCMessageName(uint eMsg)
		{
			eMsg = MsgUtil.GetGCMsg( eMsg );

			// first lets try the enum'd emsgs
			Type[] eMsgEnums =
			{
				typeof( SteamKit2.GC.Dota.Internal.EDOTAGCMsg ),
				typeof( SteamKit2.GC.Internal.EGCBaseMsg ),
				typeof( SteamKit2.GC.Internal.ESOMsg ),
				typeof( SteamKit2.GC.Internal.EGCSystemMsg ),
				typeof( SteamKit2.GC.Internal.EGCItemMsg ),
				typeof( SteamKit2.GC.Internal.EGCBaseClientMsg ),
			};

			foreach ( var enumType in eMsgEnums )
			{
				if ( Enum.IsDefined( enumType, ( int )eMsg ) )
					return Enum.GetName( enumType, ( int )eMsg );
			}

			// no dice on those, back to the classes
			List<FieldInfo> fields = new List<FieldInfo>();
			fields.AddRange( typeof( SteamKit2.GC.TF2.EGCMsg ).GetFields( BindingFlags.Public | BindingFlags.Static ) );
			fields.AddRange( typeof( SteamKit2.GC.CSGO.EGCMsg ).GetFields( BindingFlags.Public | BindingFlags.Static ) );

			var field = fields.SingleOrDefault( f =>
			{
				uint value = ( uint )f.GetValue( null );
				return value == eMsg;
			} );

			if ( field != null )
				return string.Format( "{0} ({1})", field.Name, field.DeclaringType.FullName );

			return eMsg.ToString();
		}
	}
}
