using System;
using QuickWheel.Core.Interfaces;
using QuickWheel.Core.States;
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

                var selectedItem = _stateManager.GetSlot(finalIndex);
                OnItemSelected?.Invoke(finalIndex, selectedItem);
            }

            _stateManager.TransitionTo(WheelState.Hiding);
            _stateManager.TransitionTo(WheelState.Hidden);

            _eventBus.TriggerWheelHidden(finalIndex);

            if (_view != null)
            {
                _view.OnWheelHidden(finalIndex);
            }
        }

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
            _stateManager.SetHoveredIndex(index);
            _eventBus.TriggerSlotHovered(index);
        }

        public void ManualConfirm()
        {
            Hide(true);
        }

        public void ManualCancel()
        {
            Hide(false);
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
                return;
            }

            if (_selectionStrategy == null || _view == null)
            {
                return;
            }

            // Selection strategy 会在业务层调用 ManualSetHover，因此此处仅保留扩展点。
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
