using System;
using UnityEngine;

namespace QuickWheel.Core.States
{
    /// <summary>
    /// 轮盘状态管理器
    /// 管理轮盘的状态和槽位数据
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public class WheelStateManager<T>
    {
        // === 状态数据 ===
        private WheelState _currentState = WheelState.Hidden;
        private T[] _slots;                    // 槽位数据数组
        private int _selectedIndex = -1;       // 当前选中索引
        private int _hoveredIndex = -1;        // 当前hover索引

        // === 事件 ===
        public event Action<WheelState, WheelState> OnStateChanged;
        public event Action<int, T> OnSlotDataChanged;
        public event Action<int, int> OnSlotsSwapped;

        /// <summary>
        /// 当前状态
        /// </summary>
        public WheelState CurrentState => _currentState;

        /// <summary>
        /// 槽位数量
        /// </summary>
        public int SlotCount => _slots?.Length ?? 0;

        /// <summary>
        /// 选中索引
        /// </summary>
        public int SelectedIndex => _selectedIndex;

        /// <summary>
        /// Hover索引
        /// </summary>
        public int HoveredIndex => _hoveredIndex;

        /// <summary>
        /// 初始化状态管理器
        /// </summary>
        /// <param name="slotCount">槽位数量</param>
        public WheelStateManager(int slotCount)
        {
            if (slotCount < 3 || slotCount > 8)
            {
                Debug.LogError($"Invalid slot count: {slotCount}. Must be between 3 and 8.");
                slotCount = Mathf.Clamp(slotCount, 3, 8);
            }

            _slots = new T[slotCount];
        }

        /// <summary>
        /// 状态转换
        /// </summary>
        /// <param name="newState">新状态</param>
        public void TransitionTo(WheelState newState)
        {
            if (_currentState == newState) return;

            var oldState = _currentState;
            _currentState = newState;

            Debug.Log($"[WheelState] {oldState} → {newState}");
            OnStateChanged?.Invoke(oldState, newState);
        }

        /// <summary>
        /// 是否可以修改数据（只在Hidden或Active状态允许）
        /// </summary>
        public bool CanModifyData()
        {
            return _currentState == WheelState.Hidden || _currentState == WheelState.Active;
        }

        /// <summary>
        /// 获取槽位数据
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>槽位数据</returns>
        public T GetSlot(int index)
        {
            if (index < 0 || index >= _slots.Length)
            {
                Debug.LogWarning($"Invalid slot index: {index}");
                return default(T);
            }

            return _slots[index];
        }

        /// <summary>
        /// 设置槽位数据
        /// </summary>
        /// <param name="index">索引</param>
        /// <param name="item">数据</param>
        /// <returns>true=成功，false=失败（状态不允许）</returns>
        public bool SetSlot(int index, T item)
        {
            if (!CanModifyData())
            {
                Debug.LogWarning($"Cannot modify slot data in state: {_currentState}");
                return false;
            }

            if (index < 0 || index >= _slots.Length)
            {
                Debug.LogWarning($"Invalid slot index: {index}");
                return false;
            }

            _slots[index] = item;
            OnSlotDataChanged?.Invoke(index, item);
            return true;
        }

        /// <summary>
        /// 交换两个槽位
        /// </summary>
        /// <param name="index1">索引1</param>
        /// <param name="index2">索引2</param>
        /// <returns>true=成功，false=失败</returns>
        public bool SwapSlots(int index1, int index2)
        {
            if (!CanModifyData())
            {
                Debug.LogWarning($"Cannot swap slots in state: {_currentState}");
                return false;
            }

            if (index1 < 0 || index1 >= _slots.Length || index2 < 0 || index2 >= _slots.Length)
            {
                Debug.LogWarning($"Invalid slot indices: {index1}, {index2}");
                return false;
            }

            // 交换数据
            T temp = _slots[index1];
            _slots[index1] = _slots[index2];
            _slots[index2] = temp;

            OnSlotsSwapped?.Invoke(index1, index2);
            return true;
        }

        /// <summary>
        /// 设置选中索引
        /// </summary>
        /// <param name="index">索引（-1表示无选中）</param>
        public void SetSelectedIndex(int index)
        {
            if (index < -1 || index >= _slots.Length)
            {
                Debug.LogWarning($"Invalid selected index: {index}");
                return;
            }

            _selectedIndex = index;
        }

        /// <summary>
        /// 设置hover索引
        /// </summary>
        /// <param name="index">索引（-1表示无hover）</param>
        public void SetHoveredIndex(int index)
        {
            if (index < -1 || index >= _slots.Length)
            {
                return;  // 不打印警告（高频调用）
            }

            _hoveredIndex = index;
        }

        /// <summary>
        /// 清空所有槽位
        /// </summary>
        public void ClearAllSlots()
        {
            if (!CanModifyData())
            {
                Debug.LogWarning($"Cannot clear slots in state: {_currentState}");
                return;
            }

            for (int i = 0; i < _slots.Length; i++)
            {
                _slots[i] = default(T);
                OnSlotDataChanged?.Invoke(i, default(T));
            }
        }

        /// <summary>
        /// 批量设置槽位
        /// </summary>
        /// <param name="items">数据数组（长度必须等于SlotCount）</param>
        /// <returns>true=成功，false=失败</returns>
        public bool SetSlots(T[] items)
        {
            if (!CanModifyData())
            {
                Debug.LogWarning($"Cannot set slots in state: {_currentState}");
                return false;
            }

            if (items == null || items.Length != _slots.Length)
            {
                Debug.LogWarning($"Invalid items array length. Expected {_slots.Length}, got {items?.Length ?? 0}");
                return false;
            }

            for (int i = 0; i < items.Length; i++)
            {
                _slots[i] = items[i];
                OnSlotDataChanged?.Invoke(i, items[i]);
            }

            return true;
        }

        /// <summary>
        /// 获取所有槽位数据的副本
        /// </summary>
        /// <returns>槽位数据数组</returns>
        public T[] GetAllSlots()
        {
            T[] copy = new T[_slots.Length];
            Array.Copy(_slots, copy, _slots.Length);
            return copy;
        }

        /// <summary>
        /// 清除所有事件订阅
        /// </summary>
        public void ClearEvents()
        {
            OnStateChanged = null;
            OnSlotDataChanged = null;
            OnSlotsSwapped = null;
        }
    }
}
