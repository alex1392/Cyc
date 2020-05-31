using System;

namespace Cyc.Standard {

	public interface ILogger {

		#region Public Methods

		void Log(Exception ex);

		void Log(string message);

		#endregion Public Methods
	}
}