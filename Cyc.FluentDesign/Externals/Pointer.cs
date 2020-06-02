using System;

namespace Cyc
{
  public static class PointerExtensions
  {
    /// <summary>
    /// Gets high bits values of the pointer.
    /// </summary>
    public static int HIWORD(this IntPtr ptr) => (ptr.ToInt32() >> 16) & 0xFFFF;

    /// <summary>
    /// Gets low bits values of the pointer.
    /// </summary>
    public static int LOWORD(this IntPtr ptr) => ptr.ToInt32() & 0xFFFF;
  }
}
