using System;
using QuickWheel.Core.Interfaces;
using UnityEngine;

namespace QuickWheel.Input
{
    /// <summary>
    /// 鼠标轮盘输入处理器
    /// 按下键显示轮盘，鼠标移动选择，松开键确认
    /// </summary>
    public class MouseWheelInput : IWheelInputHandler
    {
        public event Action<Vector2> OnPositionChanged;
        public event Action OnConfirm;
        public event Action OnCancel;

        private KeyCode _triggerKey;
        private bool _isPressed;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="triggerKey">触发键</param>
        public MouseWheelInput(KeyCode triggerKey = KeyCode.Alpha1)
        {
            _triggerKey = triggerKey;
        }

        /// <summary>
        /// 每帧更新
        /// </summary>
        public void OnUpdate()
        {
            // 检测按键按下
            if (UnityEngine.Input.GetKeyDown(_triggerKey))
            {
                _isPressed = true;
            }

            // 按键按住时
            if (_isPressed)
            {
                // 持续发送鼠标位置
                OnPositionChanged?.Invoke(UnityEngine.Input.mousePosition);

                // 检测松开
                if (UnityEngine.Input.GetKeyUp(_triggerKey))
                {
                    _isPressed = false;
                    OnConfirm?.Invoke();
                }
            }

            // Esc取消
            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape) && _isPressed)
            {
                _isPressed = false;
                OnCancel?.Invoke();
            }
        }

        /// <summary>
        /// 重置状态
        /// </summary>
        public void Reset()
        {
            _isPressed = false;
        }
    }
}
