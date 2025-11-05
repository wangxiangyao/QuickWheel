using System;
using System.Collections.Generic;

namespace QuickWheel.Core.Interfaces
{
    /// <summary>
    /// 数据提供者接口
    /// 负责提供轮盘的数据源，监听数据变化
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public interface IWheelDataProvider<T>
    {
        /// <summary>
        /// 获取所有可用数据
        /// </summary>
        /// <returns>数据集合</returns>
        IEnumerable<T> GetAvailableItems();

        /// <summary>
        /// 数据添加事件
        /// </summary>
        event Action<T> OnItemAdded;

        /// <summary>
        /// 数据移除事件
        /// </summary>
        event Action<T> OnItemRemoved;

        /// <summary>
        /// 数据变更事件（旧数据, 新数据）
        /// </summary>
        event Action<T, T> OnItemChanged;

        /// <summary>
        /// 验证数据有效性
        /// </summary>
        /// <param name="item">要验证的数据</param>
        /// <returns>true=有效，false=无效</returns>
        bool IsValid(T item);
    }
}
