using UnityEngine;
using UnityEngine.UI;
using QuickWheel.Core;
using QuickWheel.Utils;
using QuickWheel.Input;
using QuickWheel.Selection;
using QuickWheel.Persistence;
using System.Collections.Generic;

namespace QuickWheel.Examples.GridWheel
{
    /// <summary>
    /// 9宫格轮盘完整示例
    /// 展示如何使用QuickWheel框架创建简单的9宫格轮盘
    ///
    /// 特性：
    /// - 固定9槽位（8个方向 + 1个中心空）
    /// - 手动创建UI（不依赖预制体）
    /// - 鼠标矢量选择（根据方向角度）
    /// - 支持拖拽交换
    /// - 可选持久化
    /// </summary>
    public class GridWheelExample : MonoBehaviour
    {
        [Header("配置")]
        [SerializeField] private KeyCode _triggerKey = KeyCode.V;
        [SerializeField] private bool _enablePersistence = false;

        [Header("测试数据")]
        [SerializeField] private VoiceData[] _testVoices;

        // 轮盘实例
        private Wheel<VoiceData> _wheel;

        // UI组件
        private Canvas _wheelCanvas;
        private GameObject _wheelContainer;
        private List<GridWheelDisplay> _gridDisplays = new List<GridWheelDisplay>();

        // 输入处理
        private MouseWheelInput _input;
        private bool _isWheelVisible = false;

        // 选择逻辑
        private GridSelectionStrategy _selectionStrategy;
        private int _currentHoverIndex = -1;

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

        void Start()
        {
            // 创建轮盘
            CreateWheel();

            // 初始化UI
            InitializeUI();

            // 添加测试数据
            if (_testVoices != null && _testVoices.Length > 0)
            {
                for (int i = 0; i < Mathf.Min(_testVoices.Length, 8); i++)
                {
                    _wheel.SetSlot(i, _testVoices[i]);
                }
            }

            Debug.Log($"[GridWheelExample] 9宫格轮盘已创建，按 {_triggerKey} 键显示");
        }

        /// <summary>
        /// 创建轮盘实例
        /// </summary>
        private void CreateWheel()
        {
            var builder = new WheelBuilder<VoiceData>()
                .WithConfig(config =>
                {
                    config.GridCellSize = 40f;
                    config.GridSpacing = 5f;
                    config.EnableDragSwap = true;
                    config.EnableClickSelect = true;
                    config.DeadZoneRadius = 20f;
                    config.EnablePersistence = _enablePersistence;
                    config.PersistenceKey = "GridWheel";
                })
                .WithAdapter(new VoiceWheelAdapter())
                .OnItemSelected((index, voice) =>
                {
                    if (voice != null)
                    {
                        Debug.Log($"[GridWheelExample] 选中语音: {voice.DisplayName}");
                        PlayVoice(voice);
                    }
                })
                .OnWheelShown(() => Debug.Log("[GridWheelExample] 轮盘显示"))
                .OnWheelHidden((finalIndex) =>
                {
                    if (finalIndex >= 0)
                        Debug.Log($"[GridWheelExample] 轮盘隐藏，选中索引: {finalIndex}");
                    else
                        Debug.Log("[GridWheelExample] 轮盘隐藏，未选择");
                });

            // 添加持久化
            if (_enablePersistence)
            {
                builder.WithPersistence(new JsonWheelPersistence<VoiceData>());
            }

            // 添加输入处理
            _input = new MouseWheelInput(_triggerKey);
            builder.WithInput(_input);

            // 添加选择策略
            _selectionStrategy = new GridSelectionStrategy();
            builder.WithSelectionStrategy(_selectionStrategy);

            _wheel = builder.Build();
        }

        /// <summary>
        /// 初始化UI（手动创建）
        /// </summary>
        private void InitializeUI()
        {
            // 创建Canvas
            var canvasObj = new GameObject("GridWheelCanvas");
            canvasObj.transform.SetParent(transform, false);

            _wheelCanvas = canvasObj.AddComponent<Canvas>();
            _wheelCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _wheelCanvas.sortingOrder = 100;

            var canvasScaler = canvasObj.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);

            canvasObj.AddComponent<GraphicRaycaster>();

            // 创建容器
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

                var slotObj = new GameObject($"GridSlot_{i}");
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
                var display = slotObj.AddComponent<GridWheelDisplay>();
                var wheelItem = _wheel.GetSlot(i) != null
                    ? new WheelItemWrapper
                    {
                        Icon = _wheel.GetSlot(i).Icon,
                        DisplayName = _wheel.GetSlot(i).DisplayName,
                        IsValid = true
                    }
                    : null;

                display.Initialize(wheelItem, i, new Vector2(cellSize, cellSize));
                _gridDisplays.Add(display);
            }

            // 初始隐藏
            _wheelContainer.SetActive(false);
        }

        /// <summary>
        /// 播放语音
        /// </summary>
        private void PlayVoice(VoiceData voice)
        {
            if (voice.AudioClip != null)
            {
                // TODO: 播放音频逻辑
                Debug.Log($"[GridWheelExample] 播放音频: {voice.DisplayName}");
            }

            if (!string.IsNullOrEmpty(voice.SubtitleText))
            {
                Debug.Log($"[GridWheelExample] 字幕: {voice.SubtitleText}");
            }
        }

        void Update()
        {
            // 更新输入
            _input?.OnUpdate();

            // 处理显示/隐藏
            HandleWheelVisibility();

            // 更新选择
            if (_isWheelVisible)
            {
                UpdateSelection();
            }
        }

        /// <summary>
        /// 处理轮盘显示/隐藏
        /// </summary>
        private void HandleWheelVisibility()
        {
            bool shouldShow = Input.GetKey(_triggerKey);

            if (shouldShow && !_isWheelVisible)
            {
                ShowWheel();
            }
            else if (!shouldShow && _isWheelVisible)
            {
                HideWheel();
            }
        }

        /// <summary>
        /// 显示轮盘
        /// </summary>
        private void ShowWheel()
        {
            _isWheelVisible = true;
            _wheelContainer.SetActive(true);

            // 轮盘显示在鼠标位置
            var containerRect = _wheelContainer.GetComponent<RectTransform>();
            containerRect.position = Input.mousePosition;

            _wheel.Show();
        }

        /// <summary>
        /// 隐藏轮盘
        /// </summary>
        private void HideWheel()
        {
            _isWheelVisible = false;
            _wheelContainer.SetActive(false);

            // 如果有选中，触发事件
            if (_currentHoverIndex >= 0 && _currentHoverIndex < 8)
            {
                var selectedData = _wheel.GetSlot(_currentHoverIndex);
                if (selectedData != null)
                {
                    _wheel.EventBus.TriggerItemSelected(_currentHoverIndex, selectedData);
                }
            }

            _wheel.Hide(_currentHoverIndex);
            _currentHoverIndex = -1;
        }

        /// <summary>
        /// 更新选择（根据鼠标方向）
        /// </summary>
        private void UpdateSelection()
        {
            var containerRect = _wheelContainer.GetComponent<RectTransform>();
            Vector2 wheelCenter = containerRect.position;
            Vector2 mousePos = Input.mousePosition;

            // 检查死区
            if (_selectionStrategy.IsInDeadZone(wheelCenter, mousePos, _wheel.Config.DeadZoneRadius))
            {
                if (_currentHoverIndex != -1)
                {
                    // 清除选中
                    UpdateGridDisplaySelection(-1);
                    _currentHoverIndex = -1;
                }
                return;
            }

            // 计算选中索引
            int newIndex = _selectionStrategy.GetSlotIndexFromPosition(
                wheelCenter,
                mousePos,
                9,
                null
            );

            // 跳过中心格子（索引8）
            if (newIndex == 8)
            {
                newIndex = -1;
            }

            // 更新选中
            if (newIndex != _currentHoverIndex)
            {
                UpdateGridDisplaySelection(newIndex);
                _currentHoverIndex = newIndex;
            }
        }

        /// <summary>
        /// 更新格子显示的选中状态
        /// </summary>
        private void UpdateGridDisplaySelection(int selectedIndex)
        {
            foreach (var display in _gridDisplays)
            {
                display.SetSelected(display.GetSlotIndex() == selectedIndex);
            }
        }

        void OnDestroy()
        {
            _wheel?.Dispose();
        }

        #if UNITY_EDITOR
        [ContextMenu("添加测试语音数据")]
        private void AddTestVoiceData()
        {
            _testVoices = new VoiceData[]
            {
                new VoiceData { VoiceID = "hello", DisplayName = "你好", SubtitleText = "你好！" },
                new VoiceData { VoiceID = "thanks", DisplayName = "谢谢", SubtitleText = "谢谢！" },
                new VoiceData { VoiceID = "sorry", DisplayName = "抱歉", SubtitleText = "对不起！" },
                new VoiceData { VoiceID = "help", DisplayName = "求助", SubtitleText = "救命！" },
                new VoiceData { VoiceID = "yes", DisplayName = "同意", SubtitleText = "好的！" },
                new VoiceData { VoiceID = "no", DisplayName = "拒绝", SubtitleText = "不行！" },
                new VoiceData { VoiceID = "attack", DisplayName = "进攻", SubtitleText = "进攻！" },
                new VoiceData { VoiceID = "defend", DisplayName = "防守", SubtitleText = "防守！" },
            };

            Debug.Log("[GridWheelExample] 已添加8个测试语音数据");
        }
        #endif
    }
}
