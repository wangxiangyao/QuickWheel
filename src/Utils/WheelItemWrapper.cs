using QuickWheel.Core.Interfaces;
using UnityEngine;

namespace QuickWheel.Utils
{
    /// <summary>
    /// IWheelItem的默认包装实现
    /// 方便快速创建轮盘项
    /// </summary>
    public class WheelItemWrapper : IWheelItem
    {
        public Sprite Icon { get; set; }
        public string DisplayName { get; set; }
        public bool IsValid { get; set; }

        public Sprite GetIcon() => Icon;
        public string GetDisplayName() => DisplayName;
        bool IWheelItem.IsValid() => IsValid;
    }
}
