using QuickWheel.Core.Interfaces;
using QuickWheel.Utils;

namespace QuickWheel.Examples.VoiceWheel
{
    /// <summary>
    /// 语音轮盘适配器
    /// 将VoiceData转换为IWheelItem
    /// </summary>
    public class VoiceWheelAdapter : IWheelItemAdapter<VoiceData>
    {
        /// <summary>
        /// 将VoiceData转换为IWheelItem
        /// </summary>
        public IWheelItem ToWheelItem(VoiceData voice)
        {
            // null处理
            if (voice == null) return null;

            // 返回包装对象
            return new WheelItemWrapper
            {
                Icon = voice.Icon,
                DisplayName = voice.DisplayName,
                IsValid = !string.IsNullOrEmpty(voice.VoiceID)
            };
        }

        /// <summary>
        /// 从IWheelItem还原为VoiceData（通常不需要）
        /// </summary>
        public VoiceData FromWheelItem(IWheelItem item)
        {
            return null;
        }
    }
}
