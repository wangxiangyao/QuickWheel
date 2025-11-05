using System;
using QuickWheel.Core.Interfaces;
using QuickWheel.Utils;
using UnityEngine;

namespace QuickWheel.UI
{
    public static class WheelBuilderDefaultViewExtensions
    {
        public static WheelBuilder<T> UseDefaultView<T>(this WheelBuilder<T> builder, Transform parent = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.WithView(new DefaultWheelView<T>(parent));
        }
    }
}
