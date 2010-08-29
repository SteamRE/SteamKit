package steamkit.types;

public enum EUniverse
{
    Invalid(0),

    Public(1),
    Beta(2),
    Internal(3),
    Dev(4),
    RC(5),

    Max(6);
    
    private int code;

    private EUniverse( int e )
    {
      code = e;
    }

    public int getCode()
    {
      return code;
    }
    
    public static EUniverse lookupUniverse( int universe )
    {
    	for ( EUniverse current : values() )
    	{
    		if ( current.getCode() == universe )
    		{
    			return current;
    		}
    	}
    	
    	return Invalid;
    }
}