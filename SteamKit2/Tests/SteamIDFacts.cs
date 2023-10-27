using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteamKit2;

namespace Tests
{
    [TestClass]
    public class SteamIDFacts
    {
        [TestMethod]
        public void EmptyConstructorInvalid()
        {
            SteamID sid = new SteamID();

            Assert.IsFalse( sid.IsValid );
        }

        [TestMethod]
        public void FullConstructorValid()
        {
            SteamID sid = new SteamID( 1234, SteamID.ConsoleInstance, EUniverse.Beta, EAccountType.Chat );

            Assert.AreEqual( 1234u, sid.AccountID );
            Assert.AreEqual( SteamID.ConsoleInstance, sid.AccountInstance );
            Assert.AreEqual( EUniverse.Beta, sid.AccountUniverse );
            Assert.AreEqual( EAccountType.Chat, sid.AccountType );


            sid = new SteamID( 4321, EUniverse.Invalid, EAccountType.Pending );

            Assert.AreEqual( 4321u, sid.AccountID );
            Assert.AreEqual( SteamID.DesktopInstance, sid.AccountInstance );
            Assert.AreEqual( EUniverse.Invalid, sid.AccountUniverse );
            Assert.AreEqual( EAccountType.Pending, sid.AccountType );
        }

        [TestMethod]
        public void LongConstructorAndSetterGetterValid()
        {
            SteamID sid = new SteamID( 103582791432294076 );

            Assert.AreEqual( 2772668u, sid.AccountID );
            Assert.AreEqual( SteamID.AllInstances, sid.AccountInstance );
            Assert.AreEqual( EUniverse.Public, sid.AccountUniverse );
            Assert.AreEqual( EAccountType.Clan, sid.AccountType );

            sid.SetFromUInt64( 157626004137848889 );

            Assert.AreEqual( 12345u, sid.AccountID );
            Assert.AreEqual( SteamID.WebInstance, sid.AccountInstance );
            Assert.AreEqual( EUniverse.Beta, sid.AccountUniverse );
            Assert.AreEqual( EAccountType.GameServer, sid.AccountType );

            Assert.AreEqual( 157626004137848889ul, sid.ConvertToUInt64() );
        }

        [TestMethod]
        public void Steam2CorrectParse()
        {
            SteamID sidEven = new SteamID( "STEAM_0:0:4491990" );

            Assert.AreEqual( 8983980u, sidEven.AccountID );
            Assert.AreEqual( SteamID.DesktopInstance, sidEven.AccountInstance );
            Assert.AreEqual( EUniverse.Public, sidEven.AccountUniverse );


            SteamID sidOdd = new SteamID( "STEAM_0:1:4491990" );

            Assert.AreEqual( 8983981u, sidOdd.AccountID );
            Assert.AreEqual( SteamID.DesktopInstance, sidOdd.AccountInstance );
            Assert.AreEqual( EUniverse.Public, sidOdd.AccountUniverse );
        }

        [TestMethod]
        public void SetFromSteam3StringCorrectParse()
        {
            SteamID sidUser = new SteamID();
            sidUser.SetFromSteam3String( "[U:1:123]" );
            Assert.AreEqual( 123u, sidUser.AccountID );
            Assert.AreEqual( EUniverse.Public, sidUser.AccountUniverse );
            Assert.AreEqual( 1u, sidUser.AccountInstance );
            Assert.AreEqual( EAccountType.Individual, sidUser.AccountType );

            SteamID sidAnonGSUser = new SteamID();
            sidAnonGSUser.SetFromSteam3String( "[A:1:123:456]" );
            Assert.AreEqual( 123u, sidAnonGSUser.AccountID );
            Assert.AreEqual( EUniverse.Public, sidAnonGSUser.AccountUniverse );
            Assert.AreEqual( 456u, sidAnonGSUser.AccountInstance );
            Assert.AreEqual( EAccountType.AnonGameServer, sidAnonGSUser.AccountType );

            SteamID sidLobby = new SteamID();
            sidLobby.SetFromSteam3String( "[L:1:123]" );
            Assert.AreEqual( 123u, sidLobby.AccountID );
            Assert.AreEqual( EUniverse.Public, sidLobby.AccountUniverse );
            Assert.IsTrue( ( ( SteamID.ChatInstanceFlags )sidLobby.AccountInstance ).HasFlag( SteamID.ChatInstanceFlags.Lobby ) );
            Assert.AreEqual( EAccountType.Chat, sidLobby.AccountType );

            SteamID sidClanChat = new SteamID();
            sidClanChat.SetFromSteam3String( "[c:1:123]" );
            Assert.AreEqual( 123u, sidClanChat.AccountID );
            Assert.AreEqual( EUniverse.Public, sidClanChat.AccountUniverse );
            Assert.IsTrue( ( ( SteamID.ChatInstanceFlags )sidClanChat.AccountInstance ).HasFlag( SteamID.ChatInstanceFlags.Clan ) );
            Assert.AreEqual( EAccountType.Chat, sidClanChat.AccountType );

            SteamID sidMultiseat = new SteamID();
            sidMultiseat.SetFromSteam3String( "[M:1:123:456]" );
            Assert.AreEqual( 123u, sidMultiseat.AccountID );
            Assert.AreEqual( EUniverse.Public, sidMultiseat.AccountUniverse );
            Assert.AreEqual( 456u, sidMultiseat.AccountInstance );
            Assert.AreEqual( EAccountType.Multiseat, sidMultiseat.AccountType );

            SteamID sidLowercaseI = new SteamID();
            sidLowercaseI.SetFromSteam3String( "[i:2:456]" );
            Assert.AreEqual( 456u, sidLowercaseI.AccountID );
            Assert.AreEqual( EUniverse.Beta, sidLowercaseI.AccountUniverse );
            Assert.AreEqual( 1u, sidLowercaseI.AccountInstance );
            Assert.AreEqual( EAccountType.Invalid, sidLowercaseI.AccountType );
        }

        [TestMethod]
        public void SetFromOldStyleSteam3StringCorrectParse()
        {
            SteamID sidMultiseat = new SteamID();
            sidMultiseat.SetFromSteam3String( "[M:1:123(456)]" );
            Assert.AreEqual( 123u, sidMultiseat.AccountID);
            Assert.AreEqual( EUniverse.Public, sidMultiseat.AccountUniverse );
            Assert.AreEqual( 456u, sidMultiseat.AccountInstance );
            Assert.AreEqual( EAccountType.Multiseat, sidMultiseat.AccountType );

            SteamID sidAnonGSUser = new SteamID();
            sidAnonGSUser.SetFromSteam3String( "[A:1:123(456)]" );
            Assert.AreEqual( 123u, sidAnonGSUser.AccountID );
            Assert.AreEqual( EUniverse.Public, sidAnonGSUser.AccountUniverse );
            Assert.AreEqual( 456u, sidAnonGSUser.AccountInstance );
            Assert.AreEqual( EAccountType.AnonGameServer, sidAnonGSUser.AccountType );
        }

        [TestMethod]
        public void Steam3StringSymmetric()
        {
            var steamIds = new[]
            {
                "[U:1:123]",
                "[U:1:123:2]",
                "[G:1:626]",
                "[A:2:165:1234]",
                "[M:2:165:1234]",
            };

            foreach ( var steamId in steamIds )
            {
                SteamID sid = new SteamID();
                bool parsed = sid.SetFromSteam3String( steamId );
                Assert.IsTrue( parsed );
                Assert.AreEqual( steamId, sid.Render() );
            }
        }

        [TestMethod]
        public void SetFromStringHandlesInvalid()
        {
            SteamID sid = new SteamID();

            bool setFromNullString = sid.SetFromString( null, EUniverse.Public );
            Assert.IsFalse( setFromNullString );

            bool setFromEmptyString = sid.SetFromString( "", EUniverse.Public );
            Assert.IsFalse( setFromEmptyString );

            bool setFromInvalidString = sid.SetFromString( "NOT A STEAMID!", EUniverse.Public );
            Assert.IsFalse( setFromInvalidString );

            bool setFromInvalidAccountId = sid.SetFromString( "STEAM_0:1:999999999999999999999999999999", EUniverse.Public );
            Assert.IsFalse( setFromInvalidAccountId );

            bool universeOutOfRange = sid.SetFromSteam3String( "STEAM_5:0:123" );
            Assert.IsFalse( universeOutOfRange );
        }

        [TestMethod]
        public void SetFromSteam3StringHandlesInvalid()
        {
            SteamID sid = new SteamID();

            bool setFromNullString = sid.SetFromSteam3String( null );
            Assert.IsFalse( setFromNullString );

            bool setFromEmptyString = sid.SetFromSteam3String("");
            Assert.IsFalse( setFromEmptyString );

            bool setFromInvalidString = sid.SetFromSteam3String( "NOT A STEAMID!" );
            Assert.IsFalse( setFromInvalidString );

            bool setFromInvalidAccountId = sid.SetFromSteam3String( "STEAM_0:1:999999999999999999999999999999" );
            Assert.IsFalse( setFromInvalidAccountId );

            bool setFromSteam2String = sid.SetFromSteam3String( "STEAM_0:1:4491990" );
            Assert.IsFalse(setFromSteam2String);

            bool mixingBracketsAndColons1 = sid.SetFromSteam3String( "[A:1:2:345)]" );
            Assert.IsFalse( mixingBracketsAndColons1 );

            bool mixingBracketsAndColons2 = sid.SetFromSteam3String( "[A:1:2(345]" );
            Assert.IsFalse( mixingBracketsAndColons2 );

            bool universeOutOfRange = sid.SetFromSteam3String( "[U:5:123]" );
            Assert.IsFalse( universeOutOfRange );
        }

        [TestMethod]
        public void SetValidAndHandlesClan()
        {
            SteamID sid = new SteamID();

            sid.Set( 1234u, EUniverse.Internal, EAccountType.ContentServer );

            Assert.AreEqual( 1234u, sid.AccountID );
            Assert.AreEqual( EUniverse.Internal, sid.AccountUniverse );
            Assert.AreEqual( EAccountType.ContentServer, sid.AccountType );
            Assert.AreEqual( SteamID.DesktopInstance, sid.AccountInstance );


            sid.Set( 4321u, EUniverse.Public, EAccountType.Clan );

            Assert.AreEqual( 4321u, sid.AccountID );
            Assert.AreEqual( EUniverse.Public, sid.AccountUniverse );
            Assert.AreEqual( EAccountType.Clan, sid.AccountType );
            Assert.AreEqual( 0u, sid.AccountInstance );
        }

        [TestMethod]
        public void Steam2RenderIsValid()
        {
            SteamID sid = 76561197969249708;

            Assert.AreEqual( "STEAM_0:0:4491990", sid.Render( steam3: false ) );

            sid.AccountUniverse = EUniverse.Beta;
            Assert.AreEqual( "STEAM_2:0:4491990", sid.Render( steam3: false ) );

            sid.AccountType = EAccountType.GameServer;
            Assert.AreEqual( "157625991261918636", sid.Render( steam3: false ) );
        }

        [TestMethod]
        public void RendersSteam3ByDefault()
        {
            SteamID sid = 76561197969249708;

            Assert.AreEqual( "[U:1:8983980]", sid.Render() );
            Assert.AreEqual( "[U:1:8983980]", sid.ToString() );
        }

        [TestMethod]
        public void SteamIDsEquality()
        {
            SteamID sid = 76561197969249708;
            SteamID sid2 = new SteamID( 76561197969249708 );

            Assert.IsTrue( sid == sid2 );
            Assert.IsTrue( sid.Equals( sid2 ) );
            Assert.IsTrue( sid.Equals( ( object )sid2 ) );

            Assert.IsFalse( sid.Equals( new object() ) );
            Assert.IsFalse( sid.Equals( ( SteamID )null ) );
            Assert.IsFalse( sid.Equals( ( object )null ) );

            SteamID sid3 = 12345;

            Assert.IsTrue( sid != sid3 );

            ulong sid4 = sid;
            Assert.IsTrue( sid4 == sid );
        }

        [TestMethod]
        public void SteamIDHashCodeUsesLongHashCode()
        {
            SteamID sid = 172376458626834;
            ulong longValue = 172376458626834;

            Assert.IsTrue( sid.GetHashCode() == longValue.GetHashCode() );
        }

        [TestMethod]
        public void InitializesInstancesCorrectly()
        {
            SteamID sid = new SteamID();

            sid.SetFromSteam3String( "[g:1:1234]" );
            Assert.AreEqual( EUniverse.Public, sid.AccountUniverse );
            Assert.AreEqual( EAccountType.Clan, sid.AccountType );
            Assert.AreEqual( 0u, sid.AccountInstance );
            Assert.AreEqual( 1234u, sid.AccountID );

            sid.SetFromSteam3String( "[T:1:1234]" );
            Assert.AreEqual( EUniverse.Public, sid.AccountUniverse );
            Assert.AreEqual( EAccountType.Chat, sid.AccountType );
            Assert.AreEqual( 0u, sid.AccountInstance );
            Assert.AreEqual( 1234u, sid.AccountID );

            sid.SetFromSteam3String( "[c:1:1234]" );
            Assert.AreEqual( EUniverse.Public, sid.AccountUniverse );
            Assert.AreEqual( EAccountType.Chat, sid.AccountType );
            Assert.AreEqual( (uint)SteamID.ChatInstanceFlags.Clan, sid.AccountInstance );
            Assert.AreEqual( 1234u, sid.AccountID);

            sid.SetFromSteam3String( "[L:1:1234]" );
            Assert.AreEqual( EUniverse.Public, sid.AccountUniverse );
            Assert.AreEqual( EAccountType.Chat, sid.AccountType );
            Assert.AreEqual( ( uint ) SteamID.ChatInstanceFlags.Lobby, sid.AccountInstance );
            Assert.AreEqual( 1234u, sid.AccountID );
        }

        [TestMethod]
        public void RendersOutOfRangeAccountTypeAsLowercaseI()
        {
            SteamID sid = new SteamID( 123, EUniverse.Beta, (EAccountType)(-1) );
            Assert.AreEqual( "[i:2:123]", sid.Render() );
        }

        [TestMethod]
        public void ToChatIDConvertsWellKnownID()
        {
            var clanID = new SteamID( 4, EUniverse.Public, EAccountType.Clan );
            var expectedChatID = 110338190870577156UL;
            Assert.AreEqual( expectedChatID, clanID.ToChatID().ConvertToUInt64() );
        }

        [TestMethod]
        public void ToChatIDDoesNotModifySelf()
        {
            var clanID = new SteamID( 4, EUniverse.Public, EAccountType.Clan );
            clanID.ToChatID();
            Assert.AreEqual( EUniverse.Public, clanID.AccountUniverse );
            Assert.AreEqual( EAccountType.Clan, clanID.AccountType );
            Assert.AreEqual( 0u, clanID.AccountInstance );
            Assert.AreEqual( 4u, clanID.AccountID );
        }

        [TestMethod]
        public void ToChatIDOnlySupportsClans()
        {
            var types = new[]
            {
                EAccountType.AnonGameServer,
                EAccountType.AnonUser,
                EAccountType.Chat,
                EAccountType.ConsoleUser,
                EAccountType.ContentServer,
                EAccountType.GameServer,
                EAccountType.Individual,
                EAccountType.Multiseat,
                EAccountType.Pending,
            };

            foreach ( var type in types )
            {
                var id = new SteamID( 1, EUniverse.Public, type );
                Assert.ThrowsException<InvalidOperationException>( () => id.ToChatID() );
            }
        }

        [TestMethod]
        public void TryGetClanIDConvertsWellKnownID()
        {
            var clanID = new SteamID( 4, (uint)SteamID.ChatInstanceFlags.Clan, EUniverse.Public, EAccountType.Chat );
            Assert.IsTrue( clanID.TryGetClanID( out var groupID ) );
            Assert.AreEqual( 103582791429521412UL, groupID.ConvertToUInt64() );
        }

        [TestMethod]
        public void TryGetClanIDDoesNotModifySelf()
        {
            var clanID = new SteamID(4, (uint)SteamID.ChatInstanceFlags.Clan, EUniverse.Public, EAccountType.Chat );
            Assert.IsTrue( clanID.TryGetClanID( out var groupID ) );

            Assert.AreEqual( EUniverse.Public, clanID.AccountUniverse );
            Assert.AreEqual( EAccountType.Chat, clanID.AccountType );
            Assert.AreEqual( ( uint )SteamID.ChatInstanceFlags.Clan, clanID.AccountInstance );
            Assert.AreEqual( 4u, clanID.AccountID );
        }

        [TestMethod]
        public void TryGetClanIDReturnsFalseForAdHocChatRoom()
        {
            var chatID = new SteamID( 108093571196988453 );
            Assert.IsFalse( chatID.TryGetClanID( out var groupID ), groupID?.Render() );
            Assert.IsNull( groupID );
        }

        [TestMethod]
        public void TryGetClanIDOnlySupportsClanChatRooms()
        {
            var types = new[]
            {
                EAccountType.AnonGameServer,
                EAccountType.AnonUser,
                EAccountType.ConsoleUser,
                EAccountType.ContentServer,
                EAccountType.GameServer,
                EAccountType.Individual,
                EAccountType.Multiseat,
                EAccountType.Pending,
            };

            foreach ( var type in types )
            {
                var id = new SteamID( 4, ( uint )SteamID.ChatInstanceFlags.Clan, EUniverse.Public, type );
                Assert.IsFalse( id.TryGetClanID( out var groupID ), groupID?.Render() );
                Assert.IsNull( groupID );
            }
        }

        [TestMethod]
        public void KnownAccountTypesAreValid()
        {
            var types = new[]
            {
                EAccountType.Individual,
                EAccountType.Multiseat,
                EAccountType.GameServer,
                EAccountType.AnonGameServer,
                EAccountType.Pending,
                EAccountType.ContentServer,
                EAccountType.Clan,
                EAccountType.Chat,
                EAccountType.ConsoleUser,
                EAccountType.AnonUser,
            };

            foreach ( var type in types )
            {
                SteamID sid = 76561198074261126;
                sid.AccountInstance = 0; // for Clan to pass
                sid.AccountType = type;
                Assert.IsTrue( sid.IsValid );
            }
        }

        [TestMethod]
        public void UnknownAccountTypesAreInvalid()
        {
            var types = new[]
            {
                EAccountType.Invalid,
                (EAccountType)11,
                (EAccountType)12,
                (EAccountType)13,
            };

            foreach ( var type in types )
            {
                SteamID sid = 76561198074261126;
                sid.AccountType = type;
                Assert.IsFalse( sid.IsValid );
            }
        }

        [TestMethod]
        public void KnownAccountUniversesAreValid()
        {
            var universes = new[]
            {
                EUniverse.Public,
                EUniverse.Beta,
                EUniverse.Internal,
                EUniverse.Dev,
            };

            foreach ( var universe in universes )
            {
                SteamID sid = 76561198074261126;
                sid.AccountUniverse = universe;
                Assert.IsTrue( sid.IsValid );
            }
        }

        [TestMethod]
        public void UnknownAccountUniversesAreInvalid()
        {
            var universes = new[]
            {
                EUniverse.Invalid,
                (EUniverse)5,
                (EUniverse)6,
                (EUniverse)7,
            };

            foreach ( var universe in universes )
            {
                SteamID sid = 76561198074261126;
                sid.AccountUniverse = universe;
                Assert.IsFalse( sid.IsValid );
            }
        }

        [TestMethod]
        public void EUniverseEnumHasNotChanged()
        {
            // If this enum has changed, update SteamID.IsValid
            Assert.AreEqual( 5, Enum.GetValues( typeof( EUniverse ) ).Length );
        }

        [TestMethod]
        public void EAccountTypeEnumHasNotChanged()
        {
            // If this enum has changed, update SteamID.IsValid
            Assert.AreEqual( 11, Enum.GetValues( typeof( EAccountType ) ).Length );
        }
    }
}
