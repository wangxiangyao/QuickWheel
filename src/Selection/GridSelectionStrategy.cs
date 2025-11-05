using QuickWheel.Core.Interfaces;
using UnityEngine;

namespace QuickWheel.Selection
{
    /// <summary>
    /// 9宫格选择策略
    /// 根据鼠标相对轮盘中心的方向角度选择8个方向的格子（中心空）
    ///
    /// 布局（索引分布）：
    /// [7] [2] [6]    左上 上中 右上
    /// [0] [ ] [1]    左中 中心 右中
    /// [4] [3] [5]    左下 下中 右下
    ///
    /// 角度范围（从右侧0度开始逆时针，Unity坐标系）：
    /// 0: 左中 (157.5° - 202.5°)
    /// 1: 右中 (337.5° - 22.5°)
    /// 2: 上中 (247.5° - 292.5°)
    /// 3: 下中 (67.5° - 112.5°)
    /// 4: 左下 (112.5° - 157.5°)
    /// 5: 右下 (22.5° - 67.5°)
    /// 6: 右上 (292.5° - 337.5°)
    /// 7: 左上 (202.5° - 247.5°)
    /// </summary>
    public class GridSelectionStrategy : IWheelSelectionStrategy
    {
        // 固定9宫格（8个方向 + 1个中心空位）
        private const int GRID_SLOT_COUNT = 9;

        // 9宫格位置映射（索引 0-8，中间索引8为空）
        // 索引对应的方向角度
        private static readonly float[] DIRECTION_ANGLES = new float[]
        {
            180f,  // 0: 左中
            0f,    // 1: 右中
            270f,  // 2: 上中
            90f,   // 3: 下中
            135f,  // 4: 左下
            45f,   // 5: 右下
            315f,  // 6: 右上
            225f,  // 7: 左上
            0f,    // 8: 中心（不使用）
        };

    
        /// <summary>
        /// 根据输入位置计算选中的槽位索引（9宫格方向判断）
        /// </summary>
        public int GetSlotIndexFromPosition(
            Vector2 wheelCenter,
            Vector2 inputPosition,
            int slotCount,
            float[] slotAngles)
        {
            // 固定9宫格，忽略slotCount参数
            if (slotCount != GRID_SLOT_COUNT)
            {
                Debug.LogWarning($"[GridSelectionStrategy] 槽位数应为{GRID_SLOT_COUNT}，当前为{slotCount}");
            }

            // 计算方向向量
            Vector2 direction = inputPosition - wheelCenter;

            // 计算角度（0-360度，0度为右侧，逆时针）
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Unity的Atan2返回-180到180，需要转换为0-360
            if (angle < 0) angle += 360f;

            // 找到最接近的方向
            return GetDirectionIndexFromAngle(angle);
        }

        /// <summary>
        /// 判断是否在死区内
        /// </summary>
        public bool IsInDeadZone(Vector2 wheelCenter, Vector2 inputPosition, float deadZoneRadius)
        {
            return Vector2.Distance(wheelCenter, inputPosition) < deadZoneRadius;
        }

        /// <summary>
        /// 根据角度获取方向索引
        /// 使用8方向判断（上下左右 + 四个对角）
        /// </summary>
        private int GetDirectionIndexFromAngle(float angle)
        {
            // 8方向角度范围判断
            // 优先判断4个主方向（上下左右），然后判断4个对角方向

            // 右中 (337.5° - 22.5°)
            if (angle >= 337.5f || angle < 22.5f)
                return 1;

            // 右下 (22.5° - 67.5°)
            if (angle >= 22.5f && angle < 67.5f)
                return 5;

            // 下中 (67.5° - 112.5°)
            if (angle >= 67.5f && angle < 112.5f)
                return 3;

            // 左下 (112.5° - 157.5°)
            if (angle >= 112.5f && angle < 157.5f)
                return 4;

            // 左中 (157.5° - 202.5°)
            if (angle >= 157.5f && angle < 202.5f)
                return 0;

            // 左上 (202.5° - 247.5°)
            if (angle >= 202.5f && angle < 247.5f)
                return 7;

            // 上中 (247.5° - 292.5°)
            if (angle >= 247.5f && angle < 292.5f)
                return 2;

            // 右上 (292.5° - 337.5°)
            if (angle >= 292.5f && angle < 337.5f)
                return 6;

            // 默认返回右中（理论上不应该到达这里）
            return 1;
        }

        /// <summary>
        /// 获取指定索引的方向角度
        /// </summary>
        public float GetDirectionAngle(int index)
        {
            if (index < 0 || index >= DIRECTION_ANGLES.Length)
                return 0f;

            return DIRECTION_ANGLES[index];
        }
    }
}
