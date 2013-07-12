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
    }
}
