namespace QuickWheel.Core.Interfaces
{
    /// <summary>
    /// 轮盘项适配器接口
    /// 负责将业务数据转换为UI可显示的IWheelItem
    /// </summary>
    /// <typeparam name="T">业务数据类型</typeparam>
    public interface IWheelItemAdapter<T>
    {
        /// <summary>
        /// 将业务数据转换为UI可显示对象
        /// </summary>
        /// <param name="data">业务数据</param>
        /// <returns>UI显示对象，null表示空槽位</returns>
        IWheelItem ToWheelItem(T data);

        /// <summary>
        /// 从UI对象还原为业务数据（可选实现）
        /// </summary>
        /// <param name="item">UI对象</param>
        /// <returns>业务数据，通常返回null</returns>
        T FromWheelItem(IWheelItem item);
    }
}
