# QuickWheel - 通用轮盘模块

> 高度解耦、完全泛型、易于使用的Unity轮盘选择系统

[![Version](https://img.shields.io/badge/version-2.0-blue.svg)](https://github.com/yourusername/QuickWheel)
[![Unity](https://img.shields.io/badge/Unity-2020.3+-green.svg)](https://unity.com)
[![License](https://img.shields.io/badge/license-MIT-orange.svg)](LICENSE)

---

## ✨ 特性

- ✅ **完全泛型** - 支持任意数据类型（Item、Voice、Emote等）
- ✅ **3-8槽位可配置** - 灵活的槽位数量约束
- ✅ **易于使用** - Builder模式，3行代码即可上手
- ✅ **高度解耦** - 核心、UI、业务三层零依赖
- ✅ **事件驱动** - 完善的事件系统，响应式更新
- ✅ **可选功能** - 持久化、输入处理、数据提供者均可选
- ✅ **多种输入** - 鼠标、手柄、触摸、VR等
- ✅ **可扩展** - 支持自定义选择算法、持久化方式等

---

## 🚀 快速开始

### 最简使用（3行代码）

```csharp
using QuickWheel.Core;
using QuickWheel.Utils;

// 1. 创建轮盘
var wheel = WheelBuilder.CreateSimple<VoiceData>()
    .WithAdapter(new VoiceWheelAdapter())
    .Build();

// 2. 添加数据
wheel.SetSlot(0, new VoiceData { Name = "Hello", Icon = mySprite });

// 3. 显示轮盘
wheel.Show(Input.mousePosition);
```

### 监听选择事件

```csharp
wheel.OnItemSelected += (index, voiceData) =>
{
    Debug.Log($"选中了: {voiceData.Name}");
    PlayVoice(voiceData);
};
```

---

## 📦 项目结构

```
QuickWheel/
├── Documentation/               # 📚 完整文档
│   ├── Architecture.md         # 架构设计文档
│   ├── API.md                  # API使用手册
│   └── Interfaces.md           # 接口说明文档
│
├── Core/                        # 🎯 核心层（完全通用）
│   ├── Interfaces/             # 核心接口定义
│   │   ├── IWheelItem.cs
│   │   ├── IWheelItemAdapter.cs
│   │   ├── IWheelDataProvider.cs
│   │   ├── IWheelPersistence.cs
│   │   ├── IWheelInputHandler.cs
│   │   └── IWheelSelectionStrategy.cs
│   │
│   ├── States/                 # 状态管理
│   │   ├── WheelState.cs
│   │   └── WheelStateManager.cs
│   │
│   ├── Wheel.cs                # 主类（泛型）
│   ├── WheelConfig.cs          # 配置类
│   ├── WheelGlobalConfig.cs    # 全局配置
│   └── WheelEventBus.cs        # 事件总线
│
├── Utils/                       # 🔧 工具类
│   ├── WheelItemWrapper.cs     # IWheelItem默认实现
│   └── WheelBuilder.cs         # 构建器
│
├── UI/                          # 🎨 UI层（待实现）
│   ├── WheelViewController.cs
│   ├── WheelSlotView.cs
│   └── WheelAnimator.cs
│
├── Input/                       # ⌨️ 输入实现（待实现）
│   ├── MouseWheelInput.cs
│   └── GamepadWheelInput.cs
│
├── Selection/                   # 🎯 选择算法（待实现）
│   └── AngleSelectionStrategy.cs
│
├── Persistence/                 # 💾 持久化（待实现）
│   └── JsonWheelPersistence.cs
│
└── Examples/                    # 📖 示例代码（待实现）
    ├── ItemWheel/
    └── VoiceWheel/
```

---

## 📊 开发进度

### ✅ Phase 1: 核心层开发（已完成）

- [x] 定义所有核心接口
  - IWheelItem - UI显示接口
  - IWheelItemAdapter - 适配器接口
  - IWheelDataProvider - 数据源接口
  - IWheelPersistence - 持久化接口
  - IWheelInputHandler - 输入处理接口
  - IWheelSelectionStrategy - 选择算法接口

- [x] 实现配置系统
  - WheelConfig - 实例配置（3-8槽位约束）
  - WheelGlobalConfig - 全局配置

- [x] 实现事件系统
  - WheelEventBus - 事件总线
  - 防循环触发锁机制

- [x] 实现状态管理
  - WheelState - 状态枚举
  - WheelStateManager - 状态管理器

- [x] 实现主类
  - Wheel<T> - 泛型主类
  - 完整的API接口

- [x] 实现工具类
  - WheelItemWrapper - 默认包装
  - WheelBuilder - 构建器

### 🚧 Phase 2: UI层开发（待开始）

- [ ] WheelViewController - 轮盘视图控制器
- [ ] WheelSlotView - 单个槽位视图
- [ ] WheelAnimator - 动画控制器
- [ ] Unity Prefabs - 可视化预制体

### 🚧 Phase 3: 默认实现（待开始）

- [ ] MouseWheelInput - 鼠标输入
- [ ] AngleSelectionStrategy - 角度选择算法
- [ ] JsonWheelPersistence - JSON持久化

### 🚧 Phase 4: 示例代码（待开始）

- [ ] ItemWheel - 物品轮盘示例
- [ ] VoiceWheel - 语音轮盘示例
- [ ] 完整的使用教程

---

## 📚 文档

详细文档请查看 `Documentation/` 目录：

- **[Architecture.md](Documentation/Architecture.md)** - 架构设计文档
  - 三层架构详解
  - 类图和数据流图
  - 设计模式说明
  - 与旧架构对比

- **[API.md](Documentation/API.md)** - API使用手册
  - 快速开始教程
  - 完整API参考
  - 配置选项说明
  - 常见用法示例

- **[Interfaces.md](Documentation/Interfaces.md)** - 接口说明文档
  - 每个接口的详细说明
  - 实现指南和最佳实践
  - 完整示例代码

---

## 🏗️ 架构概览

### 三层架构

```
业务层（ItemWheel、VoiceWheel）
    ↓ 通过适配器
适配层（Adapter、DataProvider）
    ↓ 实现接口
核心层（Wheel<T>、完全泛型）
    ↓ 事件驱动
UI层（WheelViewController、通用视图）
```

### 数据流

```
业务数据 → DataProvider → Adapter → 核心State → 事件通知 → UI渲染
```

### 事件流

```
用户输入 → InputHandler → Wheel → StateManager → EventBus → UI/业务响应
```

---

## 💡 使用示例

### 完整配置示例

```csharp
var wheel = new WheelBuilder<VoiceData>()
    .WithConfig(config => {
        config.SlotCount = 6;
        config.SlotRadius = 150f;
        config.EnablePersistence = true;
        config.PersistenceKey = "MyVoiceWheel";
    })
    .WithAdapter(new VoiceWheelAdapter())
    .WithDataProvider(new VoiceDataProvider())
    .WithInput(new MouseWheelInput(KeyCode.V))
    .OnItemSelected((index, data) => PlayVoice(data))
    .OnWheelShown(() => Debug.Log("Wheel shown"))
    .Build();
```

### 多轮盘管理

```csharp
public class WheelManager : MonoBehaviour
{
    private Wheel<Item> _itemWheel;
    private Wheel<VoiceData> _voiceWheel;
    private Wheel<EmoteData> _emoteWheel;

    void Start()
    {
        _itemWheel = CreateItemWheel();   // 1-8键
        _voiceWheel = CreateVoiceWheel(); // V键
        _emoteWheel = CreateEmoteWheel(); // E键
    }
}
```

---

## 🔧 核心API

### 创建轮盘

```csharp
// 简单模式
var wheel = WheelBuilder.CreateSimple<T>()
    .WithAdapter(adapter)
    .Build();

// 完整模式
var wheel = new WheelBuilder<T>()
    .WithConfig(config => { ... })
    .WithAdapter(adapter)
    .WithDataProvider(provider)
    .WithInput(input)
    .Build();
```

### 显示与隐藏

```csharp
wheel.Show(position);           // 显示轮盘
wheel.Hide(executeSelection);   // 隐藏轮盘
bool isVisible = wheel.IsVisible;
```

### 槽位操作

```csharp
wheel.SetSlot(index, item);     // 设置槽位
T item = wheel.GetSlot(index);  // 获取槽位
wheel.RemoveSlot(index);        // 移除槽位
wheel.SwapSlots(from, to);      // 交换槽位
wheel.ClearAllSlots();          // 清空所有
```

### 事件订阅

```csharp
wheel.OnItemSelected += (index, item) => { };
wheel.OnWheelShown += () => { };
wheel.OnWheelHidden += (finalIndex) => { };
```

---

## 🎯 设计目标

### 已实现

- ✅ **完全解耦** - 核心不依赖任何业务逻辑
- ✅ **类型安全** - 泛型设计 + 接口约束
- ✅ **易于使用** - Builder模式 + 链式API
- ✅ **高度灵活** - 3-8槽位可配置
- ✅ **可选功能** - 持久化、输入由实例决定
- ✅ **输入解耦** - 不内置触发方式
- ✅ **事件驱动** - 完善的事件系统

### 待实现

- ⏳ UI层可视化
- ⏳ 默认输入实现
- ⏳ 默认选择算法
- ⏳ 示例代码

---

## 📝 许可证

MIT License - 详见 [LICENSE](LICENSE) 文件

---

## 🤝 贡献

欢迎提交Issue和Pull Request！

---

## 📮 联系方式

- 项目地址：[GitHub](https://github.com/yourusername/QuickWheel)
- 问题反馈：[Issues](https://github.com/yourusername/QuickWheel/issues)

---

**版本**: v2.0
**最后更新**: 2025-01-05
**开发者**: QuickWheel Team
