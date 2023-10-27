using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteamKit2;

namespace Tests
{
    [TestClass]
    public class KeyValueFacts
    {
        [TestMethod]
        public void KeyValueInitializesCorrectly()
        {
            KeyValue kv = new KeyValue( "name", "value" );

            Assert.AreEqual( "name", kv.Name );
            Assert.AreEqual( "value", kv.Value );

            Assert.IsTrue( kv.Children.Count == 0 );
        }

        [TestMethod]
        public void KeyValueIndexerReturnsValidAndInvalid()
        {
            KeyValue kv = new KeyValue();

            kv.Children.Add( new KeyValue( "exists", "value" ) );

            Assert.AreEqual( "value", kv["exists"].Value );
            Assert.AreEqual( KeyValue.Invalid, kv["thiskeydoesntexist"] );
        }

        [TestMethod]
        public void KeyValueIndexerDoesntallowDuplicates()
        {
            KeyValue kv = new KeyValue();

            kv["key"] = new KeyValue();

            Assert.IsTrue( kv.Children.Count == 1 );

            kv["key"] = new KeyValue();

            Assert.IsTrue( kv.Children.Count == 1 );

            kv["key2"] = new KeyValue();

            Assert.AreEqual( 2, kv.Children.Count );
        }

        [TestMethod]
        public void KeyValueIndexerUpdatesKey()
        {
            KeyValue kv = new KeyValue();

            KeyValue subkey = new KeyValue();

            Assert.IsNull( subkey.Name );

            kv["subkey"] = subkey;

            Assert.AreEqual( "subkey", subkey.Name );
            Assert.AreEqual( "subkey", kv["subkey"].Name );
        }

        [TestMethod]
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

            Assert.AreEqual( "root", kv.Name );
            Assert.IsNull( kv.Value );

            Assert.AreEqual( "name", kv[ "name" ].Name );
            Assert.AreEqual( "value", kv[ "name" ].Value );
            Assert.IsTrue( kv[ "name" ].Children.Count == 0 );

            KeyValue subKey = kv[ "subkey" ];

            Assert.IsTrue( subKey.Children.Count == 1 );

            Assert.AreEqual( "name2", subKey[ "name2" ].Name );
            Assert.AreEqual( "value2", subKey[ "name2" ].Value );
            Assert.IsTrue( subKey[ "name2" ].Children.Count == 0 );
        }

        [TestMethod]
        public void KeyValuesMissingKeysGiveInvalid()
        {
            KeyValue kv = new KeyValue();

            Assert.AreSame( KeyValue.Invalid, kv[ "missingkey" ] );
        }

        [TestMethod]
        public void KeyValuesKeysAreCaseInsensitive()
        {
            KeyValue kv = KeyValue.LoadFromString(
                @"""root""
                  {
                      ""name"" ""value""
                  }" );

            Assert.AreEqual( "value", kv[ "name" ].Value );
            Assert.AreEqual( "value", kv[ "NAME" ].Value );
            Assert.AreEqual( "value", kv[ "NAme" ].Value );
        }

        [TestMethod]
        public void KeyValuesHandlesBool()
        {
            KeyValue kv = KeyValue.LoadFromString(
                @"""root""
                  {
                      ""name"" ""1""
                  }" );

            Assert.IsTrue( kv[ "name" ].AsBoolean() );

            kv[ "name" ].Value = "0";
            Assert.IsFalse( kv[ "name" ].AsBoolean() );

            kv[ "name" ].Value = "100";
            Assert.IsTrue( kv[ "name" ].AsBoolean(), "values other than 0 are truthy" );

            kv[ "name" ].Value = "invalidbool";
            Assert.IsFalse( kv[ "name" ].AsBoolean(), "values that cannot be converted to integers are falsey" );
        }

        [TestMethod]
        public void KeyValuesHandlesFloat()
        {
            KeyValue kv = KeyValue.LoadFromString(
                @"""root""
                  {
                      ""name"" ""123.456""
                  }" );

            Assert.AreEqual( 123.456f, kv[ "name" ].AsFloat() );

            kv[ "name" ].Value = "invalidfloat";
            Assert.AreEqual( 321.654f, kv[ "name" ].AsFloat( 321.654f ) ); // invalid parse returns the default
        }

        [TestMethod]
        public void KeyValuesHandlesInt()
        {
            KeyValue kv = KeyValue.LoadFromString(
                @"""root""
                  {
                      ""name"" ""123""
                  }" );

            Assert.AreEqual( 123, kv[ "name" ].AsInteger() );

            kv[ "name" ].Value = "invalidint";
            Assert.AreEqual( 987, kv[ "name" ].AsInteger( 987 ) ); // invalid parse returns the default
        }

        [TestMethod]
        public void KeyValuesHandlesLong()
        {
            KeyValue kv = KeyValue.LoadFromString(
                @"""root""
                  {
                      ""name"" ""-5001050759734897745""
                  }" );

            Assert.AreEqual( -5001050759734897745, kv[ "name" ].AsLong() );

            kv[ "name" ].Value = "invalidlong";
            Assert.AreEqual( 678, kv[ "name" ].AsLong( 678 ) );  // invalid parse returns the default
        }

        [TestMethod]
        public void KeyValuesHandlesString()
        {
            KeyValue kv = KeyValue.LoadFromString(
                @"""root""
                  {
                      ""name"" ""stringvalue""
                  }" );

            Assert.AreEqual( "stringvalue", kv[ "name" ].AsString() );
            Assert.AreEqual( "stringvalue", kv[ "name" ].Value );
        }

        [TestMethod]
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
                var hexValue = BitConverter.ToString( binaryValue ).Replace( "-", "" );

                Assert.AreEqual( expectedHexValue, hexValue );
            }
            finally
            {
                if ( tempFileName != null && File.Exists( tempFileName ) )
                {
                    File.Delete( tempFileName );
                }
            }
        }

        [TestMethod]
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

            var hexValue = BitConverter.ToString( binaryValue ).Replace( "-", "" );

            Assert.AreEqual( expectedHexValue, hexValue );
        }

        [TestMethod]
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

            Assert.IsTrue( loaded );

            Assert.AreEqual( kv.Name, deserializedKv.Name );
            Assert.AreEqual( kv.Children.Count, deserializedKv.Children.Count );

            for ( int i = 0; i < kv.Children.Count; i++ )
            {
                var originalChild = kv.Children[ i ];
                var deserializedChild = deserializedKv.Children[ i ];
                
                Assert.AreEqual( originalChild.Name, deserializedChild.Name );
                Assert.AreEqual( originalChild.Value, deserializedChild.Value );
            }
        }

        [TestMethod]
        public void KeyValues_TryReadAsBinary_ReadsBinary()
        {
            var binary = Utils.DecodeHexString( TestObjectHex );
            var kv = new KeyValue();
            bool success;
            using ( var ms = new MemoryStream( binary ) )
            {
                success = kv.TryReadAsBinary( ms );
                Assert.AreEqual( ms.Length, ms.Position );
            }

            Assert.IsTrue( success, "Should have read test object." );
            Assert.AreEqual( "TestObject", kv.Name );
            Assert.IsTrue( kv.Children.Count == 1 );
            Assert.AreEqual( "key", kv.Children[0].Name );
            Assert.AreEqual( "value", kv.Children[0].Value );
        }

        [TestMethod]
        public void KeyValuesReadsBinaryWithLeftoverData()
        {
            var binary = Utils.DecodeHexString( TestObjectHex + Guid.NewGuid().ToString().Replace("-", "") );
            var kv = new KeyValue();
            bool success;
            using ( var ms = new MemoryStream( binary ) )
            {
                success = kv.TryReadAsBinary( ms );
                Assert.AreEqual( TestObjectHex.Length / 2, ms.Position );
                Assert.AreEqual( 16, ms.Length - ms.Position );
            }

            Assert.IsTrue( success, "Should have read test object." );
            Assert.AreEqual( "TestObject", kv.Name );
            Assert.IsTrue( kv.Children.Count == 1 );
            Assert.AreEqual( "key", kv.Children[0].Name );
            Assert.AreEqual( "value", kv.Children[0].Value );
        }

        [TestMethod]
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
                    success = kv.TryReadAsBinary( ms );
                    Assert.AreEqual( ms.Length, ms.Position );
                }

                Assert.IsFalse( success, "Should not have read test object." );
            }
        }

        [TestMethod]
        public void KeyValuesReadsBinaryWithMultipleChildren()
        {
            var hex = "00546573744f626a65637400016b6579310076616c75653100016b6579320076616c756532000808";
            var binary = Utils.DecodeHexString( hex );
            var kv = new KeyValue();
            bool success;
            using ( var ms = new MemoryStream( binary ) )
            {
                success = kv.TryReadAsBinary( ms );
            }

            Assert.IsTrue( success );
            
            Assert.AreEqual( "TestObject", kv.Name );
            Assert.AreEqual( 2, kv.Children.Count );
            Assert.AreEqual( "key1", kv.Children[ 0 ].Name );
            Assert.AreEqual( "value1", kv.Children[ 0 ].Value );
            Assert.AreEqual( "key2", kv.Children[ 1 ].Name );
            Assert.AreEqual( "value2", kv.Children[ 1 ].Value );
        }

        [TestMethod]
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

            Assert.AreEqual( expected, text );
        }

        [TestMethod]
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

            string text;
            using ( var ms = new MemoryStream() )
            {
                kv.SaveToStream( ms, asBinary: false );
                ms.Seek( 0, SeekOrigin.Begin );
                using ( var reader = new StreamReader( ms ) )
                {
                    text = reader.ReadToEnd();
                }
            }

            Assert.AreEqual( expected, text );
        }

        [TestMethod]
        public void KeyValuesUnsignedByteConversion()
        {
            byte expectedValue = 37;

            var kv = new KeyValue( "key", "37" );
            Assert.AreEqual( expectedValue, kv.AsUnsignedByte() );

            kv.Value = "256";
            Assert.AreEqual( expectedValue, kv.AsUnsignedByte(expectedValue) );
        }

        [TestMethod]
        public void KeyValuesUnsignedShortConversion()
        {
            ushort expectedValue = 1337;

            var kv = new KeyValue( "key", "1337" );
            Assert.AreEqual( expectedValue, kv.AsUnsignedShort() );

            kv.Value = "123456";
            Assert.AreEqual( expectedValue, kv.AsUnsignedShort(expectedValue) );
        }

        [TestMethod]
        public void KeyValuesEscapesTextWhenSerializing()
        {
            var kv = new KeyValue( "key" );
            kv.Children.Add( new KeyValue( "slashes", @"\o/" ) );
            kv.Children.Add( new KeyValue( "newline", "\r\n" ) );

            string text;
            using ( var ms = new MemoryStream() )
            {
                kv.SaveToStream( ms, asBinary: false );
                ms.Seek( 0, SeekOrigin.Begin );
                using ( var reader = new StreamReader( ms ) )
                {
                    text = reader.ReadToEnd();
                }
            }

            var expectedValue = "\"key\"\n{\n\t\"slashes\"\t\t\"\\\\o/\"\n\t\"newline\"\t\t\"\\r\\n\"\n}\n";
            Assert.AreEqual( expectedValue, text );
        }

        [TestMethod]
        public void KeyValuesTextPreserveEmptyObjects()
        {
            var kv = new KeyValue( "key" );
            kv.Children.Add( new KeyValue( "emptyObj" ) );
            kv.Children.Add( new KeyValue( "emptyString", string.Empty ) );

            string text;
            using ( var ms = new MemoryStream() )
            {
                kv.SaveToStream( ms, asBinary: false );
                ms.Seek( 0, SeekOrigin.Begin );
                using ( var reader = new StreamReader( ms ) )
                {
                    text = reader.ReadToEnd();
                }
            }

            var expectedValue = "\"key\"\n{\n\t\"emptyObj\"\n\t{\n\t}\n\t\"emptyString\"\t\t\"\"\n}\n";
            Assert.AreEqual( expectedValue, text );
        }

        [TestMethod]
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

            var hexValue = BitConverter.ToString( binaryValue ).Replace( "-", "" );

            Assert.AreEqual( expectedHexString, hexValue );
            Assert.IsNull( deserializedKv["emptyObj"].Value );
            Assert.IsTrue( deserializedKv[ "emptyObj" ].Children.Count == 0 );
            Assert.AreEqual( string.Empty, deserializedKv["emptyString"].Value );
        }

        [TestMethod]
        public void DecodesBinaryWithFieldType10()
        {
            var hex = "00546573744F626A656374000A6B65790001020304050607080808";
            var binary = Utils.DecodeHexString( hex );
            var kv = new KeyValue();
            using (var ms = new MemoryStream(binary))
            {
                var read = kv.TryReadAsBinary(ms);
                Assert.IsTrue(read);
            }

            Assert.AreEqual( 0x0807060504030201, kv["key"].AsLong() );
        }

        const string TestObjectHex = "00546573744F626A65637400016B65790076616C7565000808";
    }
}
