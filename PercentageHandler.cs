using System;
using System.Collections.Generic;
using System.Text;

namespace MMT.LVDOS
{
	/// <summary>
	/// Invoked when the percentage of an operation's completion changes
	/// </summary>
	/// <param name="sender">The sender of the event</param>
	/// <param name="percentage">The current processing percentage</param>
	public delegate void PercentageHandler( IPercentageSupplier sender, int percentage );
}
