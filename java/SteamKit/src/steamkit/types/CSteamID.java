package steamkit.types;

import steamkit.steam3.SteamLanguage.EAccountType;
import steamkit.steam3.SteamLanguage.EUniverse;
import steamkit.util.BitVector64;

public class CSteamID
{
	public static final CSteamID Invalid = new CSteamID();
	
	private BitVector64 id;
	
	public CSteamID()
	{
		this(0);
	}
	
    public CSteamID( int unAccountID, EUniverse eUniverse, EAccountType eAccountType )
    {
    	this();
    	Set( unAccountID, eUniverse, eAccountType );
    }

    public CSteamID( int unAccountID, int unInstance, EUniverse eUniverse, EAccountType eAccountType )
	{
    	this();
		InstancedSet( unAccountID, unInstance, eUniverse, eAccountType );
	}

	public CSteamID( long id )
	{
		this.id = new BitVector64( id );
	}
	
	
    public void Set( int unAccountID, EUniverse eUniverse, EAccountType eAccountType )
    {
        this.setAccountID( unAccountID );
        this.setUniverse( eUniverse );
        this.setAccountType( eAccountType );

        if ( eAccountType == EAccountType.Clan )
        {
            this.setAccountInstance( 0 );
        }
        else
        {
        	this.setAccountInstance( 1 );
        }
    }

    public void InstancedSet( int unAccountID, int unInstance, EUniverse eUniverse, EAccountType eAccountType )
    {
        this.setAccountID( unAccountID );
        this.setUniverse( eUniverse );
        this.setAccountType( eAccountType );
        this.setAccountInstance( unInstance );
    }
	
	
    public boolean BBlankAnonAccount()
    {
        return this.getAccountID() == 0 && BAnonAccount() && this.getAccountInstance() == 0;
    }
    
    public boolean BGameServerAccount()
    {
        return this.getAccountType() == EAccountType.GameServer || this.getAccountType() == EAccountType.AnonGameServer;
    }
    
    public boolean BContentServerAccount()
    {
        return this.getAccountType() == EAccountType.ContentServer;
    }
    
    public boolean BClanAccount()
    {
        return this.getAccountType() == EAccountType.Clan;
    }
    
    public boolean BChatAccount()
    {
        return this.getAccountType() == EAccountType.Chat;
    }
    
    public boolean IsLobby()
    {
        return ( this.getAccountType() == EAccountType.Chat ) && ( ( this.getAccountInstance() & ( 0x000FFFFF + 1 ) >> 2 ) != 0 );
    }
    
    public boolean BAnonAccount()
    {
        return this.getAccountType() == EAccountType.AnonUser || this.getAccountType() == EAccountType.AnonGameServer;
    }
    
    public boolean BAnonUserAccount()
    {
        return this.getAccountType() == EAccountType.AnonUser;
    }

    public boolean IsValid()
    {
        if ( this.getAccountType() == EAccountType.Invalid || this.getAccountType() == EAccountType.Max )
            return false;

        if ( this.getUniverse() == EUniverse.Invalid || this.getUniverse() == EUniverse.Max )
            return false;

        if ( this.getAccountType() == EAccountType.Individual )
        {
            if ( this.getAccountID() == 0 || this.getAccountInstance() != 1 )
                return false;
        }

        if ( this.getAccountType() == EAccountType.Clan )
        {
            if ( this.getAccountID() == 0 || this.getAccountInstance() != 0 )
                return false;
        }

        return true;
    }
    
    
	public long getLong()
	{
		return id.getAllData();
	}
	
	public int getAccountID()
	{
		return (int)id.getData( 0, 0xFFFFFFFF );
	}
	
	public void setAccountID( int accountid )
	{
		id.setData( 0, 0xFFFFFFFF, accountid );
	}
	
	public int getAccountInstance()
	{
		return (int)id.getData( 32, 0xFFFFF );
	}
	
	public void setAccountInstance( int accountinstance )
	{
		id.setData( 32, 0xFFFFF, accountinstance );
	}
	
	public EAccountType getAccountType()
	{
		return EAccountType.lookup( (int)id.getData( 52, 0xF ) );
	}
	
	public void setAccountType( EAccountType accounttype )
	{
		id.setData( 52, 0xF, accounttype.getCode() );
	}
    
	public EUniverse getUniverse()
	{
		return EUniverse.lookup( (int)id.getData( 56, 0xFF ) );
	}
	
	public void setUniverse( EUniverse universe )
	{
		id.setData( 56, 0xFF, universe.getCode() );
	}
	
	
	public boolean equals( Object other )
	{
	    if ( this == other )
	      return true;
	    if ( !( other instanceof CSteamID ) )
	      return false;
	    
	    CSteamID otherID = (CSteamID)other;
	    return ( getLong() == otherID.getLong() );
	}

	public int hashCode()
	{
		Long longid = getLong();
		return longid.hashCode();
	}
}
