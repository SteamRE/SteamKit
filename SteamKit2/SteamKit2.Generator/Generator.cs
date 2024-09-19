using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using System.Text.RegularExpressions;

namespace SteamKit2.Generator;

[Generator( LanguageNames.CSharp )]
public partial class WebApiGenerator : IIncrementalGenerator
{
    private static Regex FuncNameRegex = new( @"(?<name>[a-zA-Z]+)(?<version>[0-9]*)" );


    public void Initialize( IncrementalGeneratorInitializationContext context )
    {
        var methodInvocations = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static ( node, _ ) => node is InvocationExpressionSyntax invocation &&
                                           invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                                           IsInterfaceMethodInvocation( memberAccess ),
            transform: static ( context, _ ) => ExtractApiMethodInfo( context ) )
            .Where( static methodInfo => methodInfo is not null );

        context.RegisterSourceOutput( methodInvocations, static ( context, methodInfo ) =>
        {
            if ( methodInfo is not null )
            {
                var sourceCode = GenerateMethodSource( methodInfo );
                context.AddSource( $"{methodInfo.OriginalName}.g.cs", SourceText.From( sourceCode, Encoding.UTF8 ) );
            }
        } );
    }

    private static bool IsInterfaceMethodInvocation( MemberAccessExpressionSyntax memberAccess )
    {
        return memberAccess.Expression is IdentifierNameSyntax;
    }

    private static ApiMethodInfo? ExtractApiMethodInfo( GeneratorSyntaxContext context )
    {
        var invocation = ( InvocationExpressionSyntax )context.Node;
        var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;

        if ( memberAccess == null )
            return null;

        var semanticModel = context.SemanticModel;
        var symbolInfo = semanticModel.GetSymbolInfo( memberAccess.Expression );

        if ( symbolInfo.Symbol is not ILocalSymbol typeSymbol )
            return null;

        if ( typeSymbol == null || typeSymbol.Type.ToDisplayString() != "SteamKit2.WebAPI.Interface" )
            return null;

        if ( memberAccess.Name.Identifier.Text == "Call" )
            return null;

        var match = FuncNameRegex.Match( memberAccess.Name.Identifier.Text );

        if ( !match.Success )
            throw new InvalidOperationException( "Invalid API function call format. Should be FunctionName###." );

        var functionName = match.Groups[ "name" ].Value;
        var versionString = match.Groups[ "version" ].Value;

        // Default version is 1 if not specified
        int version = !string.IsNullOrEmpty( versionString ) && int.TryParse( versionString, out var ver ) ? ver : 1;

        var arguments = invocation.ArgumentList.Arguments
            .Select( arg => new ApiArgumentInfo(
                arg.NameColon?.Name.Identifier.Text ?? string.Empty,
                arg.Expression.ToString(),
                semanticModel.GetTypeInfo( arg.Expression ).Type?.ToDisplayString() ?? "object" ) )
            .ToList();

        return new ApiMethodInfo( memberAccess.Name.Identifier.Text, functionName, version, arguments );
    }

    // Generate the method source code
    private static string GenerateMethodSource( ApiMethodInfo methodInfo )
    {
        var parameters = string.Join( ", ", methodInfo.Arguments.Select( arg => $"{arg.Type} {arg.Name}" ) );
        var dictionaryEntries = string.Join( ", ", methodInfo.Arguments.Select( arg => $"{{ \"{arg.Name}\", {arg.Name} }}" ) );

        return $@"using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using SteamKit2;

#nullable enable

namespace SteamKit2;

public static class WebApiInterfaceExtensions{methodInfo.OriginalName} // todo
{{
    public static KeyValue {methodInfo.OriginalName}(this WebAPI.Interface iface, {parameters})
    {{
        var apiArgs = new Dictionary<string, object?>();
        var requestMethod = HttpMethod.Get;

        {GenerateArgumentProcessingCode( methodInfo.Arguments )}

        return iface.Call(requestMethod, ""{methodInfo.MethodName}"", {methodInfo.Version}, apiArgs);
    }}
}}
";
    }

    private static string GenerateArgumentProcessingCode( List<ApiArgumentInfo> arguments )
    {
        var processingCode = new StringBuilder();

        foreach ( var arg in arguments )
        {
            if ( arg.Name.Equals( "method", StringComparison.OrdinalIgnoreCase ) )
            {
                processingCode.AppendLine( $@"requestMethod = {arg.Name};" ); // TODO: Validate that it is HttpMethod
            }
            else
            {
                processingCode.AppendLine( $@"apiArgs.Add(""{arg.Name}"", {arg.Name});" );
                /*
                processingCode.Append( $@"
                if ({arg.Name} is IEnumerable<object> listValue)
                {{
                    int index = 0;
                    foreach (var value in listValue)
                    {{
                        apiArgs.Add($""{arg.Name}[{{index++}}"", value);
                    }}
                }}
                else
                {{
                    apiArgs.Add(""{arg.Name}"", {arg.Name});
                }}
                " );
                */
            }
        }

        return processingCode.ToString();
    }

    public class ApiMethodInfo( string originalName, string methodName, int version, List<ApiArgumentInfo> arguments )
    {
        public string OriginalName { get; } = originalName;
        public string MethodName { get; } = methodName;
        public int Version { get; } = version;
        public List<ApiArgumentInfo> Arguments { get; } = arguments;
    }

    public class ApiArgumentInfo( string name, string value, string type )
    {
        public string Name { get; } = name;
        public string Value { get; } = value;
        public string Type { get; } = type;
    }
}
