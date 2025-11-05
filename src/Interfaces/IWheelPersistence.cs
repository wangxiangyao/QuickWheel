using System;

namespace QuickWheel.Core.Interfaces
{
    /// <summary>
    /// 轮盘持久化接口（可选功能）
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public interface IWheelPersistence<T>
    {
        /// <summary>
        /// 保存轮盘状态
        /// </summary>
        /// <param name="key">唯一键名</param>
        /// <param name="data">布局数据</param>
        void Save(string key, WheelLayoutData<T> data);

        /// <summary>
        /// 加载轮盘状态
        /// </summary>
        /// <param name="key">唯一键名</param>
        /// <returns>布局数据，null表示不存在</returns>
        WheelLayoutData<T> Load(string key);

        /// <summary>
        /// 检查是否存在保存数据
        /// </summary>
        /// <param name="key">唯一键名</param>
        /// <returns>true=存在，false=不存在</returns>
        bool Has(string key);

        /// <summary>
        /// 删除保存数据
        /// </summary>
        /// <param name="key">唯一键名</param>
        void Delete(string key);
    }

    /// <summary>
    /// 持久化数据结构
    /// 注意：只保存布局结构，不保存数据内容
    /// </summary>
    [Serializable]
    public class WheelLayoutData<T>
    {
        /// <summary>
        /// 槽位数量
        /// </summary>
        public int SlotCount;

        /// <summary>
        /// 选中索引
        /// </summary>
        public int SelectedIndex;

        /// <summary>
        /// 槽位顺序（用于记录拖拽后的排列）
        /// </summary>
        public int[] SlotOrder;
    }
}
