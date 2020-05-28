using System;


namespace Cyc.Standard {
	public interface ILogger {
		void Log(Exception ex);
		void Log(string message);
	}
}