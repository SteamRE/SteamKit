using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;

namespace Tests;

public class ApiSurfaceFacts
{
    [Fact]
    public void ApiSurfaceIsWellKnown()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream( "Tests.Files.apisurface.txt" );
        using var ms = new MemoryStream();
        stream.CopyTo( ms );
        var expected = Encoding.UTF8.GetString( ms.ToArray() ).ReplaceLineEndings( "\n" );

        var actual = GenerateApiSurface( typeof( SteamKit2.SteamClient ).GetTypeInfo().Assembly );

        File.WriteAllText( Path.Join( AppDomain.CurrentDomain.BaseDirectory, "apisurface.txt" ), actual );

        Assert.Equal( expected, actual );
    }

    static string GenerateApiSurface( Assembly assembly )
    {
        var sb = new StringBuilder();

        var publicTypes = assembly.GetTypes()
            .Where( t => t.GetTypeInfo().IsPublic )
            .Where( t => !IsInternalSteamKit( t.Namespace ) )
            .Where( t => !t.IsEnum ) // Ignore enums as it would be noisy because they are generated
            .OrderBy( t => t.Namespace, StringComparer.InvariantCulture )
            .ThenBy( t => t.Name, StringComparer.InvariantCulture );

        foreach ( var type in publicTypes )
        {
            GenerateTypeApiSurface( sb, type );
        }

        return sb.ToString();
    }

    static bool IsInternalSteamKit( string ns )
    {
        if ( !ns.StartsWith( "SteamKit2.", StringComparison.Ordinal ) )
        {
            return false;
        }

        if ( ns.EndsWith( ".Internal", StringComparison.Ordinal ) )
        {
            return true;
        }

        return ns.Contains( ".Internal.", StringComparison.Ordinal );
    }

    static void GenerateTypeApiSurface( StringBuilder sb, Type type )
    {
        var typeInfo = type.GetTypeInfo();

        sb.Append( "public " );

        if ( typeInfo.IsSealed )
        {
            sb.Append( "sealed " );
        }

        if ( typeInfo.IsClass )
        {
            sb.Append( "class" );
        }
        else if ( typeInfo.IsInterface )
        {
            sb.Append( "interface" );
        }
        else if ( typeInfo.IsEnum )
        {
            sb.Append( "enum" );
        }
        else
        {
            sb.Append( "struct" );
        }

        sb.Append( ' ' );
        sb.Append( GetTypeAsString( type ) );
        sb.Append( "\n{\n" );

        if ( typeInfo.IsEnum )
        {
            var members = Enum.GetNames( type );
            foreach ( var member in members )
            {
                var rawValue = type.GetField( member, BindingFlags.Public | BindingFlags.Static ).GetValue( null );
                var convertedValue = Convert.ChangeType( rawValue, Enum.GetUnderlyingType( type ) );

                sb.Append( "    " );
                sb.Append( member );
                sb.Append( " = " );
                sb.Append( convertedValue );
                sb.Append( ";\n" );
            }

            sb.Append( '\n' );
        }

        var constructors = type
            .GetConstructors( BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )
            .Where( t => !t.IsPrivate && !t.IsAssembly && !t.IsFamilyAndAssembly )
            .OrderBy( t => t.Name, StringComparer.InvariantCulture )
            .ThenBy( t => string.Join( ", ", t.GetParameters().Select( GetParameterAsString ) ), StringComparer.InvariantCulture );

        foreach ( var constructor in constructors )
        {
            sb.Append( "    " );

            if ( constructor.IsPublic )
            {
                sb.Append( "public" );
            }
            else
            {
                sb.Append( "protected" );
            }

            if ( constructor.IsStatic )
            {
                sb.Append( " static" );
            }

            sb.Append( ' ' );
            sb.Append( constructor.Name );
            sb.Append( '(' );
            sb.Append( string.Join( ", ", constructor.GetParameters().Select( GetParameterAsString ) ) );
            sb.Append( ");\n" );
        }

        var methods = type
            .GetMethods( BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )
            .Where( t => !t.IsPrivate && !t.IsAssembly && !t.IsFamilyAndAssembly )
            .OrderBy( t => t.Name, StringComparer.InvariantCulture )
            .ThenBy( t => string.Join( ", ", t.GetParameters().Select( GetParameterAsString ) ), StringComparer.InvariantCulture );

        foreach ( var method in methods )
        {
            sb.Append( "    " );

            if ( method.IsPublic )
            {
                sb.Append( "public" );
            }
            else
            {
                sb.Append( "protected" );
            }

            if ( IsHidingMember( method ) )
            {
                sb.Append( " new" );
            }

            if ( method.IsStatic )
            {
                sb.Append( " static" );
            }

            sb.Append( ' ' );
            sb.Append( GetTypeAsString( method.ReturnType ) );
            sb.Append( ' ' );
            sb.Append( method.Name );

            if ( method.IsGenericMethodDefinition )
            {
                sb.Append( '<' );
                sb.Append( string.Join( ", ", method.GetGenericArguments().Select( GetTypeAsString ) ) );
                sb.Append( '>' );
            }

            sb.Append( '(' );

            sb.Append( string.Join( ", ", method.GetParameters().Select( GetParameterAsString ) ) );

            sb.Append( ");\n" );
        }

        sb.Append( "}\n\n" );
    }

    static bool IsHidingMember( MethodInfo method )
    {
        var baseType = method.DeclaringType.GetTypeInfo().BaseType;
        if ( baseType == null )
        {
            return false;
        }

        var baseMethods = baseType.GetMethods( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );

        foreach ( var baseMethod in baseMethods )
        {
            if ( baseMethod.Name != method.Name )
            {
                continue;
            }

            if ( baseMethod.DeclaringType == method.DeclaringType )
            {
                continue;
            }

            var methodDefinition = method.GetBaseDefinition();
            var baseMethodDefinition = baseMethod.GetBaseDefinition();

            if ( methodDefinition.DeclaringType == baseMethodDefinition.DeclaringType )
            {
                continue;
            }

            var methodParameters = method.GetParameters();
            var baseMethodParameters = baseMethod.GetParameters();
            if ( methodParameters.Length != baseMethodParameters.Length )
            {
                continue;
            }

            var hasMatchingParameters = true;

            for ( int i = 0; i < methodParameters.Length; i++ )
            {
                if ( methodParameters[ i ].ParameterType != baseMethodParameters[ i ].ParameterType )
                {
                    hasMatchingParameters = false;
                    break;
                }
            }

            if ( !hasMatchingParameters )
            {
                continue;
            }

            return true;
        }

        return false;
    }

    static string GetTypeAsString( Type type )
    {
        if ( type.IsArray )
        {
            var elementType = type.GetElementType();
            var elementTypeAsString = GetTypeAsString( elementType );
            return string.Format( CultureInfo.InvariantCulture, "{0}[]", elementTypeAsString );
        }

        if ( type == typeof( bool ) )
        {
            return "bool";
        }
        else if ( type == typeof( byte ) )
        {
            return "byte";
        }
        else if ( type == typeof( char ) )
        {
            return "char";
        }
        else if ( type == typeof( decimal ) )
        {
            return "decimal";
        }
        else if ( type == typeof( double ) )
        {
            return "double";
        }
        else if ( type == typeof( float ) )
        {
            return "float";
        }
        else if ( type == typeof( int ) )
        {
            return "int";
        }
        else if ( type == typeof( long ) )
        {
            return "long";
        }
        else if ( type == typeof( object ) )
        {
            return "object";
        }
        else if ( type == typeof( sbyte ) )
        {
            return "sbyte";
        }
        else if ( type == typeof( short ) )
        {
            return "short";
        }
        else if ( type == typeof( string ) )
        {
            return "string";
        }
        else if ( type == typeof( uint ) )
        {
            return "uint";
        }
        else if ( type == typeof( ulong ) )
        {
            return "ulong";
        }
        else if ( type == typeof( ushort ) )
        {
            return "ushort";
        }
        else if ( type == typeof( void ) )
        {
            return "void";
        }

        var sb = new StringBuilder();

        if ( type.Namespace != "System" )
        {
            sb.Append( type.Namespace );
            sb.Append( '.' );
        }

        sb.Append( type.Name );

        if ( type.IsConstructedGenericType )
        {
            sb.Append( "[[" );
            sb.Append( string.Join( ", ", type.GetGenericArguments().Select( GetTypeAsString ) ) );
            sb.Append( "]]" );
        }

        return sb.ToString();
    }

    static string GetParameterAsString( ParameterInfo parameter )
    {
        var sb = new StringBuilder();

        if ( parameter.IsOut )
        {
            sb.Append( "out " );
        }
        else if ( parameter.ParameterType.IsByRef )
        {
            sb.Append( "ref " );
        }

        sb.Append( GetTypeAsString( parameter.ParameterType ) );
        sb.Append( ' ' );
        sb.Append( parameter.Name );

        return sb.ToString();
    }
}
