using System;
using UnityEngine;

namespace QuickWheel.Core.Interfaces
{
    /// <summary>
    /// 轮盘输入处理接口（可选功能）
    /// </summary>
    public interface IWheelInputHandler
    {
        /// <summary>
        /// 每帧更新（由轮盘调用）
        /// </summary>
        void OnUpdate();

        /// <summary>
        /// 位置变化事件（鼠标/摇杆位置）
        /// </summary>
        event Action<Vector2> OnPositionChanged;

        /// <summary>
        /// 确认选择事件
        /// </summary>
        event Action OnConfirm;

        /// <summary>
        /// 取消事件
        /// </summary>
        event Action OnCancel;
    }
}
