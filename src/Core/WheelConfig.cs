using UnityEngine;

namespace QuickWheel.Core
{
    /// <summary>
    /// 轮盘配置类 - 固定9宫格布局
    /// 9个槽位（8个方向 + 1个中心空位）
    /// </summary>
    public class WheelConfig
    {
        // === 核心配置 ===

        /// <summary>
        /// 槽位数量（固定9宫格）
        /// 实际8个可用槽位（中心为空）
        /// </summary>
        public const int SLOT_COUNT = 9;
        public int SlotCount => SLOT_COUNT;

        // === 布局配置 ===

        /// <summary>
        /// 格子大小（像素）
        /// </summary>
        public float GridCellSize = 40f;

        /// <summary>
        /// 格子间距（像素）
        /// </summary>
        public float GridSpacing = 5f;

        // === 交互配置 ===

        /// <summary>
        /// 启用拖拽交换槽位
        /// </summary>
        public bool EnableDragSwap = true;

        /// <summary>
        /// 启用左键点击选中
        /// </summary>
        public bool EnableClickSelect = true;

        /// <summary>
        /// 中心死区半径（像素）
        /// </summary>
        public float DeadZoneRadius = 40f;

        // === 视觉配置 ===

        /// <summary>
        /// Hover时的缩放倍数
        /// </summary>
        public float HoverScaleMultiplier = 1.15f;

        /// <summary>
        /// 动画时长（秒）
        /// </summary>
        public float AnimationDuration = 0.2f;

        // === 持久化配置 ===

        /// <summary>
        /// 启用持久化
        /// </summary>
        public bool EnablePersistence = false;

        /// <summary>
        /// 持久化键名（EnablePersistence=true时必须提供）
        /// </summary>
        public string PersistenceKey = "";

        /// <summary>
        /// 验证配置有效性
        /// </summary>
        /// <param name="error">错误信息</param>
        /// <returns>true=有效，false=无效</returns>
        public bool Validate(out string error)
        {
            // 持久化键名检查
            if (EnablePersistence && string.IsNullOrEmpty(PersistenceKey))
            {
                error = "PersistenceKey is required when EnablePersistence is true";
                return false;
            }

            // 格子大小检查
            if (GridCellSize <= 0)
            {
                error = "GridCellSize must be greater than 0";
                return false;
            }

            // 格子间距检查
            if (GridSpacing < 0)
            {
                error = "GridSpacing must be non-negative";
                return false;
            }

            // 死区检查
            if (DeadZoneRadius < 0)
            {
                error = "DeadZoneRadius must be non-negative";
                return false;
            }

            error = null;
            return true;
        }

        /// <summary>
        /// 创建默认配置（9宫格布局）
        /// </summary>
        public static WheelConfig CreateDefault()
        {
            return new WheelConfig
            {
                GridCellSize = 40f,
                GridSpacing = 5f,
                EnableDragSwap = true,
                EnableClickSelect = true,
                DeadZoneRadius = 20f,
                HoverScaleMultiplier = 1.15f,
                AnimationDuration = 0.1f,
                EnablePersistence = false
            };
        }
    }
}
