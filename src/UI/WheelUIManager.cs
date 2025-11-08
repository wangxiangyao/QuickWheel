using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using QuickWheel.Core;
using QuickWheel.Core.Interfaces;
using QuickWheel.Utils;

namespace QuickWheel.UI
{
    /// <summary>
    /// 9宫格轮盘UI管理器
    /// 负责创建、显示和更新9宫格UI
    /// 自动集成到Wheel核心类，无需手动创建UI
    /// </summary>
    public class WheelUIManager<T>
    {
        // UI组件
        private Canvas _wheelCanvas;
        private GameObject _wheelContainer;
        private List<WheelSlotDisplay> _slotDisplays = new List<WheelSlotDisplay>();
        private GameObject _inputBlocker;

        // 轮盘引用
        private Wheel<T> _wheel;
        private IWheelItemAdapter<T> _adapter;

        // 当前选中索引
        private int _currentSelectedIndex = -1;

        // 轮盘显示时的中心位置
        private Vector2 _wheelCenter;

        // 🆕 拖拽状态标志（用于暂停输入处理）
        private bool _isDragging = false;
        public bool IsDragging => _isDragging;

        // 9宫格位置映射（屏幕坐标，相对于轮盘中心）
        private static readonly Vector2Int[] GRID_POSITIONS = new Vector2Int[]
        {
            new Vector2Int(-1,  0),  // 0: 左中
            new Vector2Int( 1,  0),  // 1: 右中
            new Vector2Int( 0, -1),  // 2: 上中
            new Vector2Int( 0,  1),  // 3: 下中
            new Vector2Int(-1,  1),  // 4: 左下
            new Vector2Int( 1,  1),  // 5: 右下
            new Vector2Int( 1, -1),  // 6: 右上
            new Vector2Int(-1, -1),  // 7: 左上
            new Vector2Int( 0,  0),  // 8: 中心（不使用）
        };

        /// <summary>
        /// 是否已显示
        /// </summary>
        public bool IsVisible => _wheelContainer != null && _wheelContainer.activeSelf;

        /// <summary>
        /// 轮盘容器RectTransform
        /// </summary>
        public RectTransform ContainerRect => _wheelContainer?.GetComponent<RectTransform>();

        /// <summary>
        /// 初始化UI管理器
        /// </summary>
        public WheelUIManager(Wheel<T> wheel, IWheelItemAdapter<T> adapter, Transform parent = null)
        {
            _wheel = wheel ?? throw new ArgumentNullException(nameof(wheel));
            _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));

            CreateUI(parent);
            SubscribeToWheelEvents();
        }

        /// <summary>
        /// 创建9宫格UI
        /// </summary>
        private void CreateUI(Transform parent)
        {
            // 创建Canvas
            var canvasObj = new GameObject("QuickWheelCanvas");
            if (parent != null)
                canvasObj.transform.SetParent(parent, false);

            _wheelCanvas = canvasObj.AddComponent<Canvas>();
            _wheelCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _wheelCanvas.sortingOrder = 10000;  // 提高层级，确保在游戏UI之上

            var canvasScaler = canvasObj.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);

            canvasObj.AddComponent<GraphicRaycaster>();

            // 创建输入拦截面板（防止输入传递到游戏）
            CreateInputBlocker(canvasObj.transform);

            // 创建轮盘容器
            _wheelContainer = new GameObject("WheelContainer");
            _wheelContainer.transform.SetParent(canvasObj.transform, false);

            var containerRect = _wheelContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            containerRect.sizeDelta = new Vector2(200, 200);

            // 创建9个格子
            float cellSize = _wheel.Config.GridCellSize;
            float spacing = _wheel.Config.GridSpacing;
            float offset = cellSize + spacing;

            for (int i = 0; i < 9; i++)
            {
                // 跳过中心格子（索引8）
                if (i == 8) continue;

                var slotObj = new GameObject($"Slot_{i}");
                slotObj.transform.SetParent(_wheelContainer.transform, false);

                var slotRect = slotObj.AddComponent<RectTransform>();
                slotRect.anchorMin = new Vector2(0.5f, 0.5f);
                slotRect.anchorMax = new Vector2(0.5f, 0.5f);

                // 计算位置
                Vector2 gridPos = new Vector2(
                    GRID_POSITIONS[i].x * offset,
                    -GRID_POSITIONS[i].y * offset  // Y轴反转（Unity UI坐标系）
                );
                slotRect.anchoredPosition = gridPos;

                // 添加显示组件
                var display = slotObj.AddComponent<WheelSlotDisplay>();
                var wheelItem = ConvertToWheelItem(_wheel.GetSlot(i));
                display.Initialize(wheelItem, i, new Vector2(cellSize, cellSize), _wheel.Config, this);  // 🆕 传入UIManager引用用于拖拽交换
                _slotDisplays.Add(display);
            }

            // 初始隐藏
            _wheelContainer.SetActive(false);
            _inputBlocker.SetActive(false);

            Debug.Log("[WheelUIManager] 9宫格UI已创建");
        }

        /// <summary>
        /// 创建输入拦截面板
        /// </summary>
        private void CreateInputBlocker(Transform parent)
        {
            _inputBlocker = new GameObject("InputBlocker");
            _inputBlocker.transform.SetParent(parent, false);

            var blockerRect = _inputBlocker.AddComponent<RectTransform>();
            blockerRect.anchorMin = Vector2.zero;
            blockerRect.anchorMax = Vector2.one;
            blockerRect.offsetMin = Vector2.zero;
            blockerRect.offsetMax = Vector2.zero;

            var blockerImage = _inputBlocker.AddComponent<Image>();
            // 使用透明背景拦截输入
            // Unity特性：alpha=0时不会拦截射线检测，需要极小的alpha值
            blockerImage.color = new Color(0, 0, 0, 0.01f);  // 几乎透明的背景，拦截所有输入
            blockerImage.raycastTarget = true;

            // 确保在轮盘容器下方
            _inputBlocker.transform.SetAsFirstSibling();
        }

        /// <summary>
        /// 订阅轮盘事件
        /// </summary>
        private void SubscribeToWheelEvents()
        {
            _wheel.EventBus.OnSlotDataChanged += OnSlotDataChanged;
            _wheel.EventBus.OnSlotsSwapped += OnSlotsSwapped;
        }

        /// <summary>
        /// 🆕 标准点击处理方法
        /// 处理槽位点击事件，触发轮盘的点击逻辑
        /// </summary>
        /// <param name="slotIndex">点击的槽位索引</param>
        public void HandleSlotClick(int slotIndex)
        {
            Debug.Log($"[WheelUIManager] 🟣 HandleSlotClick called: slotIndex={slotIndex}");

            // 通过EventBus触发点击事件，让Wheel处理选择和关闭逻辑
            _wheel.EventBus.TriggerSlotClicked(slotIndex);

            Debug.Log($"[WheelUIManager] 🟣 HandleSlotClick finished");
        }

        /// <summary>
        /// 显示轮盘
        /// </summary>
        /// <param name="centerPosition">轮盘中心位置（可选，为null则使用当前鼠标位置）</param>
        public void Show(Vector2? centerPosition = null)
        {
            if (_wheelContainer == null) return;

            _wheelContainer.SetActive(true);
            _inputBlocker.SetActive(true);

            // 使用提供的中心位置，或当前鼠标位置
            if (centerPosition.HasValue)
            {
                _wheelCenter = centerPosition.Value;
                Debug.Log($"[WheelUIManager] 使用预设中心位置: {_wheelCenter}");
            }
            else
            {
                _wheelCenter = UnityEngine.Input.mousePosition;
                Debug.Log($"[WheelUIManager] 使用当前鼠标位置: {_wheelCenter}");
            }

            // 轮盘显示在中心位置
            var containerRect = _wheelContainer.GetComponent<RectTransform>();
            containerRect.position = _wheelCenter;

            Debug.Log($"[WheelUIManager] 轮盘已显示，中心位置: {_wheelCenter}");
        }

        /// <summary>
        /// 获取轮盘中心位置
        /// </summary>
        public Vector2 GetWheelCenter()
        {
            return _wheelCenter;
        }

        /// <summary>
        /// 隐藏轮盘
        /// </summary>
        public void Hide()
        {
            if (_wheelContainer == null) return;

            // 兜底：关闭前强制清理所有拖拽状态与 hover，避免 EndDrag/Drop 丢失导致残留
            foreach (var display in _slotDisplays)
            {
                display.ForceCleanupDrag();
            }
            UpdateHover(-1);

            _wheelContainer.SetActive(false);
            _inputBlocker.SetActive(false);

            // 清除选中
            UpdateSelection(-1);

            Debug.Log("[WheelUIManager] 轮盘已隐藏");
        }

        /// <summary>
        /// 更新选中状态
        /// </summary>
        public void UpdateSelection(int selectedIndex)
        {
            if (selectedIndex == _currentSelectedIndex) return;

            _currentSelectedIndex = selectedIndex;

            foreach (var display in _slotDisplays)
            {
                display.SetSelected(display.GetSlotIndex() == selectedIndex);
            }
        }

        /// <summary>
        /// 更新悬停状态
        /// </summary>
        public void UpdateHover(int hoveredIndex)
        {
            foreach (var display in _slotDisplays)
            {
                display.SetHovered(display.GetSlotIndex() == hoveredIndex);
            }
        }

        /// <summary>
        /// 槽位数据变化事件处理
        /// </summary>
        private void OnSlotDataChanged(int index, T data)
        {
            if (index < 0 || index >= _slotDisplays.Count) return;

            var wheelItem = ConvertToWheelItem(data);
            _slotDisplays[index].SetData(wheelItem);
        }

        /// <summary>
        /// 槽位交换事件处理
        /// </summary>
        private void OnSlotsSwapped(int index1, int index2)
        {
            if (index1 < 0 || index1 >= _slotDisplays.Count) return;
            if (index2 < 0 || index2 >= _slotDisplays.Count) return;

            // 🆕 选中状态跟随物品移动
            if (_currentSelectedIndex == index1)
            {
                Debug.Log($"[WheelUIManager] Selected index moved: {index1} -> {index2}");
                UpdateSelection(index2);
            }
            else if (_currentSelectedIndex == index2)
            {
                Debug.Log($"[WheelUIManager] Selected index moved: {index2} -> {index1}");
                UpdateSelection(index1);
            }

            // 刷新两个槽位的显示
            var data1 = _wheel.GetSlot(index1);
            var data2 = _wheel.GetSlot(index2);

            _slotDisplays[index1].SetData(ConvertToWheelItem(data1));
            _slotDisplays[index2].SetData(ConvertToWheelItem(data2));
        }

        /// <summary>
        /// 将数据转换为IWheelItem
        /// </summary>
        private IWheelItem ConvertToWheelItem(T data)
        {
            if (data == null) return null;
            return _adapter.ToWheelItem(data);
        }

        /// <summary>
        /// 刷新所有槽位显示
        /// </summary>
        public void RefreshAllSlots()
        {
            for (int i = 0; i < _slotDisplays.Count; i++)
            {
                var data = _wheel.GetSlot(i);
                _slotDisplays[i].SetData(ConvertToWheelItem(data));
            }
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Dispose()
        {
            if (_wheel != null)
            {
                _wheel.EventBus.OnSlotDataChanged -= OnSlotDataChanged;
                _wheel.EventBus.OnSlotsSwapped -= OnSlotsSwapped;
            }

            if (_wheelCanvas != null)
            {
                UnityEngine.Object.Destroy(_wheelCanvas.gameObject);
            }

            _slotDisplays.Clear();
        }

        /// <summary>
        /// 🆕 交换两个槽位的数据（拖拽功能）
        /// </summary>
        /// <summary>
        /// 🆕 设置拖拽状态（用于暂停输入处理）
        /// </summary>
        public void SetDragging(bool isDragging)
        {
            _isDragging = isDragging;
            Debug.Log($"[WheelUIManager] Dragging state: {_isDragging}");
        }

        public void SwapSlots(int fromIndex, int toIndex)
        {
            Debug.Log($"[WheelUIManager] SwapSlots: {fromIndex} <-> {toIndex}");

            // 边界检查
            if (fromIndex < 0 || fromIndex >= _slotDisplays.Count ||
                toIndex < 0 || toIndex >= _slotDisplays.Count)
            {
                Debug.LogWarning($"[WheelUIManager] Invalid slot indices: {fromIndex}, {toIndex}");
                return;
            }

            // 调用 Wheel 核心进行数据交换（这会触发 EventBus 事件）
            if (_wheel != null)
            {
                _wheel.SwapSlots(fromIndex, toIndex);
                Debug.Log($"[WheelUIManager] Called Wheel.SwapSlots({fromIndex}, {toIndex})");

                // 立即刷新UI显示
                RefreshSlot(fromIndex);
                RefreshSlot(toIndex);
            }
            else
            {
                Debug.LogWarning("[WheelUIManager] Wheel is null, cannot swap slots");
            }
        }

        /// <summary>
        /// 刷新单个槽位的显示
        /// </summary>
        private void RefreshSlot(int index)
        {
            if (index < 0 || index >= _slotDisplays.Count) return;

            var display = _slotDisplays[index];
            var wheelItem = ConvertToWheelItem(_wheel.GetSlot(index));
            display.SetData(wheelItem);
        }

        /// <summary>
        /// 🆕 选中指定槽位并关闭轮盘
        /// </summary>
        /// <param name="index">要选中的槽位索引</param>
        public void SelectAndClose(int index)
        {
            Debug.Log($"[WheelUIManager] SelectAndClose: {index}");

            if (index < 0 || index >= _slotDisplays.Count)
            {
                Debug.LogWarning($"[WheelUIManager] Invalid slot index: {index}");
                return;
            }

            // 设置选中索引
            _wheel.SetSelectedIndex(index);

            // 立即关闭轮盘（执行选择）
            _wheel.Hide(true);
        }
    }
}
