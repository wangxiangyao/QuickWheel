namespace QuickWheel.Core.States
{
    /// <summary>
    /// 轮盘状态枚举
    /// </summary>
    public enum WheelState
    {
        /// <summary>
        /// 隐藏状态
        /// </summary>
        Hidden,

        /// <summary>
        /// 显示动画中
        /// </summary>
        Showing,

        /// <summary>
        /// 活跃状态（可交互）
        /// </summary>
        Active,

        /// <summary>
        /// 隐藏动画中
        /// </summary>
        Hiding
    }
}
