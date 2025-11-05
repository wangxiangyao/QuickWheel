using UnityEngine;

namespace QuickWheel.Core
{
    /// <summary>
    /// 轮盘配置类
    /// 支持灵活定制轮盘的各种参数
    /// </summary>
    public class WheelConfig
    {
        // === 核心配置 ===

        /// <summary>
        /// 槽位数量（强制约束3-8）
        /// </summary>
        private int _slotCount = 8;
        public int SlotCount
        {
            get => _slotCount;
            set => _slotCount = Mathf.Clamp(value, 3, 8);
        }

        // === 布局配置 ===

        /// <summary>
        /// 轮盘半径（像素）
        /// </summary>
        public float SlotRadius = 120f;

        /// <summary>
        /// 自定义角度分布（null=均匀分布）
        /// 例如：new float[] { 0, 45, 90, 135, 180, 225, 270, 315 }
        /// </summary>
        public float[] CustomAngles = null;

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
            // 槽位数量检查
            if (SlotCount < 3 || SlotCount > 8)
            {
                error = "SlotCount must be between 3 and 8";
                return false;
            }

            // 自定义角度检查
            if (CustomAngles != null && CustomAngles.Length != SlotCount)
            {
                error = $"CustomAngles length ({CustomAngles.Length}) must match SlotCount ({SlotCount})";
                return false;
            }

            // 持久化键名检查
            if (EnablePersistence && string.IsNullOrEmpty(PersistenceKey))
            {
                error = "PersistenceKey is required when EnablePersistence is true";
                return false;
            }

            // 半径检查
            if (SlotRadius <= 0)
            {
                error = "SlotRadius must be greater than 0";
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
        /// 创建默认配置
        /// </summary>
        public static WheelConfig CreateDefault()
        {
            return new WheelConfig
            {
                SlotCount = 8,
                SlotRadius = 120f,
                EnableDragSwap = true,
                EnableClickSelect = true,
                DeadZoneRadius = 40f,
                HoverScaleMultiplier = 1.15f,
                AnimationDuration = 0.2f,
                EnablePersistence = false
            };
        }
    }
}
