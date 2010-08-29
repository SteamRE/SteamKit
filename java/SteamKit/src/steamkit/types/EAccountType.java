package steamkit.types;

public enum EAccountType
{
    Invalid(0),

    Individual(1),
    Multiseat(2),
    GameServer(3),
    AnonGameServer(4),
    Pending(5),
    ContentServer(6),
    Clan(7),
    Chat(8),
    P2PSuperSeeder(9),
    AnonUser(10),

    Max(11);
    
    private int code;

    private EAccountType( int e )
    {
      code = e;
    }

    public int getCode()
    {
      return code;
    }
    
    public static EAccountType lookupAccount( int account )
    {
    	for ( EAccountType current : values() )
    	{
    		if ( current.getCode() == account )
    		{
    			return current;
    		}
    	}
    	
    	return Invalid;
    }
}
