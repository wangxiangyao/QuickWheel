using System;
using UnityEngine;

namespace QuickWheel.Core.Interfaces
{
	public class WheelSelectionContext
	{
		public Vector2 InputPosition { get; set; }
		public Vector2 WheelCenter { get; set; }
		public WheelConfig Config { get; set; }
		public int SlotCount { get { return Config != null ? Config.SlotCount : 0; } }
		public float DeadZoneRadius { get { return Config != null ? Config.DeadZoneRadius : 0f; } }
		public float GridCellSize { get { return Config != null ? Config.GridCellSize : 40f; } }
		public float GridSpacing { get { return Config != null ? Config.GridSpacing : 5f; } }
	}
}

