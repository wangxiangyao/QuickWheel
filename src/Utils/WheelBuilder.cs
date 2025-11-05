using System;
using QuickWheel.Core;
using QuickWheel.Core.Interfaces;

namespace QuickWheel.Utils
{
    /// <summary>
    /// 轮盘构建器
    /// 提供流畅的链式API创建轮盘
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public class WheelBuilder<T>
    {
        private WheelConfig _config;
        private IWheelItemAdapter<T> _adapter;
        private IWheelDataProvider<T> _dataProvider;
        private IWheelPersistence<T> _persistence;
        private IWheelInputHandler _inputHandler;
        private IWheelSelectionStrategy _selectionStrategy;

        private Action<int, T> _onItemSelected;
        private Action _onWheelShown;
        private Action<int> _onWheelHidden;

        public WheelBuilder()
        {
            _config = WheelConfig.CreateDefault();
        }

        /// <summary>
        /// 配置轮盘
        /// </summary>
        /// <param name="configAction">配置动作</param>
        /// <returns>构建器</returns>
        public WheelBuilder<T> WithConfig(Action<WheelConfig> configAction)
        {
            configAction?.Invoke(_config);
            return this;
        }

        /// <summary>
        /// 设置适配器（必需）
        /// </summary>
        /// <param name="adapter">适配器</param>
        /// <returns>构建器</returns>
        public WheelBuilder<T> WithAdapter(IWheelItemAdapter<T> adapter)
        {
            _adapter = adapter;
            return this;
        }

        /// <summary>
        /// 设置数据提供者（可选）
        /// </summary>
        /// <param name="dataProvider">数据提供者</param>
        /// <returns>构建器</returns>
        public WheelBuilder<T> WithDataProvider(IWheelDataProvider<T> dataProvider)
        {
            _dataProvider = dataProvider;
            return this;
        }

        /// <summary>
        /// 设置持久化（可选）
        /// </summary>
        /// <param name="persistence">持久化实现</param>
        /// <returns>构建器</returns>
        public WheelBuilder<T> WithPersistence(IWheelPersistence<T> persistence)
        {
            _persistence = persistence;
            return this;
        }

        /// <summary>
        /// 设置输入处理器（可选）
        /// </summary>
        /// <param name="inputHandler">输入处理器</param>
        /// <returns>构建器</returns>
        public WheelBuilder<T> WithInput(IWheelInputHandler inputHandler)
        {
            _inputHandler = inputHandler;
            return this;
        }

        /// <summary>
        /// 设置选择策略（可选）
        /// </summary>
        /// <param name="selectionStrategy">选择策略</param>
        /// <returns>构建器</returns>
        public WheelBuilder<T> WithSelectionStrategy(IWheelSelectionStrategy selectionStrategy)
        {
            _selectionStrategy = selectionStrategy;
            return this;
        }

        /// <summary>
        /// 订阅物品选中事件
        /// </summary>
        /// <param name="callback">回调</param>
        /// <returns>构建器</returns>
        public WheelBuilder<T> OnItemSelected(Action<int, T> callback)
        {
            _onItemSelected = callback;
            return this;
        }

        /// <summary>
        /// 订阅轮盘显示事件
        /// </summary>
        /// <param name="callback">回调</param>
        /// <returns>构建器</returns>
        public WheelBuilder<T> OnWheelShown(Action callback)
        {
            _onWheelShown = callback;
            return this;
        }

        /// <summary>
        /// 订阅轮盘隐藏事件
        /// </summary>
        /// <param name="callback">回调</param>
        /// <returns>构建器</returns>
        public WheelBuilder<T> OnWheelHidden(Action<int> callback)
        {
            _onWheelHidden = callback;
            return this;
        }

        /// <summary>
        /// 构建轮盘
        /// </summary>
        /// <returns>轮盘实例</returns>
        public Wheel<T> Build()
        {
            if (_adapter == null)
            {
                throw new InvalidOperationException("Adapter is required. Call WithAdapter() before Build().");
            }

            // 创建轮盘
            var wheel = new Wheel<T>(_config, _adapter);

            // 设置可选组件
            if (_dataProvider != null)
            {
                wheel.SetDataProvider(_dataProvider);
            }

            if (_persistence != null)
            {
                wheel.SetPersistence(_persistence);
            }

            if (_inputHandler != null)
            {
                wheel.SetInputHandler(_inputHandler);
            }

            if (_selectionStrategy != null)
            {
                wheel.SetSelectionStrategy(_selectionStrategy);
            }

            // 订阅事件
            if (_onItemSelected != null)
            {
                wheel.OnItemSelected += _onItemSelected;
            }

            if (_onWheelShown != null)
            {
                wheel.OnWheelShown += _onWheelShown;
            }

            if (_onWheelHidden != null)
            {
                wheel.OnWheelHidden += _onWheelHidden;
            }

            return wheel;
        }

        /// <summary>
        /// 创建简单轮盘（使用默认配置）
        /// </summary>
        /// <returns>构建器</returns>
        public static WheelBuilder<T> CreateSimple()
        {
            return new WheelBuilder<T>();
        }
    }
}
