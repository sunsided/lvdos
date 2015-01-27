using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Drawing;

namespace MMT.LVDOS
{
	public partial class StencilBuffer : IStencilBuffer, ICloneable
	{
		protected BitArray _buffer = null;
		private int _width = 8;
		private int _height = 8;

		public StencilBuffer()
			: this(8,8)
		{
		}

		public StencilBuffer(int width, int height)
		{
			_width = width;
			_height = height;
			_buffer = new BitArray(_width * _height);	
		}

		public StencilBuffer(StencilBuffer buffer)
		{
			_width = buffer.Width;
			_height = buffer.Height;
			_buffer = (BitArray)buffer._buffer.Clone();
		}

		public bool this[int offset]
		{
			get { return _buffer[offset]; }
			set { _buffer[offset] = value; }
		}

		public bool this[int x, int y]
		{
			get { return _buffer[x + y * _width]; }
			set { _buffer[x + y * _width] = value; }
		}

		public int Length
		{
			get { return _buffer.Length; }
		}

		public void Clear()
		{
			_buffer.SetAll(false);
		}

		public int Width
		{
			get { return _width; }
		}

		public int Height
		{
			get { return _height; }
		}

		#region ICloneable Members

		object ICloneable.Clone()
		{
			return this.Clone();
		}

		public StencilBuffer Clone()
		{
			return new StencilBuffer(this);
		}

		#endregion

		/// <summary>
		/// Merges the region, setting the interior values to true
		/// </summary>
		/// <param name="region">The region to merge</param>
		public void MergeRegion(int x, int y, int width, int height)
		{
			MergeRegion( new System.Drawing.Rectangle(x, y, width, height) );
		}

		/// <summary>
		/// Merges the region, setting the interior values to true
		/// </summary>
		/// <param name="region">The region to merge</param>
		public void MergeRegion(System.Drawing.Rectangle region)
		{
			if (region.X < 0 || region.Y < 0 || region.Right >= Width || region.Bottom >= Height) throw new ArgumentOutOfRangeException();

			int scanline = region.Y;
			int blockHeight = region.Height;
			int offset = region.X;
			int width = region.Width;

			int stride = Width;
			int shift = stride - width;

			int pos = scanline * stride + offset;

			// über alle Zeilen
			for (int count = 0; count < blockHeight; ++count)
			{
				// über alle Spalten
				for (int x = 0; x < width; ++x)
				{
					this[pos++] = true;
				}
				pos += shift;
			}
		}

		/// <summary>
		/// Counts the active flags in a given scanline
		/// </summary>
		/// <param name="scanline">The scanline to check</param>
		public int ActiveInScanline(int scanline)
		{
			int pos = scanline * Width;
			int count = 0;

			// über alle Spalten
			for (int x = 0; x < Width; ++x)
			{
				if (this[pos++]) count++;
			}

			// Ausgeben
			return count;
		}

		/// <summary>
		/// Removes the region, setting the interior values to false
		/// </summary>
		/// <param name="region">The region to remove</param>
		public void RemoveRegion(int x, int y, int width, int height)
		{
			RemoveRegion( new System.Drawing.Rectangle(x, y, width, height) );
		}

		/// <summary>
		/// Removes the region, setting the interior values to false
		/// </summary>
		/// <param name="region">The region to remove</param>
		public void RemoveRegion(System.Drawing.Rectangle region)
		{
			if (region.X < 0 || region.Y < 0 || region.Right >= Width || region.Bottom >= Height) throw new ArgumentOutOfRangeException();

			int scanline = region.Y;
			int blockHeight = region.Height;
			int offset = region.X;
			int width = region.Width;

			int stride = Width;
			int shift = stride - width;

			int pos = scanline * stride + offset;

			// über alle Zeilen
			for (int count = 0; count < blockHeight; ++count)
			{
				// über alle Spalten
				for (int x = 0; x < width; ++x)
				{
					this[pos++] = false;
				}
				pos += shift;
			}
		}

		public void SetAll(bool flag)
		{
			_buffer.SetAll(flag);
		}

		public void SetScanline(int scanline, bool flag)
		{
			scanline *= _width;
			//for (int x = 0; x < _width; ++x)
			//{
			//    _buffer[ scanline + x ] = flag;
			//}

			_width += scanline;
			for (int x = scanline; x < _width; ++x)
			{
				_buffer[x] = flag;
			}
		}

		public static StencilBuffer operator &(StencilBuffer a, StencilBuffer b)
		{
			if (a.Width != b.Width || a.Height != b.Height) throw new ArgumentException();
			StencilBuffer c = new StencilBuffer(a.Width, a.Height);
			c._buffer = a._buffer.And(b._buffer);
			return c;
		}

		public static StencilBuffer operator |(StencilBuffer a, StencilBuffer b)
		{
			if (a.Width != b.Width || a.Height != b.Height) throw new ArgumentException();
			StencilBuffer c = new StencilBuffer(a.Width, a.Height);
			c._buffer = a._buffer.Or(b._buffer);
			return c;
		}

		public static StencilBuffer operator ^(StencilBuffer a, StencilBuffer b)
		{
			if (a.Width != b.Width || a.Height != b.Height) throw new ArgumentException();
			StencilBuffer c = new StencilBuffer(a.Width, a.Height);
			c._buffer = a._buffer.Xor(b._buffer);
			return c;
		}

		public static StencilBuffer operator ~(StencilBuffer a)
		{
			return !a;
		}

		public static StencilBuffer operator !(StencilBuffer a)
		{
			StencilBuffer c = new StencilBuffer(a.Width, a.Height);
			c._buffer = a._buffer.Not();
			return c;
		}

		public static bool operator ==(StencilBuffer a, StencilBuffer b)
		{
			return (a._buffer == b._buffer);
		}

		public static bool operator !=(StencilBuffer a, StencilBuffer b)
		{
			return (a._buffer != b._buffer);
		}

		public override bool Equals(object obj)
		{
			if (obj is StencilBuffer)
			{
				return ((StencilBuffer)obj)._buffer.Equals(this._buffer);
			}
			else if (obj is BitArray)
			{
				return ((BitArray)obj).Equals(this._buffer);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return _buffer.GetHashCode();
		}

		/// <summary>
		/// Gets a quadratic rectangular region from the stencil buffer
		/// </summary>
		/// <returns>A Rectangle structure containing the biggest range to render</returns>
		public Rectangle GetRegion()
		{
			int ymin = Width, ymax = 0,
				xmin = Width, xmax = 0;
			int pos = 0;
			for (int y = 0; y < Height; ++y)
			{
				for (int x = 0; x < Width; ++x)
				{
					if (this[pos++])
					{
						if (y < ymin) ymin = y;
						if (y > ymax) ymax = y;
						if (x < xmin) xmin = x;
						if (x > xmax) xmax = x;
					}
				}
			}

			if (ymin > ymax || xmin > xmax) return Rectangle.Empty;
			return new Rectangle(xmin, ymin, xmax - xmin + 1, ymax - ymin + 1);
		}
	}
}
