using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace MMT.LVDOS.Threading
{
	/// <summary>
	/// Provides the implementation of an object that is able to thread itself
	/// </summary>
	public static class Contract
	{
		/// <summary>
		/// Timeout in msecs
		/// </summary>
		public readonly static int Timeout = 10000;

		/// <summary>
		/// System processor count
		/// </summary>
		public readonly static int ProcessorCount = Environment.ProcessorCount;
	}
}
