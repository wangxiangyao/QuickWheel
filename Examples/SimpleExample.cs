using UnityEngine;
using QuickWheel.Core;
using QuickWheel.Utils;
using QuickWheel.Selection;

namespace QuickWheel.Examples
{
    /// <summary>
    /// 最简单的9宫格轮盘示例
    /// 展示如何使用内置UI（不需要手动创建UI）
    /// </summary>
    public class SimpleExample : MonoBehaviour
    {
        [Header("配置")]
        [SerializeField] private KeyCode _triggerKey = KeyCode.V;

        [Header("测试数据")]
        [SerializeField] private VoiceData[] _testVoices;

        // 轮盘实例（内置UI）
        private Wheel<VoiceData> _wheel;

        // 选择策略
        private GridSelectionStrategy _selectionStrategy;

        void Start()
        {
            // 创建轮盘（自动启用内置9宫格UI）
            _wheel = new WheelBuilder<VoiceData>()
                .WithAdapter(new VoiceWheelAdapter())
                .WithSelectionStrategy(new GridSelectionStrategy())
                .OnItemSelected((index, voice) =>
                {
                    if (voice != null)
                    {
                        Debug.Log($"[SimpleExample] 选中: {voice.DisplayName}");
                    }
                })
                .Build();

            _selectionStrategy = new GridSelectionStrategy();

            // 添加测试数据
            if (_testVoices != null && _testVoices.Length > 0)
            {
                for (int i = 0; i < Mathf.Min(_testVoices.Length, 8); i++)
                {
                    _wheel.SetSlot(i, _testVoices[i]);
                }
            }

            Debug.Log($"[SimpleExample] 按 {_triggerKey} 显示轮盘");
        }

        void Update()
        {
            // 1. 处理显示/隐藏
            bool keyPressed = Input.GetKey(_triggerKey);

            if (keyPressed && !_wheel.IsVisible)
            {
                _wheel.Show();
            }
            else if (!keyPressed && _wheel.IsVisible)
            {
                _wheel.Hide(executeSelection: true);
            }

            // 2. 更新选择（在轮盘显示时）
            if (_wheel.IsVisible)
            {
                UpdateSelection();
            }
        }

        /// <summary>
        /// 更新选择（根据鼠标方向）
        /// </summary>
        private void UpdateSelection()
        {
            var containerRect = _wheel.GetUIContainerRect();
            if (containerRect == null) return;

            Vector2 wheelCenter = containerRect.position;
            Vector2 mousePos = Input.mousePosition;

            // 检查死区
            if (_selectionStrategy.IsInDeadZone(wheelCenter, mousePos, _wheel.Config.DeadZoneRadius))
            {
                _wheel.UpdateUISelection(-1);
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

            // 更新UI选中状态
            _wheel.UpdateUISelection(newIndex);
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

            Debug.Log("[SimpleExample] 已添加8个测试语音数据");
        }
        #endif
    }
}
