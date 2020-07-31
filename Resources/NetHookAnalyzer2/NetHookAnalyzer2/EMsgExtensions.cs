using System;
using System.Collections.Generic;
using SteamKit2;

using Artifact = SteamKit2.GC.Artifact.Internal;
using CSGO = SteamKit2.GC.CSGO.Internal;
using Dota = SteamKit2.GC.Dota.Internal;
using TF2 = SteamKit2.GC.TF2.Internal;
using Underlords = SteamKit2.GC.Underlords.Internal;

namespace NetHookAnalyzer2
{
	static class EMsgExtensions
	{
		public static string GetGCMessageName(uint eMsg, uint appId)
		{
			eMsg = MsgUtil.GetGCMsg( eMsg );

			var eMsgEnums = GetGCEMsgEnums(appId);

			foreach ( var enumType in eMsgEnums )
			{
				if ( Enum.IsDefined( enumType, ( int )eMsg ) )
					return Enum.GetName( enumType, ( int )eMsg );
			}

			return eMsg.ToString();
		}

		static IEnumerable<Type> GetGCEMsgEnums(uint appId)
		{
			switch (appId)
			{
				case WellKnownAppIDs.TeamFortress2:
					yield return typeof(TF2.ETFGCMsg);
					yield return typeof(TF2.EGCBaseMsg);
					yield return typeof(TF2.ESOMsg);
					yield return typeof(TF2.EGCSystemMsg);
					yield return typeof(TF2.EGCItemMsg);
					yield return typeof(TF2.EGCBaseClientMsg);
					break;

				case WellKnownAppIDs.Dota2:
					yield return typeof(Dota.EDOTAGCMsg);
					yield return typeof(Dota.EGCBaseMsg);
					yield return typeof(Dota.ESOMsg);
					yield return typeof(Dota.EGCItemMsg);
					yield return typeof(Dota.EGCBaseClientMsg);
					break;

				case WellKnownAppIDs.CounterStrikeGlobalOffensive:
					yield return typeof(CSGO.ECsgoGCMsg);
					yield return typeof(CSGO.EGCBaseMsg);
					yield return typeof(CSGO.ESOMsg);
					yield return typeof(CSGO.EGCSystemMsg);
					yield return typeof(CSGO.EGCItemMsg);
					yield return typeof(CSGO.EGCBaseClientMsg);
					break;

				case WellKnownAppIDs.Artifact:
					yield return typeof(Artifact.EGCBaseMsg);
					yield return typeof(Artifact.ESOMsg);
					yield return typeof(Artifact.EGCItemMsg);
					yield return typeof(Artifact.EGCBaseClientMsg);
					yield return typeof(Artifact.EGCDCGClientMessages);
					yield return typeof(Artifact.EGCDCGCommonMessages);
					yield return typeof(Artifact.EGCDCGServerMessages);
					break;

                case WellKnownAppIDs.Underlords:
                    yield return typeof( Underlords.EGCBaseMsg );
                    yield return typeof( Underlords.ESOMsg );
                    yield return typeof( Underlords.EGCItemMsg );
                    yield return typeof( Underlords.EGCBaseClientMsg );
                    yield return typeof( Underlords.EGCDACClientMessages );
                    yield return typeof( Underlords.EGCDACCommonMessages );
                    yield return typeof( Underlords.EGCDACServerMessages );
                    break;
            }
		}
	}
}
