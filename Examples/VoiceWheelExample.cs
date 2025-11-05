using QuickWheel.Core;
using QuickWheel.Input;
using QuickWheel.Persistence;
using QuickWheel.Utils;
using UnityEngine;

namespace QuickWheel.Examples.VoiceWheel
{
    /// <summary>
    /// 语音轮盘示例
    /// 展示如何创建和使用轮盘系统
    /// </summary>
    public class VoiceWheelExample : MonoBehaviour
    {
        [Header("配置")]
        [SerializeField] private KeyCode _triggerKey = KeyCode.V;
        [SerializeField] private int _slotCount = 6;
        [SerializeField] private bool _enablePersistence = true;

        [Header("语音数据")]
        [SerializeField] private VoiceData[] _availableVoices;

        // 轮盘实例
        private Wheel<VoiceData> _wheel;

        // 音频播放器
        private AudioSource _audioSource;

        void Start()
        {
            // 获取或创建AudioSource
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }

            // 创建语音轮盘
            CreateVoiceWheel();

            // 初始化槽位
            InitializeSlots();
        }

        /// <summary>
        /// 创建语音轮盘
        /// </summary>
        private void CreateVoiceWheel()
        {
            var builder = new WheelBuilder<VoiceData>()
                .WithConfig(config =>
                {
                    config.SlotCount = _slotCount;
                    config.SlotRadius = 100f;
                    config.EnableDragSwap = true;
                    config.EnableClickSelect = true;
                    config.EnablePersistence = _enablePersistence;
                    config.PersistenceKey = "VoiceWheel";
                })
                .WithAdapter(new VoiceWheelAdapter())
                .WithInput(new MouseWheelInput(_triggerKey))
                .OnItemSelected(PlayVoice)
                .OnWheelShown(() => Debug.Log("语音轮盘已显示"))
                .OnWheelHidden((finalIndex) =>
                {
                    if (finalIndex >= 0)
                        Debug.Log($"语音轮盘隐藏，选中索引: {finalIndex}");
                    else
                        Debug.Log("语音轮盘隐藏，未选择");
                });

            // 如果启用持久化，添加持久化实现
            if (_enablePersistence)
            {
                builder.WithPersistence(new JsonWheelPersistence<VoiceData>());
            }

            _wheel = builder.Build();

            Debug.Log($"[VoiceWheelExample] 语音轮盘已创建，按 {_triggerKey} 键显示");
        }

        /// <summary>
        /// 初始化槽位
        /// </summary>
        private void InitializeSlots()
        {
            if (_availableVoices == null || _availableVoices.Length == 0)
            {
                Debug.LogWarning("[VoiceWheelExample] 没有可用的语音数据");
                return;
            }

            // 将语音添加到轮盘
            int count = Mathf.Min(_availableVoices.Length, _slotCount);
            for (int i = 0; i < count; i++)
            {
                _wheel.SetSlot(i, _availableVoices[i]);
            }

            Debug.Log($"[VoiceWheelExample] 已添加 {count} 个语音到轮盘");
        }

        /// <summary>
        /// 播放语音
        /// </summary>
        private void PlayVoice(int index, VoiceData voice)
        {
            if (voice == null || voice.AudioClip == null)
            {
                Debug.LogWarning($"[VoiceWheelExample] 槽位 {index} 没有有效的语音数据");
                return;
            }

            Debug.Log($"[VoiceWheelExample] 播放语音: {voice.DisplayName}");

            // 播放音频
            _audioSource.clip = voice.AudioClip;
            _audioSource.Play();

            // 显示字幕（如果有）
            if (!string.IsNullOrEmpty(voice.SubtitleText))
            {
                ShowSubtitle(voice.SubtitleText);
            }
        }

        /// <summary>
        /// 显示字幕
        /// </summary>
        private void ShowSubtitle(string text)
        {
            // TODO: 实现字幕显示逻辑
            Debug.Log($"[字幕] {text}");
        }

        void Update()
        {
            // 更新轮盘输入
            _wheel?.Update();
        }

        void OnDestroy()
        {
            // 清理轮盘资源
            _wheel?.Dispose();
        }

        // === Inspector辅助方法 ===

        #if UNITY_EDITOR
        [ContextMenu("添加测试语音数据")]
        private void AddTestVoiceData()
        {
            _availableVoices = new VoiceData[]
            {
                new VoiceData { VoiceID = "hello", DisplayName = "你好", SubtitleText = "你好！" },
                new VoiceData { VoiceID = "thanks", DisplayName = "谢谢", SubtitleText = "谢谢！" },
                new VoiceData { VoiceID = "sorry", DisplayName = "抱歉", SubtitleText = "对不起！" },
                new VoiceData { VoiceID = "help", DisplayName = "求助", SubtitleText = "救命！" },
                new VoiceData { VoiceID = "yes", DisplayName = "同意", SubtitleText = "好的！" },
                new VoiceData { VoiceID = "no", DisplayName = "拒绝", SubtitleText = "不行！" },
            };

            Debug.Log("[VoiceWheelExample] 已添加测试语音数据");
        }
        #endif
    }
}
