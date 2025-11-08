using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using QuickWheel.Core;
using QuickWheel.Core.Interfaces;
using QuickWheel.Utils;

namespace QuickWheel.UI
{
    /// <summary>
    /// 9瀹牸杞洏UI绠＄悊鍣?
    /// 璐熻矗鍒涘缓銆佹樉绀哄拰鏇存柊9瀹牸UI
    /// 鑷姩闆嗘垚鍒癢heel鏍稿績绫伙紝鏃犻渶鎵嬪姩鍒涘缓UI
    /// </summary>
    public class WheelUIManager<T>
    {
        // UI缁勪欢
        private Canvas _wheelCanvas;
        private GameObject _wheelContainer;
        private List<WheelSlotDisplay> _slotDisplays = new List<WheelSlotDisplay>();
        private GameObject _inputBlocker;

        // 杞洏寮曠敤
        private Wheel<T> _wheel;
        private IWheelItemAdapter<T> _adapter;

        // 褰撳墠閫変腑绱㈠紩
        private int _currentSelectedIndex = -1;

        // 杞洏鏄剧ず鏃剁殑涓績浣嶇疆
        private Vector2 _wheelCenter;

        // 馃啎 鎷栨嫿鐘舵€佹爣蹇楋紙鐢ㄤ簬鏆傚仠杈撳叆澶勭悊锛?
        private bool _isDragging = false;
        public bool IsDragging => _isDragging;

        // 9瀹牸浣嶇疆鏄犲皠锛堝睆骞曞潗鏍囷紝鐩稿浜庤疆鐩樹腑蹇冿級
        private static readonly Vector2Int[] GRID_POSITIONS = new Vector2Int[]
        {
            new Vector2Int(-1,  0),  // 0: 宸︿腑
            new Vector2Int( 1,  0),  // 1: 鍙充腑
            new Vector2Int( 0, -1),  // 2: 涓婁腑
            new Vector2Int( 0,  1),  // 3: 涓嬩腑
            new Vector2Int(-1,  1),  // 4: 宸︿笅
            new Vector2Int( 1,  1),  // 5: 鍙充笅
            new Vector2Int( 1, -1),  // 6: 鍙充笂
            new Vector2Int(-1, -1),  // 7: 宸︿笂
            new Vector2Int( 0,  0),  // 8: 涓績锛堜笉浣跨敤锛?
        };

        /// <summary>
        /// 鏄惁宸叉樉绀?
        /// </summary>
        public bool IsVisible => _wheelContainer != null && _wheelContainer.activeSelf;

        /// <summary>
        /// 杞洏瀹瑰櫒RectTransform
        /// </summary>
        public RectTransform ContainerRect => _wheelContainer?.GetComponent<RectTransform>();

        /// <summary>
        /// 鍒濆鍖朥I绠＄悊鍣?
        /// </summary>
        public WheelUIManager(Wheel<T> wheel, IWheelItemAdapter<T> adapter, Transform parent = null)
        {
            _wheel = wheel ?? throw new ArgumentNullException(nameof(wheel));
            _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));

            CreateUI(parent);
            SubscribeToWheelEvents();
        }

        /// <summary>
        /// 鍒涘缓9瀹牸UI
        /// </summary>
        private void CreateUI(Transform parent)
        {
            // 鍒涘缓Canvas
            var canvasObj = new GameObject("QuickWheelCanvas");
            if (parent != null)
                canvasObj.transform.SetParent(parent, false);

            _wheelCanvas = canvasObj.AddComponent<Canvas>();
            _wheelCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _wheelCanvas.sortingOrder = 10000;  // 鎻愰珮灞傜骇锛岀‘淇濆湪娓告垙UI涔嬩笂

            var canvasScaler = canvasObj.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);

            canvasObj.AddComponent<GraphicRaycaster>();

            // 鍒涘缓杈撳叆鎷︽埅闈㈡澘锛堥槻姝㈣緭鍏ヤ紶閫掑埌娓告垙锛?
            CreateInputBlocker(canvasObj.transform);

            // 鍒涘缓杞洏瀹瑰櫒
            _wheelContainer = new GameObject("WheelContainer");
            _wheelContainer.transform.SetParent(canvasObj.transform, false);

            var containerRect = _wheelContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            containerRect.sizeDelta = new Vector2(200, 200);

            // 鍒涘缓9涓牸瀛?
            float cellSize = _wheel.Config.GridCellSize;
            float spacing = _wheel.Config.GridSpacing;
            float offset = cellSize + spacing;

            for (int i = 0; i < 9; i++)
            {
                // 璺宠繃涓績鏍煎瓙锛堢储寮?锛?
                if (i == 8) continue;

                var slotObj = new GameObject($"Slot_{i}");
                slotObj.transform.SetParent(_wheelContainer.transform, false);

                var slotRect = slotObj.AddComponent<RectTransform>();
                slotRect.anchorMin = new Vector2(0.5f, 0.5f);
                slotRect.anchorMax = new Vector2(0.5f, 0.5f);

                // 璁＄畻浣嶇疆
                Vector2 gridPos = new Vector2(
                    GRID_POSITIONS[i].x * offset,
                    -GRID_POSITIONS[i].y * offset  // Y杞村弽杞紙Unity UI鍧愭爣绯伙級
                );
                slotRect.anchoredPosition = gridPos;

                // 娣诲姞鏄剧ず缁勪欢
                var display = slotObj.AddComponent<WheelSlotDisplay>();
                var wheelItem = ConvertToWheelItem(_wheel.GetSlot(i));
                display.Initialize(wheelItem, i, new Vector2(cellSize, cellSize), _wheel.Config, this);  // 馃啎 浼犲叆UIManager寮曠敤鐢ㄤ簬鎷栨嫿浜ゆ崲
                _slotDisplays.Add(display);
            }

            // 鍒濆闅愯棌
            _wheelContainer.SetActive(false);
            _inputBlocker.SetActive(false);

            Debug.Log("[WheelUIManager] log");
        }

        /// <summary>
        /// 鍒涘缓杈撳叆鎷︽埅闈㈡澘
        /// </summary>
        private void CreateInputBlocker(Transform parent)
        {
            _inputBlocker = new GameObject("InputBlocker");
            _inputBlocker.transform.SetParent(parent, false);

            var blockerRect = _inputBlocker.AddComponent<RectTransform>();
            blockerRect.anchorMin = Vector2.zero;
            blockerRect.anchorMax = Vector2.one;
            blockerRect.offsetMin = Vector2.zero;
            blockerRect.offsetMax = Vector2.zero;

            var blockerImage = _inputBlocker.AddComponent<Image>();
            // 浣跨敤閫忔槑鑳屾櫙鎷︽埅杈撳叆
            // Unity鐗规€э細alpha=0鏃朵笉浼氭嫤鎴皠绾挎娴嬶紝闇€瑕佹瀬灏忕殑alpha鍊?
            blockerImage.color = new Color(0, 0, 0, 0.01f);  // 鍑犱箮閫忔槑鐨勮儗鏅紝鎷︽埅鎵€鏈夎緭鍏?
            blockerImage.raycastTarget = true;

            // 纭繚鍦ㄨ疆鐩樺鍣ㄤ笅鏂?
            _inputBlocker.transform.SetAsFirstSibling();
        }

        /// <summary>
        /// 璁㈤槄杞洏浜嬩欢
        /// </summary>
        private void SubscribeToWheelEvents()
        {
            _wheel.EventBus.OnSlotDataChanged += OnSlotDataChanged;
            _wheel.EventBus.OnSlotsSwapped += OnSlotsSwapped;
        }

        /// <summary>
        /// 馃啎 鏍囧噯鐐瑰嚮澶勭悊鏂规硶
        /// 澶勭悊妲戒綅鐐瑰嚮浜嬩欢锛岃Е鍙戣疆鐩樼殑鐐瑰嚮閫昏緫
        /// </summary>
        /// <param name="slotIndex">鐐瑰嚮鐨勬Ы浣嶇储寮?/param>
        public void HandleSlotClick(int slotIndex)
        {
            Debug.Log($"[WheelUIManager] 馃煟 HandleSlotClick called: slotIndex={slotIndex}");

            // 閫氳繃EventBus瑙﹀彂鐐瑰嚮浜嬩欢锛岃Wheel澶勭悊閫夋嫨鍜屽叧闂€昏緫
            _wheel.EventBus.TriggerSlotClicked(slotIndex);

            Debug.Log($"[WheelUIManager] 馃煟 HandleSlotClick finished");
        }

        /// <summary>
        /// 鏄剧ず杞洏
        /// </summary>
        /// <param name="centerPosition">杞洏涓績浣嶇疆锛堝彲閫夛紝涓簄ull鍒欎娇鐢ㄥ綋鍓嶉紶鏍囦綅缃級</param>
        public void Show(Vector2? centerPosition = null)
        {
            if (_wheelContainer == null) return;

            _wheelContainer.SetActive(true);
            _inputBlocker.SetActive(true);

            // 浣跨敤鎻愪緵鐨勪腑蹇冧綅缃紝鎴栧綋鍓嶉紶鏍囦綅缃?
            if (centerPosition.HasValue)
            {
                _wheelCenter = centerPosition.Value;
                Debug.Log($"[WheelUIManager] 浣跨敤棰勮涓績浣嶇疆: {_wheelCenter}");
            }
            else
            {
                _wheelCenter = UnityEngine.Input.mousePosition;
                Debug.Log($"[WheelUIManager] 浣跨敤褰撳墠榧犳爣浣嶇疆: {_wheelCenter}");
            }

            // 杞洏鏄剧ず鍦ㄤ腑蹇冧綅缃?
            var containerRect = _wheelContainer.GetComponent<RectTransform>();
            containerRect.position = _wheelCenter;

            Debug.Log($"[WheelUIManager] 杞洏宸叉樉绀猴紝涓績浣嶇疆: {_wheelCenter}");
        }

        /// <summary>
        /// 鑾峰彇杞洏涓績浣嶇疆
        /// </summary>
        public Vector2 GetWheelCenter()
        {
            return _wheelCenter;
        }

        /// <summary>
        /// 闅愯棌杞洏
        /// </summary>
        public void Hide()
        {
            if (_wheelContainer == null) return;

            // 鍏滃簳锛氬叧闂墠寮哄埗娓呯悊鎵€鏈夋嫋鎷界姸鎬佷笌 hover锛岄伩鍏?EndDrag/Drop 涓㈠け瀵艰嚧娈嬬暀
            foreach (var display in _slotDisplays)
            {
                display.ForceCleanupDrag();
            }
            UpdateHover(-1);

            _wheelContainer.SetActive(false);
            _inputBlocker.SetActive(false);

            // 娓呴櫎閫変腑
            UpdateSelection(-1);

            Debug.Log("[WheelUIManager] log");
        }

        /// <summary>
        /// 鏇存柊閫変腑鐘舵€?
        /// </summary>
        public void UpdateSelection(int selectedIndex)
        {
            if (selectedIndex == _currentSelectedIndex) return;

            _currentSelectedIndex = selectedIndex;

            foreach (var display in _slotDisplays)
            {
                display.SetSelected(display.GetSlotIndex() == selectedIndex);
            }
        }

        /// <summary>
        /// 鏇存柊鎮仠鐘舵€?
        /// </summary>
        public void UpdateHover(int hoveredIndex)
        {
            foreach (var display in _slotDisplays)
            {
                display.SetHovered(display.GetSlotIndex() == hoveredIndex);
            }
        }

        /// <summary>
        /// 妲戒綅鏁版嵁鍙樺寲浜嬩欢澶勭悊
        /// </summary>
        private void OnSlotDataChanged(int index, T data)
        {
            if (index < 0 || index >= _slotDisplays.Count) return;

            var wheelItem = ConvertToWheelItem(data);
            _slotDisplays[index].SetData(wheelItem);
        }

        /// <summary>
        /// 妲戒綅浜ゆ崲浜嬩欢澶勭悊
        /// </summary>
        private void OnSlotsSwapped(int index1, int index2)
        {
            if (index1 < 0 || index1 >= _slotDisplays.Count) return;
            if (index2 < 0 || index2 >= _slotDisplays.Count) return;

            // 馃啎 閫変腑鐘舵€佽窡闅忕墿鍝佺Щ鍔?
            if (_currentSelectedIndex == index1)
            {
                Debug.Log($"[WheelUIManager] Selected index moved: {index1} -> {index2}");
                UpdateSelection(index2);
            }
            else if (_currentSelectedIndex == index2)
            {
                Debug.Log($"[WheelUIManager] Selected index moved: {index2} -> {index1}");
                UpdateSelection(index1);
            }

            // 鍒锋柊涓や釜妲戒綅鐨勬樉绀?
            var data1 = _wheel.GetSlot(index1);
            var data2 = _wheel.GetSlot(index2);

            _slotDisplays[index1].SetData(ConvertToWheelItem(data1));
            _slotDisplays[index2].SetData(ConvertToWheelItem(data2));
        }

        /// <summary>
        /// 灏嗘暟鎹浆鎹负IWheelItem
        /// </summary>
        private IWheelItem ConvertToWheelItem(T data)
        {
            if (data == null) return null;
            return _adapter.ToWheelItem(data);
        }

        /// <summary>
        /// 鍒锋柊鎵€鏈夋Ы浣嶆樉绀?
        /// </summary>
        public void RefreshAllSlots()
        {
            for (int i = 0; i < _slotDisplays.Count; i++)
            {
                var data = _wheel.GetSlot(i);
                _slotDisplays[i].SetData(ConvertToWheelItem(data));
            }
        }

        /// <summary>
        /// 娓呯悊璧勬簮
        /// </summary>
        public void Dispose()
        {
            if (_wheel != null)
            {
                _wheel.EventBus.OnSlotDataChanged -= OnSlotDataChanged;
                _wheel.EventBus.OnSlotsSwapped -= OnSlotsSwapped;
            }

            if (_wheelCanvas != null)
            {
                UnityEngine.Object.Destroy(_wheelCanvas.gameObject);
            }

            _slotDisplays.Clear();
        }

        /// <summary>
        /// 馃啎 浜ゆ崲涓や釜妲戒綅鐨勬暟鎹紙鎷栨嫿鍔熻兘锛?
        /// </summary>
        /// <summary>
        /// 馃啎 璁剧疆鎷栨嫿鐘舵€侊紙鐢ㄤ簬鏆傚仠杈撳叆澶勭悊锛?
        /// </summary>
        public void SetDragging(bool isDragging)
        {
            _isDragging = isDragging;
            Debug.Log($"[WheelUIManager] Dragging state: {_isDragging}");
        }

        public void SwapSlots(int fromIndex, int toIndex)
        {
            Debug.Log($"[WheelUIManager] SwapSlots: {fromIndex} <-> {toIndex}");

            // 杈圭晫妫€鏌?
            if (fromIndex < 0 || fromIndex >= _slotDisplays.Count ||
                toIndex < 0 || toIndex >= _slotDisplays.Count)
            {
                Debug.LogWarning($"[WheelUIManager] Invalid slot indices: {fromIndex}, {toIndex}");
                return;
            }

            // 璋冪敤 Wheel 鏍稿績杩涜鏁版嵁浜ゆ崲锛堣繖浼氳Е鍙?EventBus 浜嬩欢锛?
            if (_wheel != null)
            {
                _wheel.SwapSlots(fromIndex, toIndex);
                Debug.Log($"[WheelUIManager] Called Wheel.SwapSlots({fromIndex}, {toIndex})");

                // 绔嬪嵆鍒锋柊UI鏄剧ず
                RefreshSlot(fromIndex);
                RefreshSlot(toIndex);
            }
            else
            {
                Debug.LogWarning("[WheelUIManager] Wheel is null, cannot swap slots");
            }
        }

        /// <summary>
        /// 鍒锋柊鍗曚釜妲戒綅鐨勬樉绀?
        /// </summary>
        private void RefreshSlot(int index)
        {
            if (index < 0 || index >= _slotDisplays.Count) return;

            var display = _slotDisplays[index];
            var wheelItem = ConvertToWheelItem(_wheel.GetSlot(index));
            display.SetData(wheelItem);
        }

        /// <summary>
        /// 馃啎 閫変腑鎸囧畾妲戒綅骞跺叧闂疆鐩?
        /// </summary>
        /// <param name="index">瑕侀€変腑鐨勬Ы浣嶇储寮?/param>
        public void SelectAndClose(int index)
        {
            Debug.Log($"[WheelUIManager] SelectAndClose: {index}");

            if (index < 0 || index >= _slotDisplays.Count)
            {
                Debug.LogWarning($"[WheelUIManager] Invalid slot index: {index}");
                return;
            }

            // 璁剧疆閫変腑绱㈠紩
            _wheel.SetSelectedIndex(index);

            // 绔嬪嵆鍏抽棴杞洏锛堟墽琛岄€夋嫨锛?
            _wheel.Hide(true);
        }
    }
}



