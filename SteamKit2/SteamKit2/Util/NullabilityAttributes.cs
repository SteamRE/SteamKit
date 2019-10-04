namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsage( AttributeTargets.Field |
                    AttributeTargets.Parameter |
                    AttributeTargets.Property )]
    internal sealed class AllowNullAttribute : Attribute
    {
    }

    [AttributeUsage( AttributeTargets.Field |
                    AttributeTargets.Parameter |
                    AttributeTargets.Property )]
    internal sealed class DisallowNullAttribute : Attribute
    {
    }

    [AttributeUsage( AttributeTargets.Field |
                    AttributeTargets.Parameter |
                    AttributeTargets.Property |
                    AttributeTargets.ReturnValue )]
    internal sealed class MaybeNullAttribute : Attribute
    {
    }

    [AttributeUsage( AttributeTargets.Field |
                    AttributeTargets.Parameter |
                    AttributeTargets.Property |
                    AttributeTargets.ReturnValue )]
    internal sealed class NotNullAttribute : Attribute
    {
    }

    [AttributeUsage( AttributeTargets.Parameter )]
    internal sealed class MaybeNullWhenAttribute : Attribute
    {
        public MaybeNullWhenAttribute( bool returnValue )
        {
            ReturnValue = returnValue;
        }

        public bool ReturnValue { get; }
    }

    [AttributeUsage( AttributeTargets.Parameter )]
    internal sealed class NotNullWhenAttribute : Attribute
    {
        public NotNullWhenAttribute( bool returnValue )
        {
            ReturnValue = returnValue;
        }

        public bool ReturnValue { get; }
    }

    [AttributeUsage( AttributeTargets.Parameter |
                    AttributeTargets.Property |
                    AttributeTargets.ReturnValue, AllowMultiple = true )]
    internal sealed class NotNullIfNotNullAttribute : Attribute
    {
        public NotNullIfNotNullAttribute( string parameterName )
        {
            ParameterName = parameterName;
        }

        public string ParameterName { get; }
    }

    [AttributeUsage( AttributeTargets.Method | AttributeTargets.Constructor )]
    internal sealed class DoesNotReturnAttribute : Attribute
    {
    }

    [AttributeUsage( AttributeTargets.Parameter )]
    internal sealed class DoesNotReturnIfAttribute : Attribute
    {
        public DoesNotReturnIfAttribute( bool parameterValue )
        {
            ParameterValue = parameterValue;
        }

        public bool ParameterValue { get; }
    }
}
