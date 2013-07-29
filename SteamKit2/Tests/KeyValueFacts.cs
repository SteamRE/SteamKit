using Xunit;
using System.Linq;
using SteamKit2;

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
    }
}
