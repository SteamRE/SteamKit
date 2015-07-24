using System;
using System.IO;
using SteamKit2;
using Xunit;

namespace Tests
{
    public class KeyValueFacts
    {
        [Fact]
        public void KeyValueInitializesCorrectly()
        {
            KeyValue kv = new KeyValue( "name", "value" );

            Assert.Equal( "name", kv.Name );
            Assert.Equal( "value", kv.Value );

            Assert.Empty( kv.Children );
        }

        [Fact]
        public void KeyValueLoadsFromString()
        {
            KeyValue kv = KeyValue.LoadFromString(
                @"""root"" 
                  {
                      ""name"" ""value""
                      ""subkey""
                      {
                          ""name2"" ""value2""
                      }
                  }" );

            Assert.Equal( "root", kv.Name );
            Assert.Null( kv.Value );

            Assert.Equal( "name", kv[ "name" ].Name );
            Assert.Equal( "value", kv[ "name" ].Value );
            Assert.Empty( kv[ "name" ].Children );

            KeyValue subKey = kv[ "subkey" ];

            Assert.Equal( 1, subKey.Children.Count );

            Assert.Equal( "name2", subKey[ "name2" ].Name );
            Assert.Equal( "value2", subKey[ "name2" ].Value );
            Assert.Empty( subKey[ "name2" ].Children );
        }

        [Fact]
        public void KeyValuesMissingKeysGiveInvalid()
        {
            KeyValue kv = new KeyValue();

            Assert.Same( KeyValue.Invalid, kv[ "missingkey" ] );
        }

        [Fact]
        public void KeyValuesKeysAreCaseInsensitive()
        {
            KeyValue kv = KeyValue.LoadFromString(
                @"""root""
                  {
                      ""name"" ""value""
                  }" );

            Assert.Equal( "value", kv[ "name" ].Value );
            Assert.Equal( "value", kv[ "NAME" ].Value );
            Assert.Equal( "value", kv[ "NAme" ].Value );
        }

        [Fact]
        public void KeyValuesHandlesBool()
        {
            KeyValue kv = KeyValue.LoadFromString(
                @"""root""
                  {
                      ""name"" ""1""
                  }" );

            Assert.True( kv[ "name" ].AsBoolean() );

            kv[ "name" ].Value = "0";
            Assert.False( kv[ "name" ].AsBoolean() );

            kv[ "name" ].Value = "100";
            Assert.True( kv[ "name" ].AsBoolean(), "values other than 0 are truthy" );

            kv[ "name" ].Value = "invalidbool";
            Assert.False( kv[ "name" ].AsBoolean(), "values that cannot be converted to integers are falsey" );
        }

        [Fact]
        public void KeyValuesHandlesFloat()
        {
            KeyValue kv = KeyValue.LoadFromString(
                @"""root""
                  {
                      ""name"" ""123.456""
                  }" );

            Assert.Equal( 123.456f, kv[ "name" ].AsFloat() );

            kv[ "name" ].Value = "invalidfloat";
            Assert.Equal( 321.654f, kv[ "name" ].AsFloat( 321.654f ) ); // invalid parse returns the default
        }

        [Fact]
        public void KeyValuesHandlesInt()
        {
            KeyValue kv = KeyValue.LoadFromString(
                @"""root""
                  {
                      ""name"" ""123""
                  }" );

            Assert.Equal( 123, kv[ "name" ].AsInteger() );

            kv[ "name" ].Value = "invalidint";
            Assert.Equal( 987, kv[ "name" ].AsInteger( 987 ) ); // invalid parse returns the default
        }

        [Fact]
        public void KeyValuesHandlesLong()
        {
            KeyValue kv = KeyValue.LoadFromString(
                @"""root""
                  {
                      ""name"" ""-5001050759734897745""
                  }" );

            Assert.Equal( -5001050759734897745, kv[ "name" ].AsLong() );

            kv[ "name" ].Value = "invalidlong";
            Assert.Equal( 678, kv[ "name" ].AsLong( 678 ) );  // invalid parse returns the default
        }

        [Fact]
        public void KeyValuesHandlesString()
        {
            KeyValue kv = KeyValue.LoadFromString(
                @"""root""
                  {
                      ""name"" ""stringvalue""
                  }" );

            Assert.Equal( "stringvalue", kv[ "name" ].AsString() );
            Assert.Equal( "stringvalue", kv[ "name" ].Value );
        }

        [Fact]
        public void KeyValuesWritesBinary()
        {
            var expectedHexValue = "00525000017374617475730023444F54415F52505F424F54505241435449434500016E756D5F706172616D730030000" +
                "17761746368696E675F736572766572005B413A313A323130383933353136393A353431325D00017761746368696E675F66726F6D5F73" +
                "6572766572005B413A313A3836343436383939343A353431325D000808";

            var kv = new KeyValue( "RP" );
            kv.Children.Add( new KeyValue( "status", "#DOTA_RP_BOTPRACTICE" ) );
            kv.Children.Add( new KeyValue( "num_params", "0" ) );
            kv.Children.Add( new KeyValue( "watching_server", "[A:1:2108935169:5412]" ) );
            kv.Children.Add( new KeyValue( "watching_from_server", "[A:1:864468994:5412]" ) );

            string tempFileName = null;
            try
            {
                tempFileName = Path.GetTempFileName();

                kv.SaveToFile( tempFileName, asBinary: true );

                var binaryValue = File.ReadAllBytes( tempFileName );
                var hexValue = BitConverter.ToString( binaryValue ).Replace( "-", "" );

                Assert.Equal( expectedHexValue, hexValue );
            }
            finally
            {
                if ( tempFileName != null && File.Exists( tempFileName ) )
                {
                    File.Delete( tempFileName );
                }
            }
        }

        [Fact]
        public void KeyValueBinarySerializationIsSymmetric()
        {
            var kv = new KeyValue( "MessageObject" );
            kv.Children.Add( new KeyValue( "key", "value" ) );

            KeyValue deserializedKv;

            var temporaryFile = Path.GetTempFileName();
            try
            {
                kv.SaveToFile( temporaryFile, asBinary: true );
                deserializedKv = KeyValue.LoadAsBinary( temporaryFile );
            }
            finally
            {
                File.Delete( temporaryFile );
            }

            Assert.Equal( kv.Name, deserializedKv.Name );
            Assert.Equal( kv.Children.Count, deserializedKv.Children.Count );

            for ( int i = 0; i < kv.Children.Count; i++ )
            {
                var originalChild = kv.Children[ i ];
                var deserializedChild = deserializedKv.Children[ i ];
                
                Assert.Equal( originalChild.Name, deserializedChild.Name );
                Assert.Equal( originalChild.Value, deserializedChild.Value );
            }
        }

        [Fact]
        public void KeyValuesReadsBinary()
        {
            var binary = Utils.DecodeHexString( TestObjectHex );
            var kv = new KeyValue();
            bool success;
            using ( var ms = new MemoryStream( binary ) )
            {
                success = kv.ReadAsBinary( ms );
                Assert.Equal( ms.Length, ms.Position );
            }

            Assert.True( success, "Should have read test object." );
            Assert.Equal( "TestObject", kv.Name );
            Assert.Equal( 1, kv.Children.Count );
            Assert.Equal( "key", kv.Children[0].Name );
            Assert.Equal( "value", kv.Children[0].Value );
        }

        [Fact]
        public void KeyValuesReadsBinaryWithLeftoverData()
        {
            var binary = Utils.DecodeHexString( TestObjectHex + Guid.NewGuid().ToString().Replace("-", "") );
            var kv = new KeyValue();
            bool success;
            using ( var ms = new MemoryStream( binary ) )
            {
                success = kv.ReadAsBinary( ms );
                Assert.Equal( TestObjectHex.Length / 2, ms.Position );
                Assert.Equal( 16, ms.Length - ms.Position );
            }

            Assert.True( success, "Should have read test object." );
            Assert.Equal( "TestObject", kv.Name );
            Assert.Equal( 1, kv.Children.Count );
            Assert.Equal( "key", kv.Children[0].Name );
            Assert.Equal( "value", kv.Children[0].Value );
        }

        [Fact]
        public void KeyValuesFailsToReadTruncatedBinary()
        {
            // Test every possible truncation boundary we have.
            for ( int i = 0; i < TestObjectHex.Length; i += 2 )
            {
                var binary = Utils.DecodeHexString( TestObjectHex.Substring( 0, i ) );
                var kv = new KeyValue();
                bool success;
                using ( var ms = new MemoryStream( binary ) )
                {
                    success = kv.ReadAsBinary( ms );
                    Assert.Equal( ms.Length, ms.Position );
                }

                Assert.False( success, "Should not have read test object." );
            }
        }


        const string TestObjectHex = "00546573744F626A65637400016B65790076616C7565000808";
    }
}
