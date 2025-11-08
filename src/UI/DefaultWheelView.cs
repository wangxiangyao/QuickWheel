using System;
using QuickWheel.Core;
using QuickWheel.Core.Interfaces;
using UnityEngine;

namespace QuickWheel.UI
{
	public class DefaultWheelView<T> : IWheelView<T>, IDisposable
	{
		public DefaultWheelView(Transform parent = null)
		{
			this._parent = parent;
		}

		public void SetWheelCenterBeforeShow(Vector2 center)
		{
			this._wheelCenter = center;
		}

		public void Attach(Wheel<T> wheel, IWheelItemAdapter<T> adapter)
		{
			if (wheel == null)
			{
				throw new ArgumentNullException("wheel");
			}
			this._wheel = wheel;
			if (adapter == null)
			{
				throw new ArgumentNullException("adapter");
			}
			this._adapter = adapter;
			this._uiManager = new WheelUIManager<T>(this._wheel, this._adapter, this._parent);
			wheel.EventBus.OnWheelShown += this.OnWheelShown;
		}

		public void OnWheelShown()
		{
			bool flag = this._wheelCenter == Vector2.zero;
			Vector2 centerToUse;
			if (flag)
			{
				centerToUse = UnityEngine.Input.mousePosition;
				this._wheelCenter = centerToUse;
			}
			else
			{
				centerToUse = this._wheelCenter;
			}
			WheelUIManager<T> uiManager = this._uiManager;
			if (uiManager != null)
			{
				uiManager.Show(new Vector2?(centerToUse));
			}
		}

		public void Detach()
		{
			bool flag = this._uiManager != null;
			if (flag)
			{
				this._uiManager.Dispose();
				this._uiManager = null;
			}
			bool flag2 = this._wheel != null;
			if (flag2)
			{
				this._wheel.EventBus.OnWheelShown -= this.OnWheelShown;
			}
			this._wheel = null;
			this._adapter = null;
		}

		public void OnWheelHidden(int finalIndex)
		{
			WheelUIManager<T> uiManager = this._uiManager;
			if (uiManager != null)
			{
				uiManager.Hide();
			}
			this._wheelCenter = Vector2.zero;
		}

		public void OnSlotDataChanged(int index, T data)
		{
		}

		public void OnSlotsSwapped(int index1, int index2)
		{
		}

		public void OnSelectionChanged(int selectedIndex)
		{
			WheelUIManager<T> uiManager = this._uiManager;
			if (uiManager != null)
			{
				uiManager.UpdateSelection(selectedIndex);
			}
		}

		public void OnHoverChanged(int hoveredIndex)
		{
			WheelUIManager<T> uiManager = this._uiManager;
			if (uiManager != null)
			{
				uiManager.UpdateHover(hoveredIndex);
			}
		}

		public Vector2 GetWheelCenter()
		{
			bool flag = this._wheelCenter != Vector2.zero;
			Vector2 result;
			if (flag)
			{
				result = this._wheelCenter;
			}
			else
			{
				result = UnityEngine.Input.mousePosition;
			}
			return result;
		}

		public void Dispose()
		{
			this.Detach();
		}

		private readonly Transform _parent;
		private WheelUIManager<T> _uiManager;
		private Wheel<T> _wheel;
		private IWheelItemAdapter<T> _adapter;
		private Vector2 _wheelCenter;
	}
}

