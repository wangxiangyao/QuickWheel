using UnityEngine;

namespace QuickWheel.Core.Interfaces
{
    /// <summary>
    /// 轮盘选择策略接口
    /// 支持不同的选择算法（角度、距离等）
    /// </summary>
    public interface IWheelSelectionStrategy
    {
        /// <summary>
        /// 根据输入位置计算选中的槽位索引
        /// </summary>
        /// <param name="wheelCenter">轮盘中心位置</param>
        /// <param name="inputPosition">输入位置（鼠标/摇杆）</param>
        /// <param name="slotCount">槽位数量</param>
        /// <param name="slotAngles">槽位角度数组</param>
        /// <returns>槽位索引，-1表示无选中（死区内）</returns>
        int GetSlotIndexFromPosition(
            Vector2 wheelCenter,
            Vector2 inputPosition,
            int slotCount,
            float[] slotAngles
        );

        /// <summary>
        /// 判断是否在死区内
        /// </summary>
        /// <param name="wheelCenter">轮盘中心位置</param>
        /// <param name="inputPosition">输入位置</param>
        /// <param name="deadZoneRadius">死区半径</param>
        /// <returns>true=在死区内，false=在死区外</returns>
        bool IsInDeadZone(
            Vector2 wheelCenter,
            Vector2 inputPosition,
            float deadZoneRadius
        );
    }
}
