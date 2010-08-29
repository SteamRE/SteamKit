package net.steam3.types;

public enum EUdpPacketType {
    Invalid(0),
    ChallengeReq(1),
    Challenge(2),
    Connect(3),
    Accept(4),
    Disconnect(5),
    Data(6),
    Datagram(7),
    Max(8);
    
    private int code;

    private EUdpPacketType( int e )
    {
      code = e;
    }

    public int getCode()
    {
      return code;
    }
    
    public static EUdpPacketType lookupType( int type )
    {
    	for ( EUdpPacketType current : values() )
    	{
    		if ( current.getCode() == type )
    		{
    			return current;
    		}
    	}
    	
    	return Invalid;
    }
}
