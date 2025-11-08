using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using QuickWheel.Core;
using QuickWheel.Core.Interfaces;

namespace QuickWheel.UI
{
    /// <summary>
    /// 9宫格单个格子显示组件
    /// 负责单个格子的UI显示、hover效果和交互
    /// 手动创建UI，不依赖预制体
    /// 🆕 支持拖拽交换，限制只能与有物品的格子交换
    /// 🆕 支持点击选中功能
    /// </summary>
    public class WheelSlotDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        private RectTransform _rectTransform;
        private Image _bgImage;
        private Image _iconImage;
        private Text _labelText;

        private IWheelItem _wheelItem;
        private int _slotIndex;
        private bool _isSelected = false;
        private bool _isHovered = false;
        private WheelConfig _config;

        // 🆕 拖拽相关字段
        private bool _isDragging = false;
        private GameObject _dragGhost;  // 拖拽时的幽灵图像
        private Canvas _dragCanvas;  // 拖拽图像的Canvas
        private object _uiManager;  // UI管理器引用（使用object因为WheelUIManager是泛型类）

        // 🆕 置灰状态字段
        private bool _isDragOverInvalid = false;  // 是否有无效拖拽悬停在此格子上
        private bool _isAnimatingGray = false;    // 是否正在播放置灰动画
        private float _grayAnimationProgress = 0f;  // 置灰动画进度

        // 视觉参数
        private const float NORMAL_SCALE = 1f;
        private const float HOVER_SCALE = 1.15f;
        private const float ANIMATION_DURATION = 0.1f;
        private float _animationProgress = 0f;
        private bool _isAnimating = false;

        private readonly Color NORMAL_COLOR = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        private readonly Color HOVER_COLOR = new Color(0.3f, 0.3f, 0.3f, 0.9f);
        private readonly Color SELECTED_COLOR = new Color(0.4f, 0.6f, 0.8f, 1f);

        // 🆕 置灰状态颜色
        private readonly Color INVALID_DROP_COLOR = new Color(0.9f, 0.05f, 0.05f, 1.0f);  // 🔴 完全不透明的鲜红色，覆盖所有背景
        private readonly Color GRAY_COLOR = new Color(0.3f, 0.3f, 0.3f, 0.4f);  // 灰色调，表示空位

        /// <summary>
        /// 初始化格子
        /// </summary>
        public void Initialize(IWheelItem wheelItem, int slotIndex, Vector2 gridSize, WheelConfig config = null, object uiManager = null)
        {
            _wheelItem = wheelItem;
            _slotIndex = slotIndex;
            _config = config;
            _uiManager = uiManager;
            CreateDisplay(gridSize);
        }

        /// <summary>
        /// 手动创建UI（不依赖预制体）
        /// </summary>
        private void CreateDisplay(Vector2 gridSize)
        {
            _rectTransform = GetComponent<RectTransform>();
            if (_rectTransform == null)
            {
                _rectTransform = gameObject.AddComponent<RectTransform>();
            }

            // 设置格子大小
            _rectTransform.sizeDelta = gridSize;

            // 创建背景
            var bgObj = new GameObject("Background");
            bgObj.transform.SetParent(transform, false);

            var bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            _bgImage = bgObj.AddComponent<Image>();
            _bgImage.raycastTarget = true;

            // 如果配置了自定义Sprite，使用Sprite；否则使用纯色
            if (_config?.SlotNormalSprite != null)
            {
                _bgImage.sprite = _config.SlotNormalSprite;
                _bgImage.type = Image.Type.Sliced;  // 支持9宫格缩放
                _bgImage.color = Color.white;  // 不染色，显示原图
            }
            else
            {
                _bgImage.color = NORMAL_COLOR;  // 使用默认纯色
            }

            // 添加Button用于交互
            var button = bgObj.AddComponent<Button>();
            button.targetGraphic = _bgImage;

            // 总是创建图标对象，即使初始时没有图标
            var iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(transform, false);

            _iconImage = iconObj.AddComponent<Image>();
            _iconImage.color = Color.white;
            _iconImage.raycastTarget = false;

            var iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = new Vector2(4, 4);
            iconRect.offsetMax = new Vector2(-4, -4);

            // 如果初始时有图标，设置图标
            if (_wheelItem != null && _wheelItem.GetIcon() != null)
            {
                _iconImage.sprite = _wheelItem.GetIcon();
                _iconImage.gameObject.SetActive(true);
            }
            else
            {
                _iconImage.gameObject.SetActive(false);
            }

            // 总是创建标签对象，即使初始时没有文本
            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(transform, false);

            _labelText = labelObj.AddComponent<Text>();
            _labelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            _labelText.fontSize = 12;
            _labelText.alignment = TextAnchor.LowerCenter;
            _labelText.color = Color.white;
            _labelText.raycastTarget = false;

            var labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0);
            labelRect.anchorMax = new Vector2(1, 0.3f);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            // 如果初始时有标签，设置标签
            if (_wheelItem != null && !string.IsNullOrEmpty(_wheelItem.GetDisplayName()))
            {
                _labelText.text = _wheelItem.GetDisplayName();
                _labelText.gameObject.SetActive(true);
            }
            else
            {
                _labelText.gameObject.SetActive(false);
            }

            // 🆕 添加EventTrigger处理点击和拖拽事件
            // 因为 bgObj 的 Image 拦截了射线，需要将事件转发到主对象
            var eventTrigger = bgObj.GetComponent<EventTrigger>();
            if (eventTrigger == null)
            {
                eventTrigger = bgObj.AddComponent<EventTrigger>();
            }

            // 点击事件
            var clickEntry = new EventTrigger.Entry();
            clickEntry.eventID = EventTriggerType.PointerClick;
            clickEntry.callback.AddListener((data) => OnPointerClick((PointerEventData)data));
            eventTrigger.triggers.Add(clickEntry);

            // 🆕 拖拽事件转发（否则拖拽不工作）
            var beginDragEntry = new EventTrigger.Entry();
            beginDragEntry.eventID = EventTriggerType.BeginDrag;
            beginDragEntry.callback.AddListener((data) => OnBeginDrag((PointerEventData)data));
            eventTrigger.triggers.Add(beginDragEntry);

            var dragEntry = new EventTrigger.Entry();
            dragEntry.eventID = EventTriggerType.Drag;
            dragEntry.callback.AddListener((data) => OnDrag((PointerEventData)data));
            eventTrigger.triggers.Add(dragEntry);

            var endDragEntry = new EventTrigger.Entry();
            endDragEntry.eventID = EventTriggerType.EndDrag;
            endDragEntry.callback.AddListener((data) => OnEndDrag((PointerEventData)data));
            eventTrigger.triggers.Add(endDragEntry);

            // 🆕 Drop 事件转发
            var dropEntry = new EventTrigger.Entry();
            dropEntry.eventID = EventTriggerType.Drop;
            dropEntry.callback.AddListener((data) => OnDrop((PointerEventData)data));
            eventTrigger.triggers.Add(dropEntry);

            Debug.Log($"[WheelSlotDisplay] ✅ 已为格子 {_slotIndex} 添加EventTrigger（点击+拖拽+Drop）");
        }

        /// <summary>
        /// 设置选中状态
        /// </summary>
        public void SetSelected(bool selected)
        {
            if (_isSelected != selected)
            {
                _isSelected = selected;
                _isAnimating = true;
                _animationProgress = 0f;
            }
        }

        /// <summary>
        /// 设置悬停状态
        /// </summary>
        public void SetHovered(bool hovered)
        {
            if (_isHovered != hovered)
            {
                _isHovered = hovered;
                _isAnimating = true;
                _animationProgress = 0f;
            }
        }

        /// <summary>
        /// 🆕 设置无效拖拽悬停状态（拖到空格子时的置灰提示）
        /// </summary>
        public void SetDragOverInvalid(bool isInvalid)
        {
            if (_isDragOverInvalid != isInvalid)
            {
                _isDragOverInvalid = isInvalid;
                _isAnimatingGray = true;
                _grayAnimationProgress = 0f;
            }
        }

        /// <summary>
        /// 🆕 检查格子是否可以接收拖拽（是否有物品）
        /// </summary>
        public bool CanReceiveDrop()
        {
            return _wheelItem != null;  // 只有有物品的格子才能接收拖拽
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        public void SetData(IWheelItem wheelItem)
        {
            _wheelItem = wheelItem;

            // 更新图标
            if (_iconImage != null)
            {
                if (wheelItem != null && wheelItem.GetIcon() != null)
                {
                    _iconImage.sprite = wheelItem.GetIcon();
                    _iconImage.gameObject.SetActive(true);
                }
                else
                {
                    _iconImage.gameObject.SetActive(false);
                }
            }

            // 更新标签
            if (_labelText != null)
            {
                if (wheelItem != null && !string.IsNullOrEmpty(wheelItem.GetDisplayName()))
                {
                    _labelText.text = wheelItem.GetDisplayName();
                    _labelText.gameObject.SetActive(true);
                }
                else
                {
                    _labelText.gameObject.SetActive(false);
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // Hover 效果由QuickWheel系统通过OnHoverChanged回调处理
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // Hover 结束效果由QuickWheel系统通过OnHoverChanged回调处理
        }

        private void Update()
        {
            // 处理基础动画（选中/悬停）
            if (_isAnimating)
            {
                _animationProgress += Time.deltaTime / ANIMATION_DURATION;
                _animationProgress = Mathf.Clamp01(_animationProgress);

                // 计算目标缩放（hover时放大，包括选中状态的hover，但无效拖拽时不放大）
                bool shouldScale = _isHovered && !_isDragOverInvalid;
                float targetScale = shouldScale ? HOVER_SCALE : NORMAL_SCALE;
                float startScale = shouldScale ? NORMAL_SCALE : HOVER_SCALE;
                float currentScale = Mathf.Lerp(startScale, targetScale, _animationProgress);

                _rectTransform.localScale = new Vector3(currentScale, currentScale, 1f);
            }

            // 处理置灰动画
            if (_isAnimatingGray)
            {
                _grayAnimationProgress += Time.deltaTime / ANIMATION_DURATION;
                _grayAnimationProgress = Mathf.Clamp01(_grayAnimationProgress);
            }

            // 更新背景颜色（优先级: 无效拖拽 > 选中 > 悬停 > 普通）
            Color finalColor;

            if (_isDragOverInvalid)
            {
                // 🆕 无效拖拽状态：显示红色警告（最高优先级）
                finalColor = INVALID_DROP_COLOR;
            }
            else if (_config?.SlotNormalSprite != null)
            {
                // 使用自定义Sprite：Sprite已经设置，只需要调整颜色
                // 🆕 优先级：选中状态保持selected不变，即使hover也不切换
                if (_isSelected && _config.SlotSelectedSprite != null)
                {
                    // 选中状态：始终显示selected sprite，即使hover也不切换
                    _bgImage.sprite = _config.SlotSelectedSprite;
                    finalColor = Color.white;
                }
                else if (_isHovered && _config.SlotHoverSprite != null && _wheelItem != null)
                {
                    // 🆕 只有非选中且有物品的格子才切换到hover sprite
                    _bgImage.sprite = _config.SlotHoverSprite;
                    finalColor = Color.white;
                }
                else
                {
                    _bgImage.sprite = _config.SlotNormalSprite;

                    // 🆕 空格子时显示灰色，有物品时显示正常颜色
                    if (_wheelItem == null)
                    {
                        finalColor = GRAY_COLOR;  // 空格子：灰色染色
                    }
                    else
                    {
                        finalColor = Color.white;  // 有物品：正常颜色
                    }
                }
            }
            else
            {
                // 使用纯色背景（优先级: Selected > Hovered > Normal）
                if (_isSelected)
                {
                    finalColor = SELECTED_COLOR;
                }
                else if (_isHovered)
                {
                    finalColor = HOVER_COLOR;
                }
                else
                {
                    // 🆕 空格子时显示灰色，有物品时显示正常颜色
                    if (_wheelItem == null)
                    {
                        finalColor = GRAY_COLOR;  // 空格子：灰色
                    }
                    else
                    {
                        finalColor = NORMAL_COLOR;  // 有物品：正常颜色
                    }
                }
            }

            _bgImage.color = finalColor;

            // 完成动画
            if (_animationProgress >= 1f)
            {
                _isAnimating = false;
            }
            if (_grayAnimationProgress >= 1f)
            {
                _isAnimatingGray = false;
            }
        }

        public int GetSlotIndex() => _slotIndex;
        public IWheelItem GetWheelItem() => _wheelItem;

        /// <summary>
        /// 🆕 强制清理拖拽状态（用于异常情况）
        /// </summary>
        public void ForceCleanupDrag()
        {
            if (!_isDragging) return;

            _isDragging = false;

            // 通知 UIManager 结束拖拽
            if (_uiManager != null)
            {
                var method = _uiManager.GetType().GetMethod("SetDragging");
                method?.Invoke(_uiManager, new object[] { false });
            }

            // 销毁拖拽幽灵
            if (_dragGhost != null)
            {
                Destroy(_dragGhost);
                _dragGhost = null;
            }

            // 同时销毁拖拽用的 Canvas，避免残留到场景里
            if (_dragCanvas != null)
            {
                Destroy(_dragCanvas.gameObject);
                _dragCanvas = null;
            }

            Debug.Log($"[WheelSlotDisplay] Force cleaned up drag state for slot {_slotIndex}");
        }

        // ═══════════════════════ 🆕 拖拽功能实现 ═══════════════════════

        public void OnBeginDrag(PointerEventData eventData)
        {
            // 只有有物品的格子才能拖拽
            if (_wheelItem == null || _config == null || !_config.EnableDragSwap)
            {
                return;
            }

            _isDragging = true;

            // 🆕 通知 UIManager 开始拖拽（暂停输入处理）
            if (_uiManager != null)
            {
                var method = _uiManager.GetType().GetMethod("SetDragging");
                method?.Invoke(_uiManager, new object[] { true });
            }

            // 创建拖拽幽灵图像
            CreateDragGhost();

            Debug.Log($"[WheelSlotDisplay] Begin drag slot {_slotIndex}");
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging || _dragGhost == null)
            {
                return;
            }

            // 跟随鼠标移动
            _dragGhost.transform.position = eventData.position;

            // 🆕 检测鼠标下方的格子，提供置灰反馈
            CheckDragOverSlots(eventData);
        }

        /// <summary>
        /// 🆕 检测拖拽经过的格子，为空格子提供置灰反馈
        /// </summary>
        private void CheckDragOverSlots(PointerEventData eventData)
        {
            // 先清除所有置灰状态
            ClearAllDragOverInvalidStates();

            // 🔧 修复：使用轮盘的Canvas进行射线检测
            Canvas wheelCanvas = null;
            var parent = transform;
            while (parent != null)
            {
                var canvas = parent.GetComponent<Canvas>();
                if (canvas != null)
                {
                    wheelCanvas = canvas;
                    break;
                }
                parent = parent.parent;
            }

            if (wheelCanvas == null)
            {
                return;
            }

            var graphicRaycaster = wheelCanvas.GetComponent<GraphicRaycaster>();
            if (graphicRaycaster == null)
            {
                return;
            }

            // 使用射线检测找到鼠标下的格子
            var results = new System.Collections.Generic.List<RaycastResult>();
            graphicRaycaster.Raycast(eventData, results);


            // 检查射线结果
            foreach (var result in results)
            {
                var slotDisplay = result.gameObject.GetComponentInParent<WheelSlotDisplay>();
                if (slotDisplay != null && slotDisplay != this)
                {

                    // 如果目标格子是空的，标记为无效拖拽
                    if (!slotDisplay.CanReceiveDrop())
                    {
                        slotDisplay.SetDragOverInvalid(true);
                    }
                    break; // 只处理第一个找到的格子
                }
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDragging)
            {
                return;
            }

            _isDragging = false;

            // 🆕 通知 UIManager 结束拖拽（恢复输入处理）
            if (_uiManager != null)
            {
                var method = _uiManager.GetType().GetMethod("SetDragging");
                method?.Invoke(_uiManager, new object[] { false });
            }

            // 🆕 清除所有无效拖拽状态
            ClearAllDragOverInvalidStates();

            // 销毁拖拽幽灵
            if (_dragGhost != null)
            {
                Destroy(_dragGhost);
                _dragGhost = null;
            }

            // 同步销毁拖拽 Canvas，防止遗留
            if (_dragCanvas != null)
            {
                Destroy(_dragCanvas.gameObject);
                _dragCanvas = null;
            }

            Debug.Log($"[WheelSlotDisplay] End drag slot {_slotIndex}");
        }

        public void OnDrop(PointerEventData eventData)
        {
            Debug.Log($"[WheelSlotDisplay] ⚪ OnDrop called on slot {_slotIndex}");

            // 清除所有无效拖拽状态
            ClearAllDragOverInvalidStates();

            // 获取拖拽的源格子（因为 EventTrigger 在 bgObj 上，pointerDrag 也是 bgObj）
            var draggedSlot = eventData.pointerDrag?.GetComponent<WheelSlotDisplay>();
            if (draggedSlot == null)
            {
                // 尝试从父对象获取
                draggedSlot = eventData.pointerDrag?.GetComponentInParent<WheelSlotDisplay>();
            }

            if (draggedSlot == null)
            {
                Debug.LogWarning($"[WheelSlotDisplay] ❌ OnDrop: Cannot find source WheelSlotDisplay from {eventData.pointerDrag?.name}");
                return;
            }

            if (draggedSlot == this)
            {
                Debug.Log($"[WheelSlotDisplay] Drop on self, ignoring");

                // 🆕 清除拖拽状态和虚影（以防 OnEndDrag 未正确触发）
                ForceCleanupDrag();

                return;
            }

            // 🆕 检查目标格子是否可以接收拖拽（必须有物品）
            if (!this.CanReceiveDrop())
            {
                Debug.Log($"[WheelSlotDisplay] ❌ Drop rejected: target slot {this.GetSlotIndex()} is empty");

                // 🆕 清除拖拽源的虚影（以防 OnEndDrag 未正确触发）
                draggedSlot.ForceCleanupDrag();

                return;  // 不执行交换
            }

            // 执行交换
            int fromIndex = draggedSlot.GetSlotIndex();
            int toIndex = this.GetSlotIndex();

            Debug.Log($"[WheelSlotDisplay] ✅ Drop: {fromIndex} -> {toIndex} (both have items)");

            // 通知UI管理器执行交换（使用反射因为WheelUIManager是泛型类）
            if (_uiManager != null)
            {
                var method = _uiManager.GetType().GetMethod("SwapSlots");
                if (method != null)
                {
                    method.Invoke(_uiManager, new object[] { fromIndex, toIndex });
                    Debug.Log($"[WheelSlotDisplay] ✅ SwapSlots invoked successfully");
                }
                else
                {
                    Debug.LogWarning("[WheelSlotDisplay] ❌ SwapSlots method not found on UIManager");
                }
            }
            else
            {
                Debug.LogWarning("[WheelSlotDisplay] ❌ UIManager is null");
            }
        }

        /// <summary>
        /// 🆕 点击事件处理
        /// 点击格子时通过UI管理器触发轮盘点击逻辑
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log($"[WheelSlotDisplay] ⚪ OnPointerClick called: slotIndex={_slotIndex}, wheelItem={_wheelItem != null}, uiManager={_uiManager != null}");

            // 只有有物品的格子才能点击
            if (_wheelItem == null)
            {
                Debug.Log($"[WheelSlotDisplay] ❌ Click ignored: slot {_slotIndex} is empty");
                return;
            }

            // 🆕 通过UI管理器的标准方法处理点击（使用反射因为WheelUIManager是泛型类）
            if (_uiManager != null)
            {
                Debug.Log($"[WheelSlotDisplay] ⚪ Getting HandleSlotClick method via reflection...");
                var method = _uiManager.GetType().GetMethod("HandleSlotClick");
                if (method != null)
                {
                    Debug.Log($"[WheelSlotDisplay] ⚪ Invoking HandleSlotClick({_slotIndex})...");
                    method.Invoke(_uiManager, new object[] { _slotIndex });
                    Debug.Log($"[WheelSlotDisplay] ⚪ HandleSlotClick invoked successfully");
                }
                else
                {
                    Debug.LogWarning("[WheelSlotDisplay] ❌ HandleSlotClick method not found on UIManager");
                }
            }
            else
            {
                Debug.LogWarning("[WheelSlotDisplay] ❌ UIManager is null");
            }
        }

        /// <summary>
        /// 🆕 清除所有格子的无效拖拽状态
        /// 在拖拽结束时调用
        /// </summary>
        private void ClearAllDragOverInvalidStates()
        {
            // 查找所有WheelSlotDisplay并清除状态
            var allSlots = FindObjectsOfType<WheelSlotDisplay>();
            foreach (var slot in allSlots)
            {
                slot.SetDragOverInvalid(false);
            }
        }

        /// <summary>
        /// 创建拖拽幽灵图像
        /// </summary>
        private void CreateDragGhost()
        {
            // 创建拖拽Canvas（超高层级，确保在所有UI之上）
            var canvasObj = new GameObject("DragCanvas");
            _dragCanvas = canvasObj.AddComponent<Canvas>();
            _dragCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _dragCanvas.sortingOrder = 30000;  // 🔧 提高层级到30000，远高于轮盘(10000)

            // 不需要GraphicRaycaster，因为幽灵不接收输入
            // canvasObj.AddComponent<GraphicRaycaster>();

            // 创建幽灵图像
            _dragGhost = new GameObject("DragGhost");
            _dragGhost.transform.SetParent(canvasObj.transform, false);

            var ghostImage = _dragGhost.AddComponent<Image>();
            if (_iconImage != null && _iconImage.sprite != null)
            {
                ghostImage.sprite = _iconImage.sprite;
            }

            ghostImage.color = new Color(1f, 1f, 1f, 0.7f);  // 🔧 稍微提高透明度到0.7
            ghostImage.raycastTarget = false;  // 不阻挡射线

            var ghostRect = _dragGhost.GetComponent<RectTransform>();
            ghostRect.sizeDelta = _rectTransform.sizeDelta * 0.85f;  // 🔧 85%大小，更明显

            Debug.Log($"[WheelSlotDisplay] Created drag ghost with sortingOrder: {_dragCanvas.sortingOrder}");
        }
    }
}
