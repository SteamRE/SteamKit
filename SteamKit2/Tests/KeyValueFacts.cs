﻿using System;
using System.IO;
using System.Text;
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
        public void KeyValueIndexerReturnsValidAndInvalid()
        {
            KeyValue kv = new KeyValue();

            kv.Children.Add( new KeyValue( "exists", "value" ) );

            Assert.Equal( "value", kv["exists"].Value );
            Assert.Equal( KeyValue.Invalid, kv["thiskeydoesntexist"] );
        }

        [Fact]
        public void KeyValueIndexerDoesntallowDuplicates()
        {
            KeyValue kv = new KeyValue();

            kv["key"] = new KeyValue();

            Assert.Single( kv.Children );

            kv["key"] = new KeyValue();

            Assert.Single( kv.Children );

            kv["key2"] = new KeyValue();

            Assert.Equal( 2, kv.Children.Count );
        }

        [Fact]
        public void KeyValueIndexerUpdatesKey()
        {
            KeyValue kv = new KeyValue();

            KeyValue subkey = new KeyValue();

            Assert.Null( subkey.Name );

            kv["subkey"] = subkey;

            Assert.Equal( "subkey", subkey.Name );
            Assert.Equal( "subkey", kv["subkey"].Name );
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

            Assert.Single( subKey.Children );

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
        public void KeyValuesWritesBinaryToFile()
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
                var hexValue = BitConverter.ToString( binaryValue ).Replace( "-", "", StringComparison.Ordinal );

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
        public void KeyValuesWritesBinaryToStream()
        {
            var expectedHexValue = "00525000017374617475730023444F54415F52505F424F54505241435449434500016E756D5F706172616D730030000" +
                "17761746368696E675F736572766572005B413A313A323130383933353136393A353431325D00017761746368696E675F66726F6D5F73" +
                "6572766572005B413A313A3836343436383939343A353431325D000808";

            var kv = new KeyValue( "RP" );
            kv.Children.Add( new KeyValue( "status", "#DOTA_RP_BOTPRACTICE" ) );
            kv.Children.Add( new KeyValue( "num_params", "0" ) );
            kv.Children.Add( new KeyValue( "watching_server", "[A:1:2108935169:5412]" ) );
            kv.Children.Add( new KeyValue( "watching_from_server", "[A:1:864468994:5412]" ) );
            
            byte[] binaryValue;
            using ( var ms = new MemoryStream() )
            {
                kv.SaveToStream( ms, asBinary: true );
                binaryValue = ms.ToArray();
            }

            var hexValue = BitConverter.ToString( binaryValue ).Replace( "-", "", StringComparison.Ordinal );

            Assert.Equal( expectedHexValue, hexValue );
        }

        [Fact]
        public void KeyValueBinarySerializationIsSymmetric()
        {
            var kv = new KeyValue( "MessageObject" );
            kv.Children.Add( new KeyValue( "key", "value" ) );

            var deserializedKv = new KeyValue();
            bool loaded;

            using ( var ms = new MemoryStream() )
            {
                kv.SaveToStream( ms, asBinary: true );
                ms.Seek( 0, SeekOrigin.Begin );
                loaded = deserializedKv.TryReadAsBinary( ms );
            }

            Assert.True( loaded );

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
        public void KeyValues_TryReadAsBinary_ReadsBinary()
        {
            var binary = Convert.FromHexString( TestObjectHex );
            var kv = new KeyValue();
            bool success;
            using ( var ms = new MemoryStream( binary ) )
            {
                success = kv.TryReadAsBinary( ms );
                Assert.Equal( ms.Length, ms.Position );
            }

            Assert.True( success, "Should have read test object." );
            Assert.Equal( "TestObject", kv.Name );
            Assert.Single( kv.Children );
            Assert.Equal( "key", kv.Children[0].Name );
            Assert.Equal( "value", kv.Children[0].Value );
        }

        [Fact]
        public void KeyValuesReadsBinaryWithLeftoverData()
        {
            var binary = Convert.FromHexString( TestObjectHex + Guid.NewGuid().ToString().Replace("-", "", StringComparison.Ordinal) );
            var kv = new KeyValue();
            bool success;
            using ( var ms = new MemoryStream( binary ) )
            {
                success = kv.TryReadAsBinary( ms );
                Assert.Equal( TestObjectHex.Length / 2, ms.Position );
                Assert.Equal( 16, ms.Length - ms.Position );
            }

            Assert.True( success, "Should have read test object." );
            Assert.Equal( "TestObject", kv.Name );
            Assert.Single( kv.Children );
            Assert.Equal( "key", kv.Children[0].Name );
            Assert.Equal( "value", kv.Children[0].Value );
        }

        [Fact]
        public void KeyValuesFailsToReadTruncatedBinary()
        {
            // Test every possible truncation boundary we have.
            for ( int i = 0; i < TestObjectHex.Length; i += 2 )
            {
                var binary = Convert.FromHexString( TestObjectHex[ ..i ] );
                var kv = new KeyValue();
                bool success;
                using ( var ms = new MemoryStream( binary ) )
                {
                    success = kv.TryReadAsBinary( ms );
                    Assert.Equal( ms.Length, ms.Position );
                }

                Assert.False( success, "Should not have read test object." );
            }
        }

        [Fact]
        public void KeyValuesReadsBinaryWithMultipleChildren()
        {
            var hex = "00546573744f626a65637400016b6579310076616c75653100016b6579320076616c756532000808";
            var binary = Convert.FromHexString( hex );
            var kv = new KeyValue();
            bool success;
            using ( var ms = new MemoryStream( binary ) )
            {
                success = kv.TryReadAsBinary( ms );
            }

            Assert.True( success );
            
            Assert.Equal( "TestObject", kv.Name );
            Assert.Equal( 2, kv.Children.Count );
            Assert.Equal( "key1", kv.Children[ 0 ].Name );
            Assert.Equal( "value1", kv.Children[ 0 ].Value );
            Assert.Equal( "key2", kv.Children[ 1 ].Name );
            Assert.Equal( "value2", kv.Children[ 1 ].Value );
        }

        [Fact]
        public void KeyValuesSavesTextToFile()
        {
            var expected = "\"RootNode\"\n{\n\t\"key1\"\t\t\"value1\"\n\t\"key2\"\n\t{\n\t\t\"ChildKey\"\t\t\"ChildValue\"\n\t}\n}\n";

            var kv = new KeyValue( "RootNode" )
            {
                Children =
                {
                    new KeyValue( "key1", "value1" ),
                    new KeyValue( "key2" )
                    {
                        Children =
                        {
                            new KeyValue( "ChildKey", "ChildValue" )
                        }
                    }
                }
            };

            string text;
            var temporaryFile = Path.GetTempFileName();
            try
            {
                kv.SaveToFile( temporaryFile, asBinary: false );
                text = File.ReadAllText( temporaryFile );
            }
            finally
            {
                File.Delete( temporaryFile );
            }

            Assert.Equal( expected, text );
        }

        [Fact]
        public void KeyValuesSavesTextToStream()
        {
            var expected = "\"RootNode\"\n{\n\t\"key1\"\t\t\"value1\"\n\t\"key2\"\n\t{\n\t\t\"ChildKey\"\t\t\"ChildValue\"\n\t}\n}\n";
            
            var kv = new KeyValue( "RootNode" )
            {
                Children =
                {
                    new KeyValue( "key1", "value1" ),
                    new KeyValue( "key2" )
                    {
                        Children =
                        {
                            new KeyValue( "ChildKey", "ChildValue" )
                        }
                    }
                }
            };

            var text = SaveToText( kv );

            Assert.Equal( expected, text );
        }

        [Fact]
        public void CanLoadUnicodeTextDocument()
        {
            var expected = "\"RootNode\"\n{\n\t\"key1\"\t\t\"value1\"\n\t\"key2\"\n\t{\n\t\t\"ChildKey\"\t\t\"ChildValue\"\n\t}\n}\n";
            var kv = new KeyValue();

            var temporaryFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText( temporaryFile, expected, Encoding.Unicode );
                kv.ReadFileAsText( temporaryFile );
            }
            finally
            {
                File.Delete( temporaryFile );
            }

            Assert.Equal( "RootNode", kv.Name );
            Assert.Equal( 2, kv.Children.Count );
            Assert.Equal( "key1", kv.Children[ 0 ].Name );
            Assert.Equal( "value1", kv.Children[ 0 ].Value );
            Assert.Equal( "key2", kv.Children[ 1 ].Name );
            Assert.Single( kv.Children[ 1 ].Children);
            Assert.Equal( "ChildKey", kv.Children[ 1 ].Children[ 0 ].Name );
            Assert.Equal( "ChildValue", kv.Children[ 1 ].Children[ 0 ].Value );
        }

        [Fact]
        public void CanLoadUnicodeTextStream()
        {
            var expected = "\"RootNode\"\n{\n\t\"key1\"\t\t\"value1\"\n\t\"key2\"\n\t{\n\t\t\"ChildKey\"\t\t\"ChildValue\"\n\t}\n}\n";
            var kv = new KeyValue();

            var temporaryFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText( temporaryFile, expected, Encoding.Unicode );

                using var fs = File.OpenRead( temporaryFile );
                kv.ReadAsText( fs );
            }
            finally
            {
                File.Delete( temporaryFile );
            }

            Assert.Equal( "RootNode", kv.Name );
            Assert.Equal( 2, kv.Children.Count );
            Assert.Equal( "key1", kv.Children[ 0 ].Name );
            Assert.Equal( "value1", kv.Children[ 0 ].Value );
            Assert.Equal( "key2", kv.Children[ 1 ].Name );
            Assert.Single( kv.Children[ 1 ].Children  );
            Assert.Equal( "ChildKey", kv.Children[ 1 ].Children[ 0 ].Name );
            Assert.Equal( "ChildValue", kv.Children[ 1 ].Children[ 0 ].Value );
        }

#if false
        [Fact]
        public void CanReadAndIgnoreConditionals()
        {
            var text = @"
""Repro""
{
""Conditional""    ""You're not running Windows.""  [$!WIN32]          // DEPRECATED
""EmptyThing""	""""
}
".Trim();

            var kv = new KeyValue();
            using ( var ms = new MemoryStream( Encoding.UTF8.GetBytes( text ) ) )
            {
                kv.ReadAsText( ms );
            }

            Assert.Equal( "Repro", kv.Name );
            Assert.Equal( 2, kv.Children.Count );
            Assert.Equal( "Conditional", kv.Children[ 0 ].Name );
            Assert.Equal( "You're not running Windows.", kv.Children[ 0 ].Value );
            Assert.Equal( "EmptyThing", kv.Children[ 1 ].Name );
            Assert.Equal( "", kv.Children[ 1 ].Value );
        }
#endif

        [Fact]
        public void WritesNewLineAsSlashN()
        {
            var kv = new KeyValue( "abc" );
            kv.Children.Add( new KeyValue( "def", "ghi\njkl" ) );
            var text = SaveToText( kv );
            var expected = ( @"
""abc""
{
" + "\t" + @"""def""		""ghi\njkl""
}
" ).Trim().Replace( "\r\n", "\n" ) + "\n";

            Assert.Equal( expected, text );
        }

        [Fact]
        public void KeyValuesUnsignedByteConversion()
        {
            byte expectedValue = 37;

            var kv = new KeyValue( "key", "37" );
            Assert.Equal( expectedValue, kv.AsUnsignedByte() );

            kv.Value = "256";
            Assert.Equal( expectedValue, kv.AsUnsignedByte(expectedValue) );
        }

        [Fact]
        public void KeyValuesUnsignedShortConversion()
        {
            ushort expectedValue = 1337;

            var kv = new KeyValue( "key", "1337" );
            Assert.Equal( expectedValue, kv.AsUnsignedShort() );

            kv.Value = "123456";
            Assert.Equal( expectedValue, kv.AsUnsignedShort(expectedValue) );
        }

        [Fact]
        public void KeyValuesEscapesTextWhenSerializing()
        {
            var kv = new KeyValue( "key" );
            kv.Children.Add( new KeyValue( "slashes", @"\o/" ) );
            kv.Children.Add( new KeyValue( "newline", "\r\n" ) );

            var text = SaveToText( kv );

            var expectedValue = "\"key\"\n{\n\t\"slashes\"\t\t\"\\\\o/\"\n\t\"newline\"\t\t\"\\r\\n\"\n}\n";
            Assert.Equal( expectedValue, text );
        }

        [Fact]
        public void KeyValuesTextPreserveEmptyObjects()
        {
            var kv = new KeyValue( "key" );
            kv.Children.Add( new KeyValue( "emptyObj" ) );
            kv.Children.Add( new KeyValue( "emptyString", string.Empty ) );

            var text = SaveToText( kv );

            var expectedValue = "\"key\"\n{\n\t\"emptyObj\"\n\t{\n\t}\n\t\"emptyString\"\t\t\"\"\n}\n";
            Assert.Equal( expectedValue, text );
        }

        [Fact]
        public void KeyValuesBinaryPreserveEmptyObjects()
        {
            var expectedHexString = "006B65790000656D7074794F626A000801656D707479537472696E6700000808";

            var kv = new KeyValue( "key" );
            kv.Children.Add( new KeyValue( "emptyObj" ) );
            kv.Children.Add( new KeyValue( "emptyString", string.Empty ) );
            
            var deserializedKv = new KeyValue();
            byte[] binaryValue;
            using ( var ms = new MemoryStream() )
            {
                kv.SaveToStream( ms, asBinary: true );
                ms.Seek( 0, SeekOrigin.Begin );
                binaryValue = ms.ToArray();
                deserializedKv.TryReadAsBinary( ms );
            }

            var hexValue = BitConverter.ToString( binaryValue ).Replace( "-", "", StringComparison.Ordinal );

            Assert.Equal( expectedHexString, hexValue );
            Assert.Null( deserializedKv["emptyObj"].Value );
            Assert.Empty( deserializedKv["emptyObj"].Children );
            Assert.Equal( string.Empty, deserializedKv["emptyString"].Value );
        }

        [Fact]
        public void DecodesBinaryWithFieldType10()
        {
            var hex = "00546573744F626A656374000A6B65790001020304050607080808";
            var binary = Convert.FromHexString( hex );
            var kv = new KeyValue();
            using (var ms = new MemoryStream(binary))
            {
                var read = kv.TryReadAsBinary(ms);
                Assert.True(read);
            }

            Assert.Equal( 0x0807060504030201, kv["key"].AsLong() );
        }

        [Fact]
        public void DecodesBinaryWithAlternateEnd()
        {
            var hex = "00546573744F626A656374000A6B65790001020304050607080B0B";
            var binary = Convert.FromHexString( hex );
            var kv = new KeyValue();
            using ( var ms = new MemoryStream( binary ) )
            {
                var read = kv.TryReadAsBinary( ms );
                Assert.True( read );
            }

            Assert.Equal( 0x0807060504030201, kv[ "key" ].AsLong() );
        }

        static string SaveToText( KeyValue kv )
        {
            string text;
            using ( var ms = new MemoryStream() )
            {
                kv.SaveToStream( ms, asBinary: false );
                ms.Seek( 0, SeekOrigin.Begin );
                using var reader = new StreamReader( ms );
                text = reader.ReadToEnd();
            }

            return text;
        }

        const string TestObjectHex = "00546573744F626A65637400016B65790076616C7565000808";
    }
}
