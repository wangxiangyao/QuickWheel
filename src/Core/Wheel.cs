using System;
using QuickWheel.Core.Interfaces;
using QuickWheel.Core.States;
using QuickWheel.UI;
using UnityEngine;

namespace QuickWheel.Core
{
    /// <summary>
    /// 轮盘主类，负责管理槽位数据、状态流转以及事件广播。
    /// UI 展示由外部注入的 IWheelView<T> 处理。
    /// </summary>
    /// <typeparam name="T">槽位数据类型</typeparam>
    public class Wheel<T> : IDisposable
    {
        private readonly WheelStateManager<T> _stateManager;
        private readonly WheelEventBus<T> _eventBus;
        private readonly WheelConfig _config;

        private IWheelDataProvider<T> _dataProvider;
        private IWheelItemAdapter<T> _adapter;
        private IWheelPersistence<T> _persistence;
        private IWheelInputHandler _inputHandler;
        private IWheelSelectionStrategy _selectionStrategy;
        private IWheelView<T> _view;

        public WheelConfig Config => _config;
        public WheelEventBus<T> EventBus => _eventBus;
        public bool IsVisible => _stateManager.CurrentState != WheelState.Hidden;

        public event Action<int, T> OnItemSelected;
        public event Action OnWheelShown;
        public event Action<int> OnWheelHidden;

        public Wheel(WheelConfig config, IWheelItemAdapter<T> adapter)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (adapter == null)
            {
                throw new ArgumentNullException(nameof(adapter));
            }

            if (!config.Validate(out var error))
            {
                throw new ArgumentException($"Invalid config: {error}", nameof(config));
            }

            _config = config;
            _adapter = adapter;

            _stateManager = new WheelStateManager<T>(_config.SlotCount);
            _eventBus = new WheelEventBus<T>();

            _stateManager.OnSlotDataChanged += HandleSlotDataChanged;
            _stateManager.OnSlotsSwapped += HandleSlotsSwapped;

            _eventBus.OnWheelShown += () => OnWheelShown?.Invoke();
            _eventBus.OnWheelHidden += index => OnWheelHidden?.Invoke(index);
            _eventBus.OnSelectionChanged += index =>
            {
                if (_view != null)
                {
                    _view.OnSelectionChanged(index);
                }
            };
            _eventBus.OnSlotHovered += index =>
            {
                if (_view != null)
                {
                    _view.OnHoverChanged(index);
                }
            };

            // 🆕 订阅点击事件，处理用户点击
            _eventBus.OnSlotClicked += clickedIndex =>
            {
                Debug.Log($"[Wheel] 🟡 OnSlotClicked event fired: clickedIndex={clickedIndex}, CurrentState={_stateManager.CurrentState}");

                // 🆕 点击时：只更新选中状态，不使用物品（参考 backpack_quickwheel 的 ChangeSelection 模式）
                // 注意：不能在这里调用 SetSelectedIndex，因为事件锁会阻塞嵌套事件
                // 解决方案：先更新 HoveredIndex，然后在 Hide 中同步到 SelectedIndex
                _stateManager.SetHoveredIndex(clickedIndex);
                Debug.Log($"[Wheel] 🟡 Updated hovered index to {clickedIndex} (will sync to selected)");

                // 关闭轮盘，传入特殊模式标记：executeSelection=true 但不使用物品
                HideAndUpdateSelection();

                Debug.Log($"[Wheel] 🟡 OnSlotClicked event handler finished");
            };

            Debug.Log($"[Wheel] Initialized with {_config.SlotCount} slots");
        }

        public void SetDataProvider(IWheelDataProvider<T> dataProvider)
        {
            if (_dataProvider != null)
            {
                _dataProvider.OnItemAdded -= HandleDataProviderItemAdded;
                _dataProvider.OnItemRemoved -= HandleDataProviderItemRemoved;
                _dataProvider.OnItemChanged -= HandleDataProviderItemChanged;
            }

            _dataProvider = dataProvider;

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

            if (_config.EnablePersistence && _persistence != null)
            {
                LoadLayout();
            }
        }

        public void SetInputHandler(IWheelInputHandler inputHandler)
        {
            if (_inputHandler != null)
            {
                _inputHandler.OnPositionChanged -= HandleInputPositionChanged;
                _inputHandler.OnConfirm -= HandleInputConfirm;
                _inputHandler.OnCancel -= HandleInputCancel;
            }

            _inputHandler = inputHandler;

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

        public void SetView(IWheelView<T> view)
        {
            if (_view == view)
            {
                return;
            }

            if (_view != null)
            {
                try
                {
                    _view.Detach();
                }
                finally
                {
                    _view.Dispose();
                }
            }

            _view = view;

            if (_view != null)
            {
                _view.Attach(this, _adapter);

                var snapshot = _stateManager.GetAllSlots();
                for (int i = 0; i < snapshot.Length; i++)
                {
                    _view.OnSlotDataChanged(i, snapshot[i]);
                }

                _view.OnSelectionChanged(_stateManager.SelectedIndex);
                _view.OnHoverChanged(_stateManager.HoveredIndex);
            }
        }

        public void Show()
        {
            if (_stateManager.CurrentState != WheelState.Hidden)
            {
                Debug.LogWarning("[Wheel] Already visible");
                return;
            }

            _stateManager.TransitionTo(WheelState.Showing);
            _eventBus.TriggerWheelShown();

            if (_view != null)
            {
                _view.OnWheelShown();
            }

            _stateManager.TransitionTo(WheelState.Active);
        }

        public void Hide(bool executeSelection = true, bool syncHoverToSelected = false)
        {
            Debug.Log($"[Wheel] 🔴 Hide called: executeSelection={executeSelection}, syncHoverToSelected={syncHoverToSelected}, CurrentState={_stateManager.CurrentState}, HoveredIndex={_stateManager.HoveredIndex}");

            if (_stateManager.CurrentState != WheelState.Active)
            {
                Debug.LogWarning($"[Wheel] ❌ Hide aborted: Not in active state (State={_stateManager.CurrentState})");
                return;
            }

            int finalIndex = -1;
            int selectedIndexToSync = -1;  // 🆕 用于延迟触发 OnSelectionChanged

            // 🆕 点击模式：同步hover到selected，但不使用物品
            if (syncHoverToSelected && _stateManager.HoveredIndex >= 0)
            {
                finalIndex = _stateManager.HoveredIndex;
                Debug.Log($"[Wheel] 🔴 Hide: Syncing hover to selected: {finalIndex}");
                _stateManager.SetSelectedIndex(finalIndex);
                selectedIndexToSync = finalIndex;  // 标记需要触发事件
            }
            // 松开快捷键模式：使用hover的物品
            else if (executeSelection && _stateManager.HoveredIndex >= 0)
            {
                finalIndex = _stateManager.HoveredIndex;
                Debug.Log($"[Wheel] 🔴 Hide: Will select item at index={finalIndex}");
                _stateManager.SetSelectedIndex(finalIndex);

                var selectedItem = _stateManager.GetSlot(finalIndex);
                Debug.Log($"[Wheel] 🔴 Hide: Invoking OnItemSelected for index={finalIndex}, item={selectedItem}");
                OnItemSelected?.Invoke(finalIndex, selectedItem);
            }
            else
            {
                Debug.Log($"[Wheel] 🔴 Hide: No selection (executeSelection={executeSelection}, HoveredIndex={_stateManager.HoveredIndex})");
            }

            Debug.Log($"[Wheel] 🔴 Hide: Transitioning to Hidden state");
            _stateManager.TransitionTo(WheelState.Hiding);
            _stateManager.TransitionTo(WheelState.Hidden);

            _eventBus.TriggerWheelHidden(finalIndex);

            if (_view != null)
            {
                _view.OnWheelHidden(finalIndex);
            }

            Debug.Log($"[Wheel] 🔴 Hide finished: finalIndex={finalIndex}");

            // 🆕 点击模式：直接调用 OnSelectionChanged 事件，绕过事件锁
            // 因为我们在 Hide 方法末尾，已经脱离了事件处理流程
            if (selectedIndexToSync >= 0)
            {
                Debug.Log($"[Wheel] 🔵 Syncing selection change: {selectedIndexToSync}");

                // 通知 View
                if (_view != null)
                {
                    _view.OnSelectionChanged(selectedIndexToSync);
                }

                // 直接调用事件，不通过 EventBus（避免事件锁）
                if (OnSelectionChanged != null)
                {
                    try
                    {
                        foreach (var handler in OnSelectionChanged.GetInvocationList())
                        {
                            try
                            {
                                ((Action<int>)handler)(selectedIndexToSync);
                            }
                            catch (System.Exception ex)
                            {
                                Debug.LogError($"[Wheel] Error invoking OnSelectionChanged handler: {ex.Message}");
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"[Wheel] Error invoking OnSelectionChanged: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// 🆕 选中状态改变事件（绕过 EventBus，直接订阅）
        /// </summary>
        public event Action<int> OnSelectionChanged;

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
            if (_stateManager.SwapSlots(fromIndex, toIndex) && _config.EnablePersistence && _persistence != null)
            {
                SaveLayout();
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

        public void ManualSetHover(int index)
        {
            Debug.Log($"[Wheel] 🔵 ManualSetHover called: index={index}, currentHovered={_stateManager.HoveredIndex}");
            _stateManager.SetHoveredIndex(index);
            _eventBus.TriggerSlotHovered(index);
            Debug.Log($"[Wheel] 🔵 ManualSetHover finished: newHovered={_stateManager.HoveredIndex}");
        }

        /// <summary>
        /// 🆕 循环切换选中项（用于滚轮输入）
        /// 直接切换 SelectedIndex 并触发 OnSelectionChanged 事件（同步快捷栏）
        /// </summary>
        /// <param name="direction">方向：正数向下一个，负数向上一个</param>
        public void CycleSelection(int direction)
        {
            if (_stateManager.CurrentState != WheelState.Active)
            {
                return;
            }

            // 从当前选中索引开始
            int currentIndex = _stateManager.SelectedIndex;
            if (currentIndex < 0)
            {
                currentIndex = 0;
            }

            // 查找下一个非空槽位
            int nextIndex = currentIndex;
            int attempts = 0;
            int maxAttempts = _config.SlotCount;

            do
            {
                nextIndex = (nextIndex + (direction > 0 ? 1 : -1) + _config.SlotCount) % _config.SlotCount;
                attempts++;

                if (_stateManager.GetSlot(nextIndex) != null || attempts >= maxAttempts)
                {
                    break;
                }
            } while (attempts < maxAttempts);

            // 只有找到非空槽位才切换
            if (_stateManager.GetSlot(nextIndex) != null)
            {
                Debug.Log($"[Wheel] 滚轮切换选中: {currentIndex} → {nextIndex}");

                // 直接设置 SelectedIndex
                _stateManager.SetSelectedIndex(nextIndex);

                // 触发 OnSelectionChanged 事件（通知外部同步快捷栏）
                if (OnSelectionChanged != null)
                {
                    try
                    {
                        foreach (var handler in OnSelectionChanged.GetInvocationList())
                        {
                            try
                            {
                                ((System.Action<int>)handler)(nextIndex);
                            }
                            catch (System.Exception ex)
                            {
                                UnityEngine.Debug.LogError($"[Wheel] Error invoking OnSelectionChanged: {ex.Message}");
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        UnityEngine.Debug.LogError($"[Wheel] Error invoking OnSelectionChanged: {ex.Message}");
                    }
                }

                // 通知 View 更新选中状态
                if (_view != null)
                {
                    _view.OnSelectionChanged(nextIndex);
                }
            }
        }

        public void ManualConfirm()
        {
            Debug.Log($"[Wheel] 🟢 ManualConfirm called: State={_stateManager.CurrentState}, HoveredIndex={_stateManager.HoveredIndex}");
            HideAndUpdateSelection();
            Debug.Log($"[Wheel] 🟢 ManualConfirm finished");
        }

        public void ManualCancel()
        {
            Hide(false);
        }

        /// <summary>
        /// 🆕 隐藏轮盘并更新选中状态（用于点击选择）
        /// 点击模式：同步hover到selected，但不使用物品
        /// </summary>
        private void HideAndUpdateSelection()
        {
            Debug.Log($"[Wheel] 🟢 HideAndUpdateSelection called: HoveredIndex={_stateManager.HoveredIndex}");
            Hide(executeSelection: false, syncHoverToSelected: true);
            Debug.Log($"[Wheel] 🟢 HideAndUpdateSelection finished");
        }

        public void Update()
        {
            _inputHandler?.OnUpdate();
        }

        private void LoadLayout()
        {
            if (_persistence == null || string.IsNullOrEmpty(_config.PersistenceKey))
            {
                return;
            }

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
            {
                return;
            }

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
            var order = new int[_config.SlotCount];
            for (int i = 0; i < _config.SlotCount; i++)
            {
                order[i] = i;
            }
            return order;
        }

        private void HandleSlotDataChanged(int index, T item)
        {
            _eventBus.TriggerSlotDataChanged(index, item);
            if (_view != null)
            {
                _view.OnSlotDataChanged(index, item);
            }
        }

        private void HandleSlotsSwapped(int index1, int index2)
        {
            _eventBus.TriggerSlotsSwapped(index1, index2);
            if (_view != null)
            {
                _view.OnSlotsSwapped(index1, index2);
            }
        }

        private void HandleDataProviderItemAdded(T item)
        {
            Debug.Log("[Wheel] Item added from data provider");
        }

        private void HandleDataProviderItemRemoved(T item)
        {
            Debug.Log("[Wheel] Item removed from data provider");
        }

        private void HandleDataProviderItemChanged(T oldItem, T newItem)
        {
            Debug.Log("[Wheel] Item changed from data provider");
        }

        private void HandleInputPositionChanged(Vector2 position)
        {
            if (_stateManager.CurrentState != WheelState.Active)
            {
                Debug.Log($"[Wheel] HandleInputPositionChanged skipped - State: {_stateManager.CurrentState}");
                return;
            }

            // 🆕 拖拽时跳过输入处理，避免选中状态跟着变化
            if (_view is DefaultWheelView<T> defaultView)
            {
                // 通过反射获取 UIManager 的拖拽状态
                var uiManagerField = defaultView.GetType().GetField("_uiManager",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (uiManagerField != null)
                {
                    var uiManager = uiManagerField.GetValue(defaultView);
                    if (uiManager != null)
                    {
                        var isDraggingProp = uiManager.GetType().GetProperty("IsDragging");
                        if (isDraggingProp != null)
                        {
                            bool isDragging = (bool)isDraggingProp.GetValue(uiManager);
                            if (isDragging)
                            {
                                // Debug.Log($"[Wheel] HandleInputPositionChanged skipped - Dragging");
                                return;
                            }
                        }
                    }
                }
            }

            if (_selectionStrategy == null || _view == null)
            {
                Debug.LogWarning($"[Wheel] HandleInputPositionChanged skipped - Strategy: {_selectionStrategy != null}, View: {_view != null}");
                return;
            }

            // 创建选择上下文
            var context = new WheelSelectionContext
            {
                InputPosition = position,
                WheelCenter = _view.GetWheelCenter(),
                Config = _config
            };

            // 使用选择策略计算选中索引
            int selectedIndex = _selectionStrategy.GetSelectedIndex(context);

            // Debug.Log($"[Wheel] Position: {position}, Center: {context.WheelCenter}, Selected: {selectedIndex}, Current: {_stateManager.HoveredIndex}");

            // 更新悬停状态
            if (selectedIndex != _stateManager.HoveredIndex)
            {
                _stateManager.SetHoveredIndex(selectedIndex);
                _eventBus.TriggerSlotHovered(selectedIndex);
            }
        }

        private void HandleInputConfirm()
        {
            ManualConfirm();
        }

        private void HandleInputCancel()
        {
            ManualCancel();
        }

        public void Dispose()
        {
            _stateManager.ClearEvents();
            _eventBus.ClearAllEvents();

            if (_inputHandler != null)
            {
                _inputHandler.OnPositionChanged -= HandleInputPositionChanged;
                _inputHandler.OnConfirm -= HandleInputConfirm;
                _inputHandler.OnCancel -= HandleInputCancel;
                _inputHandler = null;
            }

            if (_dataProvider != null)
            {
                _dataProvider.OnItemAdded -= HandleDataProviderItemAdded;
                _dataProvider.OnItemRemoved -= HandleDataProviderItemRemoved;
                _dataProvider.OnItemChanged -= HandleDataProviderItemChanged;
                _dataProvider = null;
            }

            if (_view != null)
            {
                try
                {
                    _view.Detach();
                }
                finally
                {
                    _view.Dispose();
                    _view = null;
                }
            }

            Debug.Log("[Wheel] Disposed");
        }
    }
}

