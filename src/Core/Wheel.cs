using System;
using QuickWheel.Core.Interfaces;
using QuickWheel.Core.States;
using UnityEngine;

namespace QuickWheel.Core
{
    /// <summary>
    /// 轮盘主类（泛型）
    /// 统一入口，协调各个子系统
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public class Wheel<T> : IDisposable
    {
        // === 核心组件 ===
        private WheelStateManager<T> _stateManager;
        private WheelEventBus<T> _eventBus;
        private WheelConfig _config;

        // === 可选组件 ===
        private IWheelDataProvider<T> _dataProvider;
        private IWheelItemAdapter<T> _adapter;
        private IWheelPersistence<T> _persistence;
        private IWheelInputHandler _inputHandler;
        private IWheelSelectionStrategy _selectionStrategy;

        // === 公开属性 ===
        public WheelConfig Config => _config;
        public WheelEventBus<T> EventBus => _eventBus;
        public bool IsVisible => _stateManager.CurrentState != WheelState.Hidden;

        // === 公开事件（方便订阅） ===
        public event Action<int, T> OnItemSelected;
        public event Action OnWheelShown;
        public event Action<int> OnWheelHidden;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="config">配置</param>
        /// <param name="adapter">适配器（必需）</param>
        public Wheel(WheelConfig config, IWheelItemAdapter<T> adapter)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (adapter == null)
                throw new ArgumentNullException(nameof(adapter));

            // 验证配置
            if (!config.Validate(out string error))
            {
                throw new ArgumentException($"Invalid config: {error}");
            }

            _config = config;
            _adapter = adapter;

            // 初始化核心组件
            _stateManager = new WheelStateManager<T>(_config.SlotCount);
            _eventBus = new WheelEventBus<T>();

            // 订阅状态管理器事件
            _stateManager.OnSlotDataChanged += (index, item) => _eventBus.TriggerSlotDataChanged(index, item);
            _stateManager.OnSlotsSwapped += (i1, i2) => _eventBus.TriggerSlotsSwapped(i1, i2);

            // 订阅事件总线转发到公开事件
            _eventBus.OnWheelShown += () => OnWheelShown?.Invoke();
            _eventBus.OnWheelHidden += (index) => OnWheelHidden?.Invoke(index);

            Debug.Log($"[Wheel] Initialized with {_config.SlotCount} slots");
        }

        // === 可选组件设置 ===

        public void SetDataProvider(IWheelDataProvider<T> dataProvider)
        {
            // 取消旧的订阅
            if (_dataProvider != null)
            {
                _dataProvider.OnItemAdded -= HandleDataProviderItemAdded;
                _dataProvider.OnItemRemoved -= HandleDataProviderItemRemoved;
                _dataProvider.OnItemChanged -= HandleDataProviderItemChanged;
            }

            _dataProvider = dataProvider;

            // 订阅新的事件
            if (_dataProvider != null)
            {
                _dataProvider.OnItemAdded += HandleDataProviderItemAdded;
                _dataProvider.OnItemRemoved += HandleDataProviderItemRemoved;
                _dataProvider.OnItemChanged += HandleDataProviderItemChanged;
            }
        }

        public void SetPersistence(IWheelPersistence<T> persistence)
        {
            _persistence = persistence;

            // 如果启用持久化，尝试加载
            if (_config.EnablePersistence && _persistence != null)
            {
                LoadLayout();
            }
        }

        public void SetInputHandler(IWheelInputHandler inputHandler)
        {
            // 取消旧的订阅
            if (_inputHandler != null)
            {
                _inputHandler.OnPositionChanged -= HandleInputPositionChanged;
                _inputHandler.OnConfirm -= HandleInputConfirm;
                _inputHandler.OnCancel -= HandleInputCancel;
            }

            _inputHandler = inputHandler;

            // 订阅新的事件
            if (_inputHandler != null)
            {
                _inputHandler.OnPositionChanged += HandleInputPositionChanged;
                _inputHandler.OnConfirm += HandleInputConfirm;
                _inputHandler.OnCancel += HandleInputCancel;
            }
        }

        public void SetSelectionStrategy(IWheelSelectionStrategy strategy)
        {
            _selectionStrategy = strategy;
        }

        // === 显示与隐藏 ===

        public void Show(Vector2 position)
        {
            if (_stateManager.CurrentState != WheelState.Hidden)
            {
                Debug.LogWarning("[Wheel] Already visible");
                return;
            }

            _stateManager.TransitionTo(WheelState.Showing);
            _eventBus.TriggerWheelShown();

            // TODO: 播放显示动画
            // 动画完成后转换到Active状态
            _stateManager.TransitionTo(WheelState.Active);
        }

        public void Hide(bool executeSelection = true)
        {
            if (_stateManager.CurrentState != WheelState.Active)
            {
                Debug.LogWarning("[Wheel] Not in active state");
                return;
            }

            int finalIndex = -1;

            if (executeSelection && _stateManager.HoveredIndex >= 0)
            {
                finalIndex = _stateManager.HoveredIndex;
                _stateManager.SetSelectedIndex(finalIndex);

                // 触发选中事件
                T selectedItem = _stateManager.GetSlot(finalIndex);
                OnItemSelected?.Invoke(finalIndex, selectedItem);
            }

            _stateManager.TransitionTo(WheelState.Hiding);

            // TODO: 播放隐藏动画
            // 动画完成后转换到Hidden状态
            _stateManager.TransitionTo(WheelState.Hidden);

            _eventBus.TriggerWheelHidden(finalIndex);
        }

        // === 槽位操作 ===

        public void SetSlot(int index, T item)
        {
            _stateManager.SetSlot(index, item);
        }

        public T GetSlot(int index)
        {
            return _stateManager.GetSlot(index);
        }

        public void RemoveSlot(int index)
        {
            _stateManager.SetSlot(index, default(T));
        }

        public void SwapSlots(int fromIndex, int toIndex)
        {
            if (_stateManager.SwapSlots(fromIndex, toIndex))
            {
                // 保存布局
                if (_config.EnablePersistence && _persistence != null)
                {
                    SaveLayout();
                }
            }
        }

        public void ClearAllSlots()
        {
            _stateManager.ClearAllSlots();
        }

        public void SetSlots(T[] items)
        {
            _stateManager.SetSlots(items);
        }

        // === 选中状态 ===

        public void SetSelectedIndex(int index)
        {
            _stateManager.SetSelectedIndex(index);
            _eventBus.TriggerSelectionChanged(index);
        }

        public int GetSelectedIndex()
        {
            return _stateManager.SelectedIndex;
        }

        public int GetHoveredIndex()
        {
            return _stateManager.HoveredIndex;
        }

        // === 手动控制（不使用InputHandler） ===

        public void ManualSetHover(int index)
        {
            _stateManager.SetHoveredIndex(index);
            _eventBus.TriggerSlotHovered(index);
        }

        public void ManualConfirm()
        {
            Hide(executeSelection: true);
        }

        public void ManualCancel()
        {
            Hide(executeSelection: false);
        }

        // === Update（如果有InputHandler） ===

        public void Update()
        {
            _inputHandler?.OnUpdate();
        }

        // === 持久化 ===

        private void LoadLayout()
        {
            if (_persistence == null || string.IsNullOrEmpty(_config.PersistenceKey))
                return;

            try
            {
                if (_persistence.Has(_config.PersistenceKey))
                {
                    var layoutData = _persistence.Load(_config.PersistenceKey);
                    if (layoutData != null)
                    {
                        _stateManager.SetSelectedIndex(layoutData.SelectedIndex);
                        Debug.Log($"[Wheel] Layout loaded from {_config.PersistenceKey}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Wheel] Failed to load layout: {ex.Message}");
            }
        }

        private void SaveLayout()
        {
            if (_persistence == null || string.IsNullOrEmpty(_config.PersistenceKey))
                return;

            try
            {
                var layoutData = new WheelLayoutData<T>
                {
                    SlotCount = _config.SlotCount,
                    SelectedIndex = _stateManager.SelectedIndex,
                    SlotOrder = GenerateSlotOrder()
                };

                _persistence.Save(_config.PersistenceKey, layoutData);
                Debug.Log($"[Wheel] Layout saved to {_config.PersistenceKey}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Wheel] Failed to save layout: {ex.Message}");
            }
        }

        private int[] GenerateSlotOrder()
        {
            int[] order = new int[_config.SlotCount];
            for (int i = 0; i < _config.SlotCount; i++)
            {
                order[i] = i;
            }
            return order;
        }

        // === 数据提供者事件处理 ===

        private void HandleDataProviderItemAdded(T item)
        {
            // TODO: 实现自动添加逻辑
            Debug.Log($"[Wheel] Item added from data provider");
        }

        private void HandleDataProviderItemRemoved(T item)
        {
            // TODO: 实现自动移除逻辑
            Debug.Log($"[Wheel] Item removed from data provider");
        }

        private void HandleDataProviderItemChanged(T oldItem, T newItem)
        {
            // TODO: 实现自动更新逻辑
            Debug.Log($"[Wheel] Item changed from data provider");
        }

        // === 输入处理事件 ===

        private void HandleInputPositionChanged(Vector2 position)
        {
            if (_stateManager.CurrentState != WheelState.Active)
                return;

            // TODO: 使用SelectionStrategy计算hover索引
            // int hoveredIndex = _selectionStrategy?.GetSlotIndexFromPosition(...);
            // ManualSetHover(hoveredIndex);
        }

        private void HandleInputConfirm()
        {
            ManualConfirm();
        }

        private void HandleInputCancel()
        {
            ManualCancel();
        }

        // === 清理 ===

        public void Dispose()
        {
            // 清除事件订阅
            _stateManager?.ClearEvents();
            _eventBus?.ClearAllEvents();

            // 清除输入处理器订阅
            if (_inputHandler != null)
            {
                _inputHandler.OnPositionChanged -= HandleInputPositionChanged;
                _inputHandler.OnConfirm -= HandleInputConfirm;
                _inputHandler.OnCancel -= HandleInputCancel;
            }

            // 清除数据提供者订阅
            if (_dataProvider != null)
            {
                _dataProvider.OnItemAdded -= HandleDataProviderItemAdded;
                _dataProvider.OnItemRemoved -= HandleDataProviderItemRemoved;
                _dataProvider.OnItemChanged -= HandleDataProviderItemChanged;
            }

            Debug.Log("[Wheel] Disposed");
        }
    }
}
