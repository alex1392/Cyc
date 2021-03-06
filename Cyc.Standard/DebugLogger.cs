﻿using System;
using System.Text;

namespace Cyc.Standard {

	public class DebugLogger : ILogger {

		#region Public Methods

		public void Log(Exception ex)
		{
			var stringBuilder = new StringBuilder();
			do {
				stringBuilder.Append($"====={ex.GetType()}====={Environment.NewLine}{ex.Message}");
				ex = ex.InnerException;
			} while (ex != null);
			Console.WriteLine(stringBuilder.ToString());
		}

		public void Log(string message)
		{
			Console.WriteLine(message);
		}

		#endregion Public Methods
	}
}