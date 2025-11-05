using System;

namespace QuickWheel.Core
{
    /// <summary>
    /// 轮盘事件总线
    /// 解耦事件通信，避免直接依赖
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public class WheelEventBus<T>
    {
        // === 数据变更事件 ===

        /// <summary>
        /// 槽位数据变更事件（索引, 新数据）
        /// </summary>
        public event Action<int, T> OnSlotDataChanged;

        /// <summary>
        /// 槽位交换事件（索引1, 索引2）
        /// </summary>
        public event Action<int, int> OnSlotsSwapped;

        // === 选中状态事件 ===

        /// <summary>
        /// 选中状态变更事件（新选中索引）
        /// </summary>
        public event Action<int> OnSelectionChanged;

        /// <summary>
        /// Hover状态变更事件（hover索引，-1表示无hover）
        /// </summary>
        public event Action<int> OnSlotHovered;

        // === 生命周期事件 ===

        /// <summary>
        /// 轮盘显示事件
        /// </summary>
        public event Action OnWheelShown;

        /// <summary>
        /// 轮盘隐藏事件（最终选中索引，-1表示取消）
        /// </summary>
        public event Action<int> OnWheelHidden;

        // === 交互事件 ===

        /// <summary>
        /// 槽位点击事件（点击的索引）
        /// </summary>
        public event Action<int> OnSlotClicked;

        /// <summary>
        /// 拖拽交换事件（from索引, to索引）
        /// </summary>
        public event Action<int, int> OnSlotDragSwapped;

        // === 防循环触发锁 ===
        private bool _isEventLocked = false;

        /// <summary>
        /// 触发事件（带锁保护）
        /// </summary>
        /// <param name="eventAction">事件动作</param>
        public void FireEvent(Action eventAction)
        {
            if (_isEventLocked) return;

            try
            {
                _isEventLocked = true;
                eventAction?.Invoke();
            }
            finally
            {
                _isEventLocked = false;
            }
        }

        /// <summary>
        /// 触发槽位数据变更事件
        /// </summary>
        public void TriggerSlotDataChanged(int index, T newData)
        {
            FireEvent(() => OnSlotDataChanged?.Invoke(index, newData));
        }

        /// <summary>
        /// 触发槽位交换事件
        /// </summary>
        public void TriggerSlotsSwapped(int index1, int index2)
        {
            FireEvent(() => OnSlotsSwapped?.Invoke(index1, index2));
        }

        /// <summary>
        /// 触发选中状态变更事件
        /// </summary>
        public void TriggerSelectionChanged(int newIndex)
        {
            FireEvent(() => OnSelectionChanged?.Invoke(newIndex));
        }

        /// <summary>
        /// 触发Hover状态变更事件
        /// </summary>
        public void TriggerSlotHovered(int hoveredIndex)
        {
            // 高频事件，不使用锁（性能优化）
            OnSlotHovered?.Invoke(hoveredIndex);
        }

        /// <summary>
        /// 触发轮盘显示事件
        /// </summary>
        public void TriggerWheelShown()
        {
            FireEvent(() => OnWheelShown?.Invoke());
        }

        /// <summary>
        /// 触发轮盘隐藏事件
        /// </summary>
        public void TriggerWheelHidden(int finalIndex)
        {
            FireEvent(() => OnWheelHidden?.Invoke(finalIndex));
        }

        /// <summary>
        /// 触发槽位点击事件
        /// </summary>
        public void TriggerSlotClicked(int clickedIndex)
        {
            FireEvent(() => OnSlotClicked?.Invoke(clickedIndex));
        }

        /// <summary>
        /// 触发拖拽交换事件
        /// </summary>
        public void TriggerSlotDragSwapped(int fromIndex, int toIndex)
        {
            FireEvent(() => OnSlotDragSwapped?.Invoke(fromIndex, toIndex));
        }

        /// <summary>
        /// 清除所有事件订阅
        /// </summary>
        public void ClearAllEvents()
        {
            OnSlotDataChanged = null;
            OnSlotsSwapped = null;
            OnSelectionChanged = null;
            OnSlotHovered = null;
            OnWheelShown = null;
            OnWheelHidden = null;
            OnSlotClicked = null;
            OnSlotDragSwapped = null;
        }
    }
}
