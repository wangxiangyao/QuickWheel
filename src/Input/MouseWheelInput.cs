using System;
using System.Diagnostics;
using QuickWheel.Core.Interfaces;
using UnityEngine;

namespace QuickWheel.Input
{
	public class MouseWheelInput : IWheelInputHandler
	{ public event Action<Vector2> OnPositionChanged; public event Action OnConfirm; public event Action OnCancel; public event Action OnKeyPressed; public event Action OnShortPressed; public event Action OnLongPressed;

		public MouseWheelInput(KeyCode triggerKey = KeyCode.Alpha1)
		{
			this._triggerKey = triggerKey;
		}

		public MouseWheelInput()
		{
			this._triggerKey = KeyCode.None;
		}

		public void OnUpdate()
		{
			if (this._triggerKey == KeyCode.None)
			{
				if (this.IsPressed)
				{
					Vector3 mousePos = UnityEngine.Input.mousePosition;
					this.OnPositionChanged?.Invoke(mousePos);
				}
				if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
				{
					this._isPressed = false;
					this.OnCancel?.Invoke();
				}
				return;
			}

			if (UnityEngine.Input.GetKeyDown(this._triggerKey))
			{
				this._isPressed = true;
				this._holdTime = 0f;
				this._hasTriggeredLongPress = false;
				this.OnKeyPressed?.Invoke();
			}

			if (this._isPressed)
			{
				this._holdTime += Time.unscaledDeltaTime;
				if (!this._hasTriggeredLongPress && this._holdTime >= 0.25f)
				{
					this._hasTriggeredLongPress = true;
					this.OnLongPressed?.Invoke();
				}
				if (this._hasTriggeredLongPress)
				{
					this.OnPositionChanged?.Invoke(UnityEngine.Input.mousePosition);
				}
				if (UnityEngine.Input.GetKeyUp(this._triggerKey))
				{
					this._isPressed = false;
					if (this._hasTriggeredLongPress)
					{
						this.OnConfirm?.Invoke();
					}
					else
					{
						this.OnShortPressed?.Invoke();
					}
				}
			}

			if (UnityEngine.Input.GetKeyDown(KeyCode.Escape) && this._isPressed)
			{
				this._isPressed = false;
				this.OnCancel?.Invoke();
			}
		}

		public void Reset()
		{
			this._isPressed = false;
			this._holdTime = 0f;
			this._hasTriggeredLongPress = false;
		}

		public bool IsPressed => this._isPressed;

		public bool IsLongPressed => this._hasTriggeredLongPress;

		public void SetPressedState(bool isPressed)
		{
			this._isPressed = isPressed;
		}

		private KeyCode _triggerKey;
		private bool _isPressed;
		private float _holdTime;
		private bool _hasTriggeredLongPress;
	}
}


