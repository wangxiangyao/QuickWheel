using System;
using UnityEngine;

namespace QuickWheel.Core
{
	// Token: 0x0200000D RID: 13
	public class WheelConfig
	{
		// Token: 0x1700000C RID: 12
		// (get) Token: 0x060000A9 RID: 169 RVA: 0x00005F8D File Offset: 0x0000418D
		public int SlotCount
		{
			get
			{
				return 9;
			}
		}

		// Token: 0x060000AA RID: 170 RVA: 0x00005F94 File Offset: 0x00004194
		public bool Validate(out string error)
		{
			bool flag = this.EnablePersistence && string.IsNullOrEmpty(this.PersistenceKey);
			bool result;
			if (flag)
			{
				error = "PersistenceKey is required when EnablePersistence is true";
				result = false;
			}
			else
			{
				bool flag2 = this.GridCellSize <= 0f;
				if (flag2)
				{
					error = "GridCellSize must be greater than 0";
					result = false;
				}
				else
				{
					bool flag3 = this.GridSpacing < 0f;
					if (flag3)
					{
						error = "GridSpacing must be non-negative";
						result = false;
					}
					else
					{
						bool flag4 = this.DeadZoneRadius < 0f;
						if (flag4)
						{
							error = "DeadZoneRadius must be non-negative";
							result = false;
						}
						else
						{
							error = null;
							result = true;
						}
					}
				}
			}
			return result;
		}

		// Token: 0x060000AB RID: 171 RVA: 0x0000602C File Offset: 0x0000422C
		public static WheelConfig CreateDefault()
		{
			return new WheelConfig
			{
				GridCellSize = 40f,
				GridSpacing = 5f,
				EnableDragSwap = true,
				EnableClickSelect = true,
				DeadZoneRadius = 20f,
				HoverScaleMultiplier = 1.15f,
				AnimationDuration = 0.1f,
				EnablePersistence = false
			};
		}

		// Token: 0x04000052 RID: 82
		public const int SLOT_COUNT = 9;

		// Token: 0x04000053 RID: 83
		public float GridCellSize = 40f;

		// Token: 0x04000054 RID: 84
		public float GridSpacing = 5f;

		// Token: 0x04000055 RID: 85
		public bool EnableDragSwap = true;

		// Token: 0x04000056 RID: 86
		public bool EnableClickSelect = true;

		// Token: 0x04000057 RID: 87
		public float DeadZoneRadius = 40f;

		// Token: 0x04000058 RID: 88
		public float HoverScaleMultiplier = 1.15f;

		// Token: 0x04000059 RID: 89
		public float AnimationDuration = 0.2f;

		// Token: 0x0400005A RID: 90
		public Sprite SlotNormalSprite = null;

		// Token: 0x0400005B RID: 91
		public Sprite SlotHoverSprite = null;

		// Token: 0x0400005C RID: 92
		public Sprite SlotSelectedSprite = null;

		// Token: 0x0400005D RID: 93
		public bool EnablePersistence = false;

		// Token: 0x0400005E RID: 94
		public string PersistenceKey = "";
	}
}

