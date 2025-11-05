using UnityEngine;

namespace QuickWheel.Core.Interfaces
{
    /// <summary>
    /// 轮盘项的UI显示接口
    /// 所有显示在轮盘上的内容都必须实现此接口
    /// </summary>
    public interface IWheelItem
    {
        /// <summary>
        /// 获取显示图标
        /// </summary>
        /// <returns>Sprite对象，null表示无图标</returns>
        Sprite GetIcon();

        /// <summary>
        /// 获取显示名称
        /// </summary>
        /// <returns>显示文本，null或空字符串表示无文本</returns>
        string GetDisplayName();

        /// <summary>
        /// 是否为有效项（用于处理null/空槽）
        /// </summary>
        /// <returns>true=有效显示，false=隐藏该槽位</returns>
        bool IsValid();
    }
}
