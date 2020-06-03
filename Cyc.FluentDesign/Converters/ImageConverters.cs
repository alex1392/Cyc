using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;

using PixelFormatWinForm = System.Drawing.Imaging.PixelFormat;
using PointWinForm = System.Drawing.Point;

namespace Cyc.FluentDesign.Converters {

	public static class ImageConverters {

		#region Public Methods

		public static Bitmap ToBitmap(this BitmapSource bitmapSource)
		{
			if (bitmapSource == null) {
				return new Bitmap(1, 1);
			}

			var bitmap = new Bitmap(bitmapSource.PixelWidth, bitmapSource.PixelHeight, PixelFormatWinForm.Format32bppPArgb);
			var data = bitmap.LockBits(new Rectangle(PointWinForm.Empty, bitmap.Size), ImageLockMode.WriteOnly, PixelFormatWinForm.Format32bppPArgb);
			bitmapSource.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
			bitmap.UnlockBits(data);
			return bitmap;
		}

		public static Icon ToIcon(this Bitmap bitmap)
		{
			var Hicon = bitmap.GetHicon();
			var newIcon = Icon.FromHandle(Hicon);

			DeleteObject(Hicon);

			return newIcon;
		}

		#endregion Public Methods

		#region Private Methods

		[DllImport("gdi32.dll")]
		private static extern bool DeleteObject(IntPtr hObject);

		#endregion Private Methods
	}
}