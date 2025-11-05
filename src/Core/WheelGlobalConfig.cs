namespace QuickWheel.Core
{
    /// <summary>
    /// 全局配置（影响所有轮盘）
    /// </summary>
    public static class WheelGlobalConfig
    {
        /// <summary>
        /// 全局死区半径（像素）
        /// </summary>
        public static float GlobalDeadZoneRadius = 40f;

        /// <summary>
        /// 全局Hover缩放倍数
        /// </summary>
        public static float GlobalHoverScale = 1.15f;

        /// <summary>
        /// 全局动画时长（秒）
        /// </summary>
        public static float GlobalAnimationDuration = 0.2f;
    }
}
