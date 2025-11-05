using System;
using QuickWheel.Core;
using QuickWheel.Core.Interfaces;

namespace QuickWheel.Utils
{
    /// <summary>
    /// 杞洏鏋勫缓鍣?
    /// 鎻愪緵娴佺晠鐨勯摼寮廇PI鍒涘缓杞洏
    /// </summary>
    /// <typeparam name="T">鏁版嵁绫诲瀷</typeparam>
    public class WheelBuilder<T>
    {
        private WheelConfig _config;
        private IWheelItemAdapter<T> _adapter;
        private IWheelDataProvider<T> _dataProvider;
        private IWheelPersistence<T> _persistence;
        private IWheelInputHandler _inputHandler;
        private IWheelSelectionStrategy _selectionStrategy;
        private IWheelView<T> _view;

        private Action<int, T> _onItemSelected;
        private Action _onWheelShown;
        private Action<int> _onWheelHidden;

        public WheelBuilder()
        {
            _config = WheelConfig.CreateDefault();
        }

        /// <summary>
        /// 閰嶇疆杞洏
        /// </summary>
        /// <param name="configAction">閰嶇疆鍔ㄤ綔</param>
        /// <returns>鏋勫缓鍣?/returns>
        public WheelBuilder<T> WithConfig(Action<WheelConfig> configAction)
        {
            configAction?.Invoke(_config);
            return this;
        }

        /// <summary>
        /// 璁剧疆閫傞厤鍣紙蹇呴渶锛?
        /// </summary>
        /// <param name="adapter">閫傞厤鍣?/param>
        /// <returns>鏋勫缓鍣?/returns>
        public WheelBuilder<T> WithAdapter(IWheelItemAdapter<T> adapter)
        {
            _adapter = adapter;
            return this;
        }

        /// <summary>
        /// 璁剧疆鏁版嵁鎻愪緵鑰咃紙鍙€夛級
        /// </summary>
        /// <param name="dataProvider">鏁版嵁鎻愪緵鑰?/param>
        /// <returns>鏋勫缓鍣?/returns>
        public WheelBuilder<T> WithDataProvider(IWheelDataProvider<T> dataProvider)
        {
            _dataProvider = dataProvider;
            return this;
        }

        /// <summary>
        /// 璁剧疆鎸佷箙鍖栵紙鍙€夛級
        /// </summary>
        /// <param name="persistence">鎸佷箙鍖栧疄鐜?/param>
        /// <returns>鏋勫缓鍣?/returns>
        public WheelBuilder<T> WithPersistence(IWheelPersistence<T> persistence)
        {
            _persistence = persistence;
            return this;
        }

        /// <summary>
        /// 璁剧疆杈撳叆澶勭悊鍣紙鍙€夛級
        /// </summary>
        /// <param name="inputHandler">杈撳叆澶勭悊鍣?/param>
        /// <returns>鏋勫缓鍣?/returns>
        public WheelBuilder<T> WithInput(IWheelInputHandler inputHandler)
        {
            _inputHandler = inputHandler;
            return this;
        }

        /// <summary>
        /// 璁剧疆閫夋嫨绛栫暐锛堝彲閫夛級
        /// </summary>
        /// <param name="selectionStrategy">閫夋嫨绛栫暐</param>
        /// <returns>鏋勫缓鍣?/returns>
        public WheelBuilder<T> WithSelectionStrategy(IWheelSelectionStrategy selectionStrategy)
        {
            _selectionStrategy = selectionStrategy;
            return this;
        }

        /// <summary>
        /// 指定轮盘视图（可选）
        /// </summary>
        public WheelBuilder<T> WithView(IWheelView<T> view)
        {
            _view = view;
            return this;
        }

        /// <summary>
        /// 鍚敤/绂佺敤鍐呯疆9瀹牸UI
        /// </summary>
        /// <returns>鏋勫缓鍣?/returns>

        /// <summary>
        /// 璁㈤槄鐗╁搧閫変腑浜嬩欢
        /// </summary>
        /// <param name="callback">鍥炶皟</param>
        /// <returns>鏋勫缓鍣?/returns>
        public WheelBuilder<T> OnItemSelected(Action<int, T> callback)
        {
            _onItemSelected = callback;
            return this;
        }

        /// <summary>
        /// 璁㈤槄杞洏鏄剧ず浜嬩欢
        /// </summary>
        /// <param name="callback">鍥炶皟</param>
        /// <returns>鏋勫缓鍣?/returns>
        public WheelBuilder<T> OnWheelShown(Action callback)
        {
            _onWheelShown = callback;
            return this;
        }

        /// <summary>
        /// 璁㈤槄杞洏闅愯棌浜嬩欢
        /// </summary>
        /// <param name="callback">鍥炶皟</param>
        /// <returns>鏋勫缓鍣?/returns>
        public WheelBuilder<T> OnWheelHidden(Action<int> callback)
        {
            _onWheelHidden = callback;
            return this;
        }

        /// <summary>
        /// 鏋勫缓杞洏
        /// </summary>
        /// <returns>杞洏瀹炰緥</returns>
        public Wheel<T> Build()
        {
            if (_adapter == null)
            {
                throw new InvalidOperationException("Adapter is required. Call WithAdapter() before Build().");
            }

            // 鍒涘缓杞洏
            var wheel = new Wheel<T>(_config, _adapter);

            // 璁剧疆鍙€夌粍浠?
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

            // 璁㈤槄浜嬩欢
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

            if (_view != null)
            {
                wheel.SetView(_view);
            }

            return wheel;
        }

        /// <summary>
        /// 鍒涘缓绠€鍗曡疆鐩橈紙浣跨敤榛樿閰嶇疆锛?
        /// </summary>
        /// <returns>鏋勫缓鍣?/returns>
        public static WheelBuilder<T> CreateSimple()
        {
            return new WheelBuilder<T>();
        }
    }
}







