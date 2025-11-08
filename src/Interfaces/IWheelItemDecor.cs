using UnityEngine;

namespace QuickWheel.Core.Interfaces
{
    /// <summary>
    /// 可选的轮盘项装饰接口：用于提供稀有度底色、右侧文字与耐久度等附加显示。
    /// 现有 IWheelItem 可不实现本接口，保持兼容。
    /// </summary>
    public interface IWheelItemDecor
    {
        /// <summary>
        /// 稀有度底色（含透明度），叠加在背景之上；返回 null 表示不显示。
        /// </summary>
        Color? GetRarityTint();

        /// <summary>
        /// 靠右显示的附加文字（例如数量/库存）。返回 null/空表示不显示。
        /// </summary>
        string GetRightText();

        /// <summary>
        /// 0-1 的耐久度；null 表示不显示耐久条。
        /// </summary>
        float? GetDurability01();

        /// <summary>
        /// 是否右对齐名称/信息，默认 true。
        /// </summary>
        bool RightAlign();
    }
}

