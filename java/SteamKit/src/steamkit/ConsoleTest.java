package steamkit;

import java.io.IOException;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.ExecutionException;
import java.util.concurrent.Future;

import steamkit.CM.CMInterface;
import steamkit.types.EAccountType;


public class ConsoleTest
{
	/**
	 * @param args
	 */
	public static void main(String[] args)
	{
		CMInterface cm = new CMInterface();
		
		CountDownLatch waithandle = cm.initialize();

		try {
			waithandle.await();
		} catch (InterruptedException e1) {
			e1.printStackTrace();
		}
		
		waithandle = cm.connect();
		
		try {
			waithandle.await();
		} catch (InterruptedException e1) {
			e1.printStackTrace();
		}
		
		System.out.println(" Ready to sign on!");
		
		cm.anonSignOn( EAccountType.AnonGameServer );
		
		try {
			System.in.read();
		} catch (IOException e) {
			e.printStackTrace();
		}
		
		cm.close();
	}

}
