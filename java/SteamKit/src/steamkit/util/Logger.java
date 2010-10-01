package steamkit.util;

public class Logger implements ILogger
{
	private static ILogger logger = new Logger();
	
	public static ILogger getLogger()
	{
		return logger;
	}
	
	public static void setLogger( ILogger logger )
	{
		Logger.logger = logger;
	}
	
	public void println( String output )
	{
		System.out.println( output );
	}
}
