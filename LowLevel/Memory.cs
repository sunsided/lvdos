using System;
using System.Collections.Generic;
using System.Text;

namespace MMT.LVDOS.LowLevel
{
	public sealed partial class Memory
	{

		/// <summary>
		/// Copies a memory block integerwise
		/// </summary>
		/// <param name="src">Source byte block</param>
		/// <param name="srcIndex">Source block starting index</param>
		/// <param name="dst">Destination byte block</param>
		/// <param name="dstIndex">Destination block starting index</param>
		/// <param name="count">Byte count to copy</param>
		static unsafe void Copy(byte[] src, int srcIndex, byte[] dst, int dstIndex, int count)
		{
			Copy32(src, srcIndex, dst, dstIndex, count);
		}

		/// <summary>
		/// Copies a memory block integerwise
		/// </summary>
		/// <param name="src">Source byte block</param>
		/// <param name="srcIndex">Source block starting index</param>
		/// <param name="dst">Destination byte block</param>
		/// <param name="dstIndex">Destination block starting index</param>
		/// <param name="count">Byte count to copy</param>
		static unsafe void Copy32(byte[] src, int srcIndex, byte[] dst, int dstIndex, int count)
		{
			if (src == null || srcIndex < 0 || dst == null || dstIndex < 0 || count < 0)
			{
				throw new ArgumentException();
			}

			int srcLen = src.Length;
			int dstLen = dst.Length;
			if (srcLen - srcIndex < count ||
				dstLen - dstIndex < count)
			{
				throw new ArgumentException();
			}

			// The following fixed statement pins the location of
			// the src and dst objects in memory so that they will
			// not be moved by garbage collection.      
			int countD8 = count / 8;
			int countM8 = count % 8;
			fixed (byte* pSrc = src, pDst = dst)
			{
				byte* ps = pSrc;
				byte* pd = pDst;

				// Loop over the count in blocks of 8 bytes, copying a
				// long (8 bytes) at a time:
				for (int n = 0; n < countD8; ++n)
				{
					*((long*)pd) = *((long*)ps);
					pd += 8;
					ps += 8;
				}

				// Complete the copy by moving any bytes that weren't
				// moved in blocks of 4:
				for (int n = 0; n < countM8; ++n)
				{
					*pd = *ps;
					pd++;
					ps++;
				}
			}
		}

		/// <summary>
		/// Copies a memory block integerwise
		/// </summary>
		/// <param name="src">Source byte block</param>
		/// <param name="srcIndex">Source block starting index</param>
		/// <param name="dst">Destination byte block</param>
		/// <param name="dstIndex">Destination block starting index</param>
		/// <param name="count">Byte count to copy</param>
		static unsafe void Copy64(byte[] src, int srcIndex, byte[] dst, int dstIndex, int count)
		{
			if (src == null || srcIndex < 0 || dst == null || dstIndex < 0 || count < 0)
			{
				throw new ArgumentException();
			}

			int srcLen = src.Length;
			int dstLen = dst.Length;
			if (srcLen - srcIndex < count ||
				dstLen - dstIndex < count)
			{
				throw new ArgumentException();
			}

			// The following fixed statement pins the location of
			// the src and dst objects in memory so that they will
			// not be moved by garbage collection.      
			int countD4 = count / 4;
			int countM4 = count % 4;
			fixed (byte* pSrc = src, pDst = dst)
			{
				byte* ps = pSrc;
				byte* pd = pDst;

				// Loop over the count in blocks of 4 bytes, copying an
				// integer (4 bytes) at a time:
				for (int n = 0; n < countD4; ++n)
				{
					*((int*)pd) = *((int*)ps);
					pd += 4;
					ps += 4;
				}

				// Complete the copy by moving any bytes that weren't
				// moved in blocks of 4:
				for (int n = 0; n < countM4; ++n)
				{
					*pd = *ps;
					pd++;
					ps++;
				}
			}
		}

	}
}
