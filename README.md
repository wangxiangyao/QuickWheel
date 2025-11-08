# QuickWheel - 通用轮盘框架（Unity Mod）

QuickWheel 是一个面向 Unity Mod 的 9 宫格轮盘选择框架。内置 UI、泛型数据模型、事件驱动、支持拖拽交换与可选持久化，3 行代码即可创建可用轮盘。

## 特性

- 内置 9 宫格 UI：代码创建/销毁，无需预制体
- 固定布局：8 个方向槽位 + 1 个中心占位
- 泛型模型：`Wheel<T>` + `IWheelItemAdapter<T>`
- 简单构建器：`WheelBuilder<T>`
- 方向选择：`GridSelectionStrategy` 基于鼠标方向/角度
- 可选持久化：JSON 保存布局
- 事件驱动：显示/隐藏/选中/槽位变更/拖拽交换
- UI 可配置：格子尺寸/间距、死区半径、格子 Sprite（普通/悬停/选中）

## 快速开始

最小示例（3 行 + 内置 UI）：

```csharp
// 1) 创建轮盘（自动创建 9 宫格 UI）
var wheel = new WheelBuilder<YourDataType>()
    .WithAdapter(new YourAdapter())
    .Build();

// 2) 设置数据
wheel.SetSlot(0, data1);
wheel.SetSlot(1, data2);

// 3) 显示/隐藏
wheel.Show();  // 显示在鼠标位置
wheel.Hide();
```

完整示例（含 UI 更新）：

```csharp
using UnityEngine;
using QuickWheel.Core;
using QuickWheel.Utils;
using QuickWheel.Selection;

public class VoiceWheelExample : MonoBehaviour
{
    private Wheel<VoiceData> _wheel;
    private GridSelectionStrategy _selection;

    void Start()
    {
        _wheel = new WheelBuilder<VoiceData>()
            .WithAdapter(new VoiceWheelAdapter())
            .WithSelectionStrategy(new GridSelectionStrategy())
            .OnItemSelected((index, voice) => Debug.Log($"选中: {voice.DisplayName}"))
            .Build();

        _selection = new GridSelectionStrategy();

        // 填充槽位
        _wheel.SetSlot(0, voiceData1);
        _wheel.SetSlot(1, voiceData2);
    }

    void Update()
    {
        // 1) 处理显示/隐藏
        bool pressed = Input.GetKey(KeyCode.V);
        if (pressed && !_wheel.IsVisible) _wheel.Show();
        else if (!pressed && _wheel.IsVisible) _wheel.Hide();

        // 2) 更新选择（根据鼠标方向）
        if (_wheel.IsVisible)
        {
            var rect = _wheel.GetUIContainerRect();
            Vector2 center = rect.position;
            Vector2 mouse = Input.mousePosition;
            if (!_selection.IsInDeadZone(center, mouse, 20f))
            {
                int i = _selection.GetSlotIndexFromPosition(center, mouse, 9, null);
                _wheel.UpdateUISelection(i == 8 ? -1 : i);
            }
        }
    }

    void OnDestroy() => _wheel?.Dispose();
}
```

适配器示例：

```csharp
public class VoiceWheelAdapter : IWheelItemAdapter<VoiceData>
{
    public IWheelItem ToWheelItem(VoiceData v)
    {
        if (v == null) return null;
        return new WheelItemWrapper { Icon = v.Icon, DisplayName = v.DisplayName, IsValid = true };
    }
    public VoiceData FromWheelItem(IWheelItem item) => null;
}
```

## 配置项（节选）

```csharp
new WheelBuilder<T>()
  .WithConfig(cfg => {
      cfg.GridCellSize   = 90f;    // 格子像素
      cfg.GridSpacing    = 12f;    // 间距
      cfg.DeadZoneRadius = 40f;    // 死区半径
      cfg.SlotNormalSprite   = normal;
      cfg.SlotHoverSprite    = hover;
      cfg.SlotSelectedSprite = selected;
  })
```

## 拖拽交换

- 同一轮盘内拖动两个槽位可交换数据
- 拖至空格显示红色“无效”态，不修改数据

## 与上层 Mod 配合

- 通过 `IWheelItemAdapter<T>` 映射业务数据 → UI
- 订阅 `OnItemSelected` 等事件完成确认逻辑
- 可选 `IWheelItemDecor`（上层 Mod 使用）提供名称着色/数量/耐久等装饰数据

## 版本

- 与 ItemWheel v1.1.0 配套发布（回归稳定 UI：背景 Sprite/拖拽/置灰）

