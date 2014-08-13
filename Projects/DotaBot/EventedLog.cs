using System;
using log4net;

namespace DotaBot
{
	public class EventedLog
	{
		ILog log;
		public event MsgHandler OnMessage;
		public delegate void MsgHandler(string msg);
		public EventedLog (ILog log)
		{
			this.log = log;
		}

		public void Debug (object message, Exception exception){
			log.Debug (message, exception);
			if (OnMessage != null)
				OnMessage ("[DEBUG] "+message.ToString());
		}

		public void Debug (object message){
			log.Debug (message);
			if (OnMessage != null)
				OnMessage ("[DEBUG]"+message.ToString());
		}

		public void DebugFormat (string format, params object[] args){
			log.DebugFormat (format, args);	
			if (OnMessage != null)
				OnMessage ("[DEBUG] "+string.Format(format, args));
		}
	}
}