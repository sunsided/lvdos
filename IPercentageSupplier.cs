using System;
using System.Collections.Generic;
using System.Text;

namespace MMT.LVDOS
{
	/// <summary>
	/// Interface that supports percentage callback
	/// </summary>
	public interface IPercentageSupplier
	{
		/// <summary>
		/// Event that is invoked when the percentage changes
		/// </summary>
		event PercentageHandler PercentageChanged;
	}
}
