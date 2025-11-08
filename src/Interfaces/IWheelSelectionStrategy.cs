using System;

namespace QuickWheel.Core.Interfaces
{
	public interface IWheelSelectionStrategy
	{
		int GetSelectedIndex(WheelSelectionContext context);
	}
}

