using System;
using QuickWheel.Core;
using QuickWheel.Core.Interfaces;
using UnityEngine;

namespace QuickWheel.UI
{
    /// <summary>
    /// Default 9-slot wheel view implementation built on WheelUIManager.
    /// </summary>
    public class DefaultWheelView<T> : IWheelView<T>
    {
        private readonly Transform _parent;
        private WheelUIManager<T> _uiManager;
        private Wheel<T> _wheel;
        private IWheelItemAdapter<T> _adapter;

        public DefaultWheelView(Transform parent = null)
        {
            _parent = parent;
        }

        public void Attach(Wheel<T> wheel, IWheelItemAdapter<T> adapter)
        {
            _wheel = wheel ?? throw new ArgumentNullException(nameof(wheel));
            _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));

            _uiManager = new WheelUIManager<T>(_wheel, _adapter, _parent);
        }

        public void Detach()
        {
            if (_uiManager != null)
            {
                _uiManager.Dispose();
                _uiManager = null;
            }

            _wheel = null;
            _adapter = null;
        }

        public void OnWheelShown()
        {
            _uiManager?.Show();
        }

        public void OnWheelHidden(int finalIndex)
        {
            _uiManager?.Hide();
        }

        public void OnSlotDataChanged(int index, T data)
        {
            // WheelUIManager already listens to slot data events.
        }

        public void OnSlotsSwapped(int index1, int index2)
        {
            // WheelUIManager refreshes swapped slots internally.
        }

        public void OnSelectionChanged(int selectedIndex)
        {
            _uiManager?.UpdateSelection(selectedIndex);
        }

        public void OnHoverChanged(int hoveredIndex)
        {
            _uiManager?.UpdateSelection(hoveredIndex);
        }

        public void Dispose()
        {
            Detach();
        }
    }
}
