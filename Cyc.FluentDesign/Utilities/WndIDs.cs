
using System;

namespace Cyc.FluentDesign
{
	internal static class WndIDs {

		#region Public Fields

		public const int WM_MOUSEHWHEEL = 0x020E;

		#endregion Public Fields

		#region Public Methods

		/// <summary>
		/// Gets high bits values of the pointer.
		/// </summary>
		public static int HIWORD(this IntPtr ptr)
		{
			return (ptr.ToInt32() >> 16) & 0xFFFF;
		}

		/// <summary>
		/// Gets low bits values of the pointer.
		/// </summary>
		public static int LOWORD(this IntPtr ptr)
		{
			return ptr.ToInt32() & 0xFFFF;
		}

		#endregion Public Methods
	}
}