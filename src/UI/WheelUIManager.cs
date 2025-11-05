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
            _wheelCanvas.sortingOrder = 1000;

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
                display.Initialize(wheelItem, i, new Vector2(cellSize, cellSize));
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
            blockerImage.color = new Color(0, 0, 0, 0.3f);  // 半透明黑色
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
        /// 显示轮盘（在鼠标位置）
        /// </summary>
        public void Show()
        {
            if (_wheelContainer == null) return;

            _wheelContainer.SetActive(true);
            _inputBlocker.SetActive(true);

            // 轮盘显示在鼠标位置
            var containerRect = _wheelContainer.GetComponent<RectTransform>();
            containerRect.position = UnityEngine.Input.mousePosition;

            Debug.Log("[WheelUIManager] 轮盘已显示");
        }

        /// <summary>
        /// 隐藏轮盘
        /// </summary>
        public void Hide()
        {
            if (_wheelContainer == null) return;

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
    }
}
