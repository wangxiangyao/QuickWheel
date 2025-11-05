using UnityEngine;

namespace QuickWheel.Examples.VoiceWheel
{
    /// <summary>
    /// 语音数据类（示例）
    /// </summary>
    [System.Serializable]
    public class VoiceData
    {
        [Tooltip("语音ID")]
        public string VoiceID;

        [Tooltip("显示名称")]
        public string DisplayName;

        [Tooltip("图标")]
        public Sprite Icon;

        [Tooltip("音频片段")]
        public AudioClip AudioClip;

        [Tooltip("字幕文本")]
        public string SubtitleText;
    }
}
