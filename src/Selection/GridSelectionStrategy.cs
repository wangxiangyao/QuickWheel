using QuickWheel.Core.Interfaces;
using UnityEngine;

namespace QuickWheel.Selection
{
	public class GridSelectionStrategy : IWheelSelectionStrategy
	{
		public int GetSelectedIndex(WheelSelectionContext context)
		{
			if (context.SlotCount != 9)
			{
				Debug.LogWarning("[GridSelectionStrategy] 槽位数应为9，当前为 " + context.SlotCount);
			}
			if (IsInDeadZone(context))
			{
				return -1;
			}
			Vector2 direction = context.InputPosition - context.WheelCenter;
			Vector2 correctedDirection = new Vector2(direction.x, -direction.y);
			float angle = Mathf.Atan2(correctedDirection.y, correctedDirection.x) * Mathf.Rad2Deg;
			if (angle < 0) angle += 360f;
			return GetDirectionIndexFromAngle(angle);
		}

		public bool IsInDeadZone(WheelSelectionContext context)
		{
			return Vector2.Distance(context.WheelCenter, context.InputPosition) < context.DeadZoneRadius;
		}

		private int GetDirectionIndexFromAngle(float angle)
		{
			if (angle >= 337.5f || angle < 22.5f) return 1;
			if (angle >= 22.5f && angle < 67.5f) return 5;
			if (angle >= 67.5f && angle < 112.5f) return 3;
			if (angle >= 112.5f && angle < 157.5f) return 4;
			if (angle >= 157.5f && angle < 202.5f) return 0;
			if (angle >= 202.5f && angle < 247.5f) return 7;
			if (angle >= 247.5f && angle < 292.5f) return 2;
			if (angle >= 292.5f && angle < 337.5f) return 6;
			return 1;
		}
	}
}
