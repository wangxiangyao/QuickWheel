# QuickWheel 视图插件化说明（2025-11）

自 2025 年 11 月起，QuickWheel 将 UI 层从核心库中拆分：

- **核心库（QuickWheel.Core）** 仅负责槽位数据、状态机、事件总线等逻辑能力。
- **视图接口（`IWheelView<T>`）** 公开给使用方，自行决定如何渲染轮盘。
- **默认视图模块（QuickWheel.UI）** 提供 `DefaultWheelView<T>` 和 `UseDefaultView()` 扩展，复用原来的 9 宫格 UI。

## 新增接口结构

```csharp
public interface IWheelView<T> : IDisposable
{
    void Attach(Wheel<T> wheel, IWheelItemAdapter<T> adapter);
    void Detach();

    void OnWheelShown();
    void OnWheelHidden(int finalIndex);
    void OnSlotDataChanged(int index, T data);
    void OnSlotsSwapped(int index1, int index2);
    void OnSelectionChanged(int selectedIndex);
    void OnHoverChanged(int hoveredIndex);
}
```

`Wheel<T>.SetView(IWheelView<T>)` 用于绑定/替换视图，`Wheel` 不再主动创建 UI。

## 默认视图

`QuickWheel.UI` 模块包含：

- `DefaultWheelView<T>`：封装原 9 宫格 UI。
- `WheelUIManager<T>` / `WheelSlotDisplay`：内部使用的 UI 组件。
- `WheelBuilderDefaultViewExtensions.UseDefaultView()`：一行代码绑定默认视图。

```csharp
using QuickWheel.UI;

var wheel = new WheelBuilder<Item>()
    .WithAdapter(new ItemWheelAdapter())
    .UseDefaultView()      // 来自 QuickWheel.UI
    .Build();
```

## 迁移指南

| 旧 API            | 新写法                                       |
|-------------------|-----------------------------------------------|
| `.WithUI(true)`   | `.UseDefaultView()` （QuickWheel.UI 模块）    |
| `.WithUI(false)`  | 不调用视图接口，或自行实现 `IWheelView<T>`   |
| `EnableUI()` 等   | 由视图实现负责，不再由 `Wheel<T>` 暴露       |

若项目需要自定义外观，可实现 `IWheelView<T>` 并在 `WheelBuilder` 流程中使用 `.WithView(...)` 或在实例化后调用 `SetView()`。
