using System;
namespace MMT.LVDOS
{
	public interface IStencilBuffer
	{
		int ActiveInScanline(int scanline);
		void Clear();
		System.Drawing.Rectangle GetRegion();
		int Height { get; }
		int Length { get; }
		void MergeRegion(System.Drawing.Rectangle region);
		void MergeRegion(int x, int y, int width, int height);
		void RemoveRegion(System.Drawing.Rectangle region);
		void RemoveRegion(int x, int y, int width, int height);
		void SetAll(bool flag);
		void SetScanline(int scanline, bool flag);
		bool this[int x, int y] { get; set; }
		bool this[int offset] { get; set; }
		int Width { get; }
	}
}
