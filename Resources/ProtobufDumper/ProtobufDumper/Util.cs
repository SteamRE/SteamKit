using System;
using System.Globalization;
using System.Text;

namespace ProtobufDumper
{
    static class Util
    {
        public static string ToLiteral( string input )
        {
            StringBuilder literal = new StringBuilder( input.Length + 2 );
            literal.Append( '"' );
            foreach ( var c in input )
            {
                switch ( c )
                {
                    case '\a':
                        literal.Append( @"\a" );
                        break;
                    case '\b':
                        literal.Append( @"\b" );
                        break;
                    case '\f':
                        literal.Append( @"\f" );
                        break;
                    case '\n':
                        literal.Append( @"\n" );
                        break;
                    case '\r':
                        literal.Append( @"\r" );
                        break;
                    case '\t':
                        literal.Append( @"\t" );
                        break;
                    case '\v':
                        literal.Append( @"\v" );
                        break;
                    case '\\':
                        literal.Append( @"\\" );
                        break;
                    case '\"':
                        literal.Append( "\\\"" );
                        break;
                    default:
                        if ( c < 0x20 || c == 0x7F )
                        {
                            AppendOctal( literal, c );
                        }
                        else
                        {
                            literal.Append( c );
                        }
                        break;
                }
            }

            literal.Append( '"' );
            return literal.ToString();
        }

        public static string ToLiteral( byte[] input )
        {
            StringBuilder literal = new StringBuilder( input.Length + 2 );
            literal.Append( '"' );
            foreach ( var b in input )
            {
                if ( b == '\\' || b == '"' )
                {
                    literal.Append( '\\' );
                    literal.Append( ( char )b );
                }
                else if ( b < 0x20 || b >= 0x7F )
                {
                    AppendOctal( literal, b );
                }
                else
                {
                    literal.Append( ( char )b );
                }
            }

            literal.Append( '"' );
            return literal.ToString();
        }

        static void AppendOctal( StringBuilder literal, int c )
        {
            literal.Append( '\\' );
            literal.Append( Convert.ToString( c, 8 ).PadLeft( 3, '0' ) );
        }

        public static string ToLiteral( double value )
        {
            if ( double.IsPositiveInfinity( value ) )
                return "inf";
            if ( double.IsNegativeInfinity( value ) )
                return "-inf";
            if ( double.IsNaN( value ) )
                return "nan";

            return Convert.ToString( value, CultureInfo.InvariantCulture );
        }

        public static string ToLiteral( float value )
        {
            if ( !float.IsFinite( value ) )
                return ToLiteral( ( double )value );

            return Convert.ToString( value, CultureInfo.InvariantCulture );
        }
    }
}
