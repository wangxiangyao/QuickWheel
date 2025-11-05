using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using QuickWheel.Core.Interfaces;

namespace QuickWheel.Examples.GridWheel
{
    /// <summary>
    /// 9宫格单个格子显示组件
    /// 负责单个格子的UI显示、hover效果和交互
    /// 手动创建UI，不依赖预制体
    /// </summary>
    public class GridWheelDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private RectTransform _rectTransform;
        private Image _bgImage;
        private Image _iconImage;
        private Text _labelText;

        private IWheelItem _wheelItem;
        private int _slotIndex;
        private bool _isHovered = false;
        private bool _isSelected = false;

        // 视觉参数
        private const float NORMAL_SCALE = 1f;
        private const float HOVER_SCALE = 1.15f;
        private const float ANIMATION_DURATION = 0.1f;
        private float _animationProgress = 0f;
        private bool _isAnimating = false;

        private readonly Color NORMAL_COLOR = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        private readonly Color HOVER_COLOR = new Color(0.3f, 0.3f, 0.3f, 0.9f);
        private readonly Color SELECTED_COLOR = new Color(0.4f, 0.6f, 0.8f, 1f);

        /// <summary>
        /// 初始化格子
        /// </summary>
        public void Initialize(IWheelItem wheelItem, int slotIndex, Vector2 gridSize)
        {
            _wheelItem = wheelItem;
            _slotIndex = slotIndex;
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
            _bgImage.color = NORMAL_COLOR;
            _bgImage.raycastTarget = true;

            // 添加Button用于交互
            var button = bgObj.AddComponent<Button>();
            button.targetGraphic = _bgImage;

            // 如果有图标，添加图标Image
            if (_wheelItem != null && _wheelItem.GetIcon() != null)
            {
                var iconObj = new GameObject("Icon");
                iconObj.transform.SetParent(transform, false);

                _iconImage = iconObj.AddComponent<Image>();
                _iconImage.sprite = _wheelItem.GetIcon();
                _iconImage.color = Color.white;
                _iconImage.raycastTarget = false;

                var iconRect = iconObj.GetComponent<RectTransform>();
                iconRect.anchorMin = Vector2.zero;
                iconRect.anchorMax = Vector2.one;
                iconRect.offsetMin = new Vector2(4, 4);
                iconRect.offsetMax = new Vector2(-4, -4);
            }

            // 如果有标签，添加Text
            if (_wheelItem != null && !string.IsNullOrEmpty(_wheelItem.GetDisplayName()))
            {
                var labelObj = new GameObject("Label");
                labelObj.transform.SetParent(transform, false);

                _labelText = labelObj.AddComponent<Text>();
                _labelText.text = _wheelItem.GetDisplayName();
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
            }
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
            _isHovered = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
        }

        private void Update()
        {
            if (!_isAnimating) return;

            _animationProgress += Time.deltaTime / ANIMATION_DURATION;
            _animationProgress = Mathf.Clamp01(_animationProgress);

            // 计算目标缩放
            float targetScale = _isSelected ? HOVER_SCALE : NORMAL_SCALE;
            float currentScale = Mathf.Lerp(
                _isSelected ? NORMAL_SCALE : HOVER_SCALE,
                targetScale,
                _animationProgress
            );

            _rectTransform.localScale = new Vector3(currentScale, currentScale, 1f);

            // 计算目标颜色
            Color targetColor = _isSelected ? SELECTED_COLOR : NORMAL_COLOR;
            Color startColor = _isSelected ? NORMAL_COLOR : SELECTED_COLOR;
            Color currentColor = Color.Lerp(startColor, targetColor, _animationProgress);
            _bgImage.color = currentColor;

            if (_animationProgress >= 1f)
            {
                _isAnimating = false;
            }
        }

        public int GetSlotIndex() => _slotIndex;
        public IWheelItem GetWheelItem() => _wheelItem;
    }
}
