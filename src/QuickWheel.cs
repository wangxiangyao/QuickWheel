using UnityEngine;

namespace QuickWheel
{
    /// <summary>
    /// QuickWheel统一入口
    /// 提供最简单的轮盘创建方式
    /// </summary>
    public static class QuickWheel
    {
        /// <summary>
        /// 创建轮盘构建器
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <returns>构建器实例</returns>
        public static Utils.WheelBuilder<T> Create<T>()
        {
            return Utils.WheelBuilder<T>.CreateSimple();
        }
    }
}