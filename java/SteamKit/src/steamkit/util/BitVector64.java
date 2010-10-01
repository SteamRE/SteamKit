package steamkit.util;

/*
 * Bit vector splitting a 64-bit int
 */
public class BitVector64
{
    private long data;

    public BitVector64()
    {
    }

    public BitVector64( long value )
    {
        data = value;
    }

    public long getAllData()
    {
    	return data;
    }
    
    public long getData( int bitoffset, long valuemask )
    {
    	return ( data >> bitoffset ) & valuemask;
    }

    public void setData( int bitoffset, long valuemask, long value )
    {
    	data = ( data & ~( valuemask << bitoffset ) ) | ( ( value & valuemask ) << bitoffset );
    }
}
