using QuickWheel.Core.Interfaces;
using UnityEngine;

namespace QuickWheel.Selection
{
    /// <summary>
    /// 角度选择策略（默认实现）
    /// 根据鼠标相对轮盘中心的角度选择槽位
    /// </summary>
    public class AngleSelectionStrategy : IWheelSelectionStrategy
    {
        /// <summary>
        /// 根据输入位置计算选中的槽位索引
        /// </summary>
        public int GetSlotIndexFromPosition(
            Vector2 wheelCenter,
            Vector2 inputPosition,
            int slotCount,
            float[] slotAngles)
        {
            // 计算方向向量
            Vector2 direction = inputPosition - wheelCenter;

            // 计算角度（0-360度）
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360f;

            // 计算每个槽位的角度范围
            float angleStep = 360f / slotCount;
            float halfStep = angleStep / 2f;

            // 找到最接近的槽位
            for (int i = 0; i < slotCount; i++)
            {
                // 获取槽位角度（使用自定义角度或均匀分布）
                float slotAngle = slotAngles != null && slotAngles.Length == slotCount
                    ? slotAngles[i]
                    : (i * angleStep);

                // 计算角度范围
                float lowerBound = (slotAngle - halfStep + 360f) % 360f;
                float upperBound = (slotAngle + halfStep) % 360f;

                // 检查角度是否在范围内（处理跨越0度的情况）
                if (IsAngleInRange(angle, lowerBound, upperBound))
                {
                    return i;
                }
            }

            return -1;  // 理论上不应该到达这里
        }

        /// <summary>
        /// 判断是否在死区内
        /// </summary>
        public bool IsInDeadZone(Vector2 wheelCenter, Vector2 inputPosition, float deadZoneRadius)
        {
            return Vector2.Distance(wheelCenter, inputPosition) < deadZoneRadius;
        }

        /// <summary>
        /// 检查角度是否在范围内（处理跨越0度的情况）
        /// </summary>
        private bool IsAngleInRange(float angle, float lowerBound, float upperBound)
        {
            // 处理跨越0度的情况
            if (lowerBound > upperBound)
            {
                return angle >= lowerBound || angle <= upperBound;
            }
            else
            {
                return angle >= lowerBound && angle <= upperBound;
            }
        }
    }
}
