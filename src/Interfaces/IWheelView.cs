using System;
using QuickWheel.Core;

namespace QuickWheel.Core.Interfaces
{
    /// <summary>
    /// 抽象轮盘视图接口，允许为 Wheel 提供自定义的 UI 呈现。
    /// </summary>
    /// <typeparam name="T">槽位数据类型</typeparam>
    public interface IWheelView<T> : IDisposable
    {
        /// <summary>
        /// 视图与轮盘绑定时调用。
        /// </summary>
        void Attach(Wheel<T> wheel, IWheelItemAdapter<T> adapter);

        /// <summary>
        /// 视图与轮盘解绑时调用。
        /// </summary>
        void Detach();

        void OnWheelShown();
        void OnWheelHidden(int finalIndex);
        void OnSlotDataChanged(int index, T data);
        void OnSlotsSwapped(int index1, int index2);
        void OnSelectionChanged(int selectedIndex);
        void OnHoverChanged(int hoveredIndex);

        /// <summary>
        /// 获取轮盘中心位置（屏幕坐标）
        /// </summary>
        UnityEngine.Vector2 GetWheelCenter();
    }
}
