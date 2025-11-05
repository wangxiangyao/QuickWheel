using System;

namespace QuickWheel.Examples.GridWheel
{
    /// <summary>
    /// 9宫格槽位数据结构（简化版）
    /// 直接用null表示空槽位，非null表示有数据
    /// </summary>
    [Serializable]
    public class GridWheelSlot<T>
    {
        /// <summary>
        /// 槽位中的数据，null表示空槽位
        /// </summary>
        public T Data;

        /// <summary>
        /// 槽位索引（0-8）
        /// </summary>
        public int Index;

        /// <summary>
        /// 检查槽位是否有数据
        /// </summary>
        public bool HasData => Data != null;

        /// <summary>
        /// 检查槽位是否为空
        /// </summary>
        public bool IsEmpty => Data == null;

        /// <summary>
        /// 创建空槽位
        /// </summary>
        public static GridWheelSlot<T> CreateEmpty(int index)
        {
            return new GridWheelSlot<T>
            {
                Data = default(T),
                Index = index
            };
        }

        /// <summary>
        /// 创建有数据的槽位
        /// </summary>
        public static GridWheelSlot<T> CreateOccupied(T data, int index)
        {
            return new GridWheelSlot<T>
            {
                Data = data,
                Index = index
            };
        }
    }
}
