using Xunit;
using SteamKit2;

namespace Tests
{
    public class SteamIDFacts
    {
        [Fact]
        public void EmptyConstructorInvalid()
        {
            SteamID sid = new SteamID();

            Assert.False( sid.IsValid );
        }

        [Fact]
        public void FullConstructorValid()
        {
            SteamID sid = new SteamID( 1234, SteamID.ConsoleInstance, EUniverse.Beta, EAccountType.Chat );

            Assert.Equal( 1234u, sid.AccountID );
            Assert.Equal( SteamID.ConsoleInstance, sid.AccountInstance );
            Assert.Equal( EUniverse.Beta, sid.AccountUniverse );
            Assert.Equal( EAccountType.Chat, sid.AccountType );


            sid = new SteamID( 4321, EUniverse.Invalid, EAccountType.Pending );

            Assert.Equal( 4321u, sid.AccountID );
            Assert.Equal( SteamID.DesktopInstance, sid.AccountInstance );
            Assert.Equal( EUniverse.Invalid, sid.AccountUniverse );
            Assert.Equal( EAccountType.Pending, sid.AccountType );
        }

        [Fact]
        public void LongConstructorAndSetterGetterValid()
        {
            SteamID sid = new SteamID( 103582791432294076 );

            Assert.Equal( 2772668u, sid.AccountID );
            Assert.Equal( SteamID.AllInstances, sid.AccountInstance );
            Assert.Equal( EUniverse.Public, sid.AccountUniverse );
            Assert.Equal( EAccountType.Clan, sid.AccountType );

            sid.SetFromUInt64( 157626004137848889 );

            Assert.Equal( 12345u, sid.AccountID );
            Assert.Equal( SteamID.WebInstance, sid.AccountInstance );
            Assert.Equal( EUniverse.Beta, sid.AccountUniverse );
            Assert.Equal( EAccountType.GameServer, sid.AccountType );

            Assert.Equal( 157626004137848889ul, sid.ConvertToUInt64() );
        }

        [Fact]
        public void Steam2CorrectParse()
        {
            SteamID sidEven = new SteamID( "STEAM_0:0:4491990" );

            Assert.Equal( 8983980u, sidEven.AccountID );
            Assert.Equal( SteamID.DesktopInstance, sidEven.AccountInstance );
            Assert.Equal( EUniverse.Public, sidEven.AccountUniverse );


            SteamID sidOdd = new SteamID( "STEAM_0:1:4491990" );

            Assert.Equal( 8983981u, sidOdd.AccountID );
            Assert.Equal( SteamID.DesktopInstance, sidOdd.AccountInstance );
            Assert.Equal( EUniverse.Public, sidOdd.AccountUniverse );
        }

        [Fact]
        public void SetFromSteam3StringCorrectParse()
        {
            SteamID sidUser = new SteamID();
            sidUser.SetFromSteam3String( "[U:1:123]" );
            Assert.Equal( 123u, sidUser.AccountID );
            Assert.Equal( EUniverse.Public, sidUser.AccountUniverse );
            Assert.Equal( 1u, sidUser.AccountInstance );
            Assert.Equal( EAccountType.Individual, sidUser.AccountType );

            SteamID sidAnonGSUser = new SteamID();
            sidAnonGSUser.SetFromSteam3String( "[A:1:123:456]" );
            Assert.Equal( 123u, sidAnonGSUser.AccountID );
            Assert.Equal( EUniverse.Public, sidAnonGSUser.AccountUniverse );
            Assert.Equal( 456u, sidAnonGSUser.AccountInstance );
            Assert.Equal( EAccountType.AnonGameServer, sidAnonGSUser.AccountType );

            SteamID sidLobby = new SteamID();
            sidLobby.SetFromSteam3String( "[L:1:123]" );
            Assert.Equal( 123u, sidLobby.AccountID );
            Assert.Equal( EUniverse.Public, sidLobby.AccountUniverse );
            Assert.True( ( ( SteamID.ChatInstanceFlags )sidLobby.AccountInstance ).HasFlag( SteamID.ChatInstanceFlags.Lobby ) );
            Assert.Equal( EAccountType.Chat, sidLobby.AccountType );

            SteamID sidClanChat = new SteamID();
            sidClanChat.SetFromSteam3String( "[c:1:123]" );
            Assert.Equal( 123u, sidClanChat.AccountID );
            Assert.Equal( EUniverse.Public, sidClanChat.AccountUniverse );
            Assert.True( ( ( SteamID.ChatInstanceFlags )sidClanChat.AccountInstance ).HasFlag( SteamID.ChatInstanceFlags.Clan ) );
            Assert.Equal( EAccountType.Chat, sidClanChat.AccountType );

            SteamID sidMultiseat = new SteamID();
            sidMultiseat.SetFromSteam3String( "[M:1:123:456]" );
            Assert.Equal( 123u, sidMultiseat.AccountID );
            Assert.Equal( EUniverse.Public, sidMultiseat.AccountUniverse );
            Assert.Equal( 456u, sidMultiseat.AccountInstance );
            Assert.Equal( EAccountType.Multiseat, sidMultiseat.AccountType );

            SteamID sidLowercaseI = new SteamID();
            sidLowercaseI.SetFromSteam3String( "[i:2:456]" );
            Assert.Equal( 456u, sidLowercaseI.AccountID );
            Assert.Equal( EUniverse.Beta, sidLowercaseI.AccountUniverse );
            Assert.Equal( 1u, sidLowercaseI.AccountInstance );
            Assert.Equal( EAccountType.Invalid, sidLowercaseI.AccountType );
        }

        [Fact]
        public void SetFromOldStyleSteam3StringCorrectParse()
        {
            SteamID sidMultiseat = new SteamID();
            sidMultiseat.SetFromSteam3String( "[M:1:123(456)]" );
            Assert.Equal( 123u, sidMultiseat.AccountID);
            Assert.Equal( EUniverse.Public, sidMultiseat.AccountUniverse );
            Assert.Equal( 456u, sidMultiseat.AccountInstance );
            Assert.Equal( EAccountType.Multiseat, sidMultiseat.AccountType );

            SteamID sidAnonGSUser = new SteamID();
            sidAnonGSUser.SetFromSteam3String( "[A:1:123(456)]" );
            Assert.Equal( 123u, sidAnonGSUser.AccountID );
            Assert.Equal( EUniverse.Public, sidAnonGSUser.AccountUniverse );
            Assert.Equal( 456u, sidAnonGSUser.AccountInstance );
            Assert.Equal( EAccountType.AnonGameServer, sidAnonGSUser.AccountType );
        }

        [Fact]
        public void Steam3StringSymmetric()
        {
            var steamIds = new[]
            {
                "[U:1:123]",
                "[U:1:123:2]",
                "[G:1:626]",
                "[A:2:165:1234]",
            };

            foreach ( var steamId in steamIds )
            {
                SteamID sid = new SteamID();
                bool parsed = sid.SetFromSteam3String( steamId );
                Assert.True( parsed );
                Assert.Equal( steamId, sid.Render( steam3: true ) );
            }
        }

        [Fact]
        public void SetFromStringHandlesInvalid()
        {
            SteamID sid = new SteamID();

            bool setFromNullString = sid.SetFromString( null, EUniverse.Public );
            Assert.False( setFromNullString );

            bool setFromEmptyString = sid.SetFromString( "", EUniverse.Public );
            Assert.False( setFromEmptyString );

            bool setFromInvalidString = sid.SetFromString( "NOT A STEAMID!", EUniverse.Public );
            Assert.False( setFromInvalidString );

            bool setFromInvalidAccountId = sid.SetFromString( "STEAM_0:1:999999999999999999999999999999", EUniverse.Public );
            Assert.False( setFromInvalidAccountId );

            bool universeOutOfRange = sid.SetFromSteam3String( "STEAM_5:0:123" );
            Assert.False( universeOutOfRange );
        }

        [Fact]
        public void SetFromSteam3StringHandlesInvalid()
        {
            SteamID sid = new SteamID();

            bool setFromNullString = sid.SetFromSteam3String( null );
            Assert.False( setFromNullString );

            bool setFromEmptyString = sid.SetFromSteam3String("");
            Assert.False( setFromEmptyString );

            bool setFromInvalidString = sid.SetFromSteam3String( "NOT A STEAMID!" );
            Assert.False( setFromInvalidString );

            bool setFromInvalidAccountId = sid.SetFromSteam3String( "STEAM_0:1:999999999999999999999999999999" );
            Assert.False( setFromInvalidAccountId );

            bool setFromSteam2String = sid.SetFromSteam3String( "STEAM_0:1:4491990" );
            Assert.False(setFromSteam2String);

            bool mixingBracketsAndColons1 = sid.SetFromSteam3String( "[A:1:2:345)]" );
            Assert.False( mixingBracketsAndColons1 );

            bool mixingBracketsAndColons2 = sid.SetFromSteam3String( "[A:1:2(345]" );
            Assert.False( mixingBracketsAndColons2 );

            bool universeOutOfRange = sid.SetFromSteam3String( "[U:5:123]" );
            Assert.False( universeOutOfRange );
        }

        [Fact]
        public void SetValidAndHandlesClan()
        {
            SteamID sid = new SteamID();

            sid.Set( 1234u, EUniverse.Internal, EAccountType.ContentServer );

            Assert.Equal( 1234u, sid.AccountID );
            Assert.Equal( EUniverse.Internal, sid.AccountUniverse );
            Assert.Equal( EAccountType.ContentServer, sid.AccountType );
            Assert.Equal( SteamID.DesktopInstance, sid.AccountInstance );


            sid.Set( 4321u, EUniverse.Public, EAccountType.Clan );

            Assert.Equal( 4321u, sid.AccountID );
            Assert.Equal( EUniverse.Public, sid.AccountUniverse );
            Assert.Equal( EAccountType.Clan, sid.AccountType );
            Assert.Equal( 0u, sid.AccountInstance );
        }

        [Fact]
        public void Steam2RenderIsValid()
        {
            SteamID sid = 76561197969249708;

            Assert.Equal( "STEAM_0:0:4491990", sid.Render() );
            Assert.Equal( sid.Render(), sid.ToString() );

            sid.AccountUniverse = EUniverse.Beta;
            Assert.Equal( "STEAM_2:0:4491990", sid.Render() );

            sid.AccountType = EAccountType.GameServer;
            Assert.Equal( "157625991261918636", sid.Render() );
        }

        [Fact]
        public void SteamIDsEquality()
        {
            SteamID sid = 76561197969249708;
            SteamID sid2 = new SteamID( 76561197969249708 );

            Assert.True( sid == sid2 );
            Assert.True( sid.Equals( sid2 ) );
            Assert.True( sid.Equals( ( object )sid2 ) );

            Assert.False( sid.Equals( new object() ) );
            Assert.False( sid.Equals( ( SteamID )null ) );
            Assert.False( sid.Equals( ( object )null ) );

            SteamID sid3 = 12345;

            Assert.True( sid != sid3 );

            ulong sid4 = sid;
            Assert.True( sid4 == sid );
        }

        [Fact]
        public void SteamIDHashCodeUsesLongHashCode()
        {
            SteamID sid = 172376458626834;
            ulong longValue = 172376458626834;

            Assert.True( sid.GetHashCode() == longValue.GetHashCode() );
        }

        [Fact]
        public void InitializesInstancesCorrectly()
        {
            SteamID sid = new SteamID();

            sid.SetFromSteam3String( "[g:1:1234]" );
            Assert.Equal( EUniverse.Public, sid.AccountUniverse );
            Assert.Equal( EAccountType.Clan, sid.AccountType );
            Assert.Equal( 0u, sid.AccountInstance );
            Assert.Equal( 1234u, sid.AccountID );

            sid.SetFromSteam3String( "[T:1:1234]" );
            Assert.Equal( EUniverse.Public, sid.AccountUniverse );
            Assert.Equal( EAccountType.Chat, sid.AccountType );
            Assert.Equal( 0u, sid.AccountInstance );
            Assert.Equal( 1234u, sid.AccountID );

            sid.SetFromSteam3String( "[c:1:1234]" );
            Assert.Equal( EUniverse.Public, sid.AccountUniverse );
            Assert.Equal( EAccountType.Chat, sid.AccountType );
            Assert.Equal( (uint)SteamID.ChatInstanceFlags.Clan, sid.AccountInstance );
            Assert.Equal( 1234u, sid.AccountID);

            sid.SetFromSteam3String( "[L:1:1234]" );
            Assert.Equal( EUniverse.Public, sid.AccountUniverse );
            Assert.Equal( EAccountType.Chat, sid.AccountType );
            Assert.Equal( ( uint ) SteamID.ChatInstanceFlags.Lobby, sid.AccountInstance );
            Assert.Equal( 1234u, sid.AccountID );
        }

        [Fact]
        public void RendersOutOfRangeAccountTypeAsLowercaseI()
        {
            SteamID sid = new SteamID( 123, EUniverse.Beta, (EAccountType)(-1) );
            Assert.Equal( "[i:2:123]", sid.Render(steam3: true) );
        }
    }
}
