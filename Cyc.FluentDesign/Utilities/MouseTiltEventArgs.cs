
using System;

namespace Cyc.FluentDesign
{
	public class MouseTiltEventArgs : EventArgs {

		#region Public Properties

		public int Tilt { get; set; }

		#endregion Public Properties

		#region Public Constructors

		public MouseTiltEventArgs(int tilt)
		{
			Tilt = tilt;
		}

		#endregion Public Constructors
	}
}