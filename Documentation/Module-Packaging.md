# QuickWheel 模块打包和使用指南

## 🎯 目标

让其他mod开发者只需引用一个 `QuickWheel.dll` 就能使用所有轮盘功能。

## 📦 打包方式

### 方式1：编译单一dll（推荐）

```bash
# 使用模块项目文件编译
dotnet build QuickWheel.Module.csproj -c Release

# 生成的文件位置：
# bin/Release/QuickWheel.dll
```

### 方式2：创建NuGet包

```bash
# 打包为NuGet包
dotnet pack QuickWheel.Module.csproj -c Release

# 生成的文件位置：
# bin/Release/QuickWheel.1.0.0.nupkg
```

### 方式3：Unity Package（手动创建）

1. 在Unity中创建新的Package
2. 将 `QuickWheel.dll` 添加到包中
3. 包含文档和示例
4. 导出为 `QuickWheel.unitypackage`

## 🚀 其他mod的使用方式

### 步骤1：获取QuickWheel.dll

其他mod开发者可以通过以下方式获取：

```bash
# 方式1：直接复制dll文件
# 从你的项目 bin/Release/QuickWheel.dll 复制

# 方式2：NuGet包管理器
Install-Package QuickWheel

# 方式3：Unity Package Manager
# 添加com.quickwheel包
```

### 步骤2：添加到项目引用

```csharp
// 在其他mod的.csproj中添加引用
<ItemGroup>
  <Reference Include="QuickWheel">
    <HintPath>libs/QuickWheel.dll</HintPath>
    <Private>false</Private>
  </Reference>
</ItemGroup>
```

### 步骤3：开始使用

```csharp
using QuickWheel;
using UnityEngine;

public class MyMod : MonoBehaviour
{
    private Wheel<Item> _itemWheel;
    private Wheel<VoiceData> _voiceWheel;

    void Start()
    {
        // === 物品轮盘 ===
        _itemWheel = QuickWheel.Create<Item>()
            .WithConfig(config => {
                config.SlotCount = 8;
                config.SlotRadius = 120f;
                config.EnableDragSwap = true;
            })
            .WithAdapter(new ItemWheelAdapter())
            .WithMouseInput(KeyCode.Q)
            .WithPersistence("ItemWheel")
            .OnItemSelected(UseItem)
            .Build();

        // 设置物品数据
        var items = GetBackpackItems();
        _itemWheel.SetSlots(items);

        // === 语音轮盘 ===
        _voiceWheel = QuickWheel.Create<VoiceData>()
            .WithConfig(config => config.SlotCount = 6)
            .WithAdapter(new VoiceWheelAdapter())
            .WithMouseInput(KeyCode.V)
            .OnItemSelected(PlayVoice)
            .Build();

        var voices = GetVoiceData();
        _voiceWheel.SetSlots(voices);
    }

    void Update()
    {
        // 更新轮盘输入
        _itemWheel?.Update();
        _voiceWheel?.Update();
    }

    void UseItem(int index, Item item)
    {
        Debug.Log($"使用物品: {item.Name}");
        // 执行使用物品的逻辑
    }

    void PlayVoice(int index, VoiceData voice)
    {
        Debug.Log($"播放语音: {voice.DisplayName}");
        // 执行播放语音的逻辑
    }
}
```

## 🔧 高级用法示例

### 自定义适配器

```csharp
public class MyCustomAdapter : IWheelItemAdapter<MyData>
{
    public IWheelItem ToWheelItem(MyData data)
    {
        if (data == null) return null;

        return new WheelItemWrapper
        {
            Icon = data.Icon,
            DisplayName = data.Name,
            IsValid = true
        };
    }

    public MyData FromWheelItem(IWheelItem item)
    {
        return null; // 通常不需要反向转换
    }
}

// 使用自定义适配器
var wheel = QuickWheel.Create<MyData>()
    .WithAdapter(new MyCustomAdapter())
    .Build();
```

### 复杂配置

```csharp
var wheel = QuickWheel.Create<MyDataType>()
    .WithConfig(config => {
        config.SlotCount = 6;
        config.SlotRadius = 150f;
        config.EnableDragSwap = true;
        config.EnableClickSelect = true;
        config.EnablePersistence = true;
        config.PersistenceKey = "MyComplexWheel";
        config.HoverScaleMultiplier = 1.2f;
        config.AnimationDuration = 0.3f;
        config.DeadZoneRadius = 50f;
        config.CustomAngles = new float[] { 0, 60, 120, 180, 240, 300 }; // 自定义角度
    })
    .WithAdapter(new MyAdapter())
    .WithMouseInput(KeyCode.F)
    .OnItemSelected((index, data) => {
        Debug.Log($"选择了: {data.Name}");
        // 处理选择逻辑
    })
    .OnWheelShown(() => Debug.Log("轮盘显示"))
    .OnWheelHidden((index) => Debug.Log($"轮盘隐藏，选择了索引: {index}"))
    .Build();
```

## 📋 依赖说明

QuickWheel.dll 包含以下功能：

- **核心功能**：Wheel、WheelConfig、事件系统
- **UI组件**：WheelViewController、WheelSlotView、动画系统
- **工具类**：WheelBuilder、WheelItemWrapper
- **输入处理**：MouseWheelInput、键盘输入支持
- **持久化**：JsonWheelPersistence
- **选择策略**：AngleSelectionStrategy

### 外部依赖

- **Unity Engine**：UnityEngine.dll、UnityEngine.UI.dll
- **Harmony**：Lib.Harmony.dll（用于mod注入）
- **游戏引用**：TeamSoda.*、Assembly-CSharp.dll（根据游戏调整）

## 🎯 最佳实践

### 1. 命名规范

```csharp
// 推荐：使用描述性的变量名
private Wheel<Item> _backpackWheel;
private Wheel<VoiceData> _voiceWheel;
private Wheel<SkillData> _skillWheel;

// 推荐：使用有意义的事件处理
.OnItemSelected(OnBackpackItemSelected)
.OnItemSelected(OnVoiceSelected)
```

### 2. 资源管理

```csharp
void OnDestroy()
{
    // 确保释放轮盘资源
    _backpackWheel?.Dispose();
    _voiceWheel?.Dispose();
    _skillWheel?.Dispose();
}
```

### 3. 性能优化

```csharp
void Update()
{
    // 只在有轮盘显示时更新
    if (_backpackWheel?.IsVisible == true)
        _backpackWheel.Update();
    if (_voiceWheel?.IsVisible == true)
        _voiceWheel.Update();
}
```

## 🐛 常见问题

### Q: 轮盘不显示？
A: 检查：
1. 是否调用了 `Show()` 方法
2. 是否设置了有效数据
3. 是否在 `Update()` 中调用轮盘更新

### Q: 点击没反应？
A: 检查：
1. 是否设置了输入处理器
2. UI层级是否正确
3. 是否启用了点击选择

### Q: 数据不持久化？
A: 检查：
1. 是否启用了持久化配置
2. 是否设置了唯一的PersistenceKey
3. 是否有读写权限

## 📞 支持

如果遇到问题：
1. 查看示例代码：`Examples/` 目录
2. 检查API文档：`Documentation/API.md`
3. 提交Issue到项目仓库

---

**版本**：v1.0.0
**最后更新**：2025-01-05
**维护者**：QuickWheel Team