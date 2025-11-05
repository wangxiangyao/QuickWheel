# QuickWheel API 使用手册

> 版本：v2.0
> 日期：2025-01-05
> 目标：让任何开发者5分钟上手轮盘系统

---

## 📋 目录

1. [快速开始](#快速开始)
2. [基础API](#基础api)
3. [配置选项](#配置选项)
4. [事件系统](#事件系统)
5. [高级用法](#高级用法)
6. [完整示例](#完整示例)
7. [常见问题](#常见问题)

---

## 快速开始

### 最简使用（3行代码）

```csharp
using QuickWheel.Core;

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

## 基础API

### 创建轮盘

#### 方式1：简单模式（使用默认配置）

```csharp
var wheel = WheelBuilder.CreateSimple<T>()
    .WithAdapter(new MyAdapter())
    .Build();
```

#### 方式2：完整模式（自定义配置）

```csharp
var wheel = new WheelBuilder<T>()
    .WithConfig(config => {
        config.SlotCount = 6;
        config.SlotRadius = 150f;
        config.EnableDragSwap = true;
        config.EnableClickSelect = true;
        config.EnablePersistence = true;
        config.PersistenceKey = "MyWheel";
    })
    .WithAdapter(new MyAdapter())
    .WithDataProvider(new MyDataProvider())  // 可选
    .WithInput(new MouseWheelInput(KeyCode.V))  // 可选
    .WithPersistence(new JsonWheelPersistence<T>())  // 可选
    .WithSelectionStrategy(new AngleSelectionStrategy())  // 可选
    .OnItemSelected((index, item) => UseItem(item))  // 可选
    .OnWheelShown(() => Debug.Log("Wheel shown"))  // 可选
    .OnWheelHidden((index) => Debug.Log($"Wheel hidden, selected: {index}"))  // 可选
    .Build();
```

### 显示与隐藏

```csharp
// 显示轮盘（在指定位置）
wheel.Show(Vector2 position);

// 隐藏轮盘（不执行选择）
wheel.Hide(executeSelection: false);

// 隐藏轮盘（执行当前hover项）
wheel.Hide(executeSelection: true);  // 默认

// 检查轮盘是否显示中
bool isVisible = wheel.IsVisible;
```

### 槽位操作

```csharp
// 设置槽位数据
wheel.SetSlot(int index, T item);

// 获取槽位数据
T item = wheel.GetSlot(int index);

// 移除槽位数据
wheel.RemoveSlot(int index);

// 交换两个槽位
wheel.SwapSlots(int fromIndex, int toIndex);

// 清空所有槽位
wheel.ClearAllSlots();

// 批量设置槽位
wheel.SetSlots(T[] items);  // 数组长度必须等于SlotCount
```

### 选中状态

```csharp
// 设置选中索引（不触发使用）
wheel.SetSelectedIndex(int index);

// 获取当前选中索引
int selected = wheel.GetSelectedIndex();

// 获取当前hover索引
int hovered = wheel.GetHoveredIndex();
```

### 手动控制（不使用输入处理器）

```csharp
// 手动更新hover状态
wheel.ManualSetHover(int index);

// 手动确认选择
wheel.ManualConfirm();

// 手动取消
wheel.ManualCancel();
```

---

## 配置选项

### WheelConfig 完整配置

```csharp
var config = new WheelConfig
{
    // === 核心配置 ===
    SlotCount = 8,  // 槽位数量（3-8，强制约束）

    // === 布局配置 ===
    SlotRadius = 120f,  // 轮盘半径
    CustomAngles = null,  // 自定义角度分布（null=均匀分布）
    // 例如：CustomAngles = new float[] { 0, 45, 90, 135, 180, 225, 270, 315 };

    // === 交互配置 ===
    EnableDragSwap = true,  // 启用拖拽交换槽位
    EnableClickSelect = true,  // 启用左键点击选中
    DeadZoneRadius = 40f,  // 中心死区半径（像素）

    // === 视觉配置 ===
    HoverScaleMultiplier = 1.15f,  // hover时的缩放倍数
    AnimationDuration = 0.2f,  // 动画时长（秒）

    // === 持久化配置 ===
    EnablePersistence = false,  // 启用持久化
    PersistenceKey = "",  // 持久化键名（EnablePersistence=true时必须）
};

// 配置验证
if (!config.Validate(out string error))
{
    Debug.LogError($"配置错误: {error}");
}
```

### 全局配置

```csharp
// 影响所有轮盘的全局配置
WheelGlobalConfig.GlobalDeadZoneRadius = 40f;
WheelGlobalConfig.GlobalHoverScale = 1.15f;
WheelGlobalConfig.GlobalAnimationDuration = 0.2f;
```

---

## 事件系统

### 核心事件

```csharp
// 物品选中事件（最重要）
wheel.OnItemSelected += (int index, T item) =>
{
    Debug.Log($"选中了槽位{index}的物品: {item}");
};

// 轮盘显示事件
wheel.OnWheelShown += () =>
{
    Debug.Log("轮盘已显示");
    PlaySound("wheel_open");
};

// 轮盘隐藏事件（带最终选中索引，-1表示取消）
wheel.OnWheelHidden += (int finalIndex) =>
{
    if (finalIndex >= 0)
        Debug.Log($"轮盘隐藏，最终选中: {finalIndex}");
    else
        Debug.Log("轮盘隐藏，未选择");
};
```

### 详细事件

```csharp
// 槽位数据变更事件
wheel.OnSlotDataChanged += (int index, T newItem) =>
{
    Debug.Log($"槽位{index}的数据已更新");
};

// 槽位交换事件
wheel.OnSlotsSwapped += (int index1, int index2) =>
{
    Debug.Log($"槽位{index1}和{index2}已交换");
    SaveLayout();  // 保存布局
};

// 选中状态变更事件
wheel.OnSelectionChanged += (int newIndex) =>
{
    Debug.Log($"选中状态变更为: {newIndex}");
};

// Hover状态变更事件
wheel.OnSlotHovered += (int hoveredIndex) =>
{
    // 高频事件，谨慎使用
    UpdateTooltip(hoveredIndex);
};

// 槽位点击事件
wheel.OnSlotClicked += (int clickedIndex) =>
{
    Debug.Log($"点击了槽位: {clickedIndex}");
};
```

### 事件订阅管理

```csharp
// 订阅事件
wheel.OnItemSelected += HandleItemSelected;

// 取消订阅
wheel.OnItemSelected -= HandleItemSelected;

// 一次性事件（订阅后只触发一次）
Action<int, T> onceHandler = null;
onceHandler = (index, item) =>
{
    HandleItemSelected(index, item);
    wheel.OnItemSelected -= onceHandler;  // 自动取消订阅
};
wheel.OnItemSelected += onceHandler;
```

---

## 高级用法

### 1. 使用数据提供者（动态数据源）

```csharp
// 实现数据提供者
public class VoiceDataProvider : IWheelDataProvider<VoiceData>
{
    private List<VoiceData> _voices = new List<VoiceData>();

    public event Action<VoiceData> OnItemAdded;
    public event Action<VoiceData> OnItemRemoved;

    public IEnumerable<VoiceData> GetAvailableItems() => _voices;

    public bool IsValid(VoiceData item) => item != null;

    public void AddVoice(VoiceData voice)
    {
        _voices.Add(voice);
        OnItemAdded?.Invoke(voice);  // 轮盘会自动更新
    }
}

// 使用数据提供者
var dataProvider = new VoiceDataProvider();
var wheel = WheelBuilder.CreateSimple<VoiceData>()
    .WithAdapter(new VoiceWheelAdapter())
    .WithDataProvider(dataProvider)  // 自动监听数据变化
    .Build();

// 添加数据时，轮盘会自动更新
dataProvider.AddVoice(new VoiceData { Name = "Hello" });
```

### 2. 自定义输入处理

```csharp
// 实现自定义输入
public class CustomWheelInput : IWheelInputHandler
{
    public event Action<Vector2> OnPositionChanged;
    public event Action OnConfirm;
    public event Action OnCancel;

    private bool _isActive;

    public void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            _isActive = true;
        }

        if (_isActive)
        {
            // 发送位置
            OnPositionChanged?.Invoke(Input.mousePosition);

            if (Input.GetKeyUp(KeyCode.V))
            {
                _isActive = false;
                OnConfirm?.Invoke();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnCancel?.Invoke();
        }
    }
}

// 使用自定义输入
var wheel = new WheelBuilder<VoiceData>()
    .WithAdapter(new VoiceWheelAdapter())
    .WithInput(new CustomWheelInput())  // 自动处理输入
    .Build();
```

### 3. 自定义选择算法

```csharp
// 实现自定义选择策略
public class DistanceSelectionStrategy : IWheelSelectionStrategy
{
    public int GetSlotIndexFromPosition(
        Vector2 wheelCenter, Vector2 inputPosition,
        int slotCount, float[] slotAngles)
    {
        // 找到距离最近的槽位
        float minDistance = float.MaxValue;
        int closestIndex = -1;

        for (int i = 0; i < slotCount; i++)
        {
            Vector2 slotPos = CalculateSlotPosition(wheelCenter, slotAngles[i]);
            float distance = Vector2.Distance(inputPosition, slotPos);

            if (distance < minDistance)
            {
                minDistance = distance;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    public bool IsInDeadZone(Vector2 wheelCenter, Vector2 inputPosition, float deadZoneRadius)
    {
        return Vector2.Distance(wheelCenter, inputPosition) < deadZoneRadius;
    }
}

// 使用自定义选择策略
var wheel = new WheelBuilder<VoiceData>()
    .WithAdapter(new VoiceWheelAdapter())
    .WithSelectionStrategy(new DistanceSelectionStrategy())
    .Build();
```

### 4. 自定义持久化

```csharp
// 实现自定义持久化
public class PlayerPrefsWheelPersistence<T> : IWheelPersistence<T>
{
    public void Save(string key, WheelLayoutData<T> data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString($"Wheel_{key}", json);
        PlayerPrefs.Save();
    }

    public WheelLayoutData<T> Load(string key)
    {
        string json = PlayerPrefs.GetString($"Wheel_{key}", null);
        return json != null ? JsonUtility.FromJson<WheelLayoutData<T>>(json) : null;
    }

    public bool Has(string key)
    {
        return PlayerPrefs.HasKey($"Wheel_{key}");
    }

    public void Delete(string key)
    {
        PlayerPrefs.DeleteKey($"Wheel_{key}");
    }
}

// 使用自定义持久化
var wheel = new WheelBuilder<VoiceData>()
    .WithAdapter(new VoiceWheelAdapter())
    .WithConfig(cfg => {
        cfg.EnablePersistence = true;
        cfg.PersistenceKey = "MyVoiceWheel";
    })
    .WithPersistence(new PlayerPrefsWheelPersistence<VoiceData>())
    .Build();
```

### 5. 多轮盘管理

```csharp
public class WheelManager : MonoBehaviour
{
    private Wheel<Item> _itemWheel;
    private Wheel<VoiceData> _voiceWheel;
    private Wheel<EmoteData> _emoteWheel;

    void Start()
    {
        // 物品轮盘（1-8键）
        _itemWheel = new WheelBuilder<Item>()
            .WithAdapter(new ItemWheelAdapter())
            .WithInput(new MouseWheelInput(KeyCode.Alpha1))
            .OnItemSelected((i, item) => UseItem(item))
            .Build();

        // 语音轮盘（V键）
        _voiceWheel = new WheelBuilder<VoiceData>()
            .WithConfig(cfg => cfg.SlotCount = 6)
            .WithAdapter(new VoiceWheelAdapter())
            .WithInput(new MouseWheelInput(KeyCode.V))
            .OnItemSelected((i, voice) => PlayVoice(voice))
            .Build();

        // 表情轮盘（E键）
        _emoteWheel = new WheelBuilder<EmoteData>()
            .WithConfig(cfg => cfg.SlotCount = 4)
            .WithAdapter(new EmoteWheelAdapter())
            .WithInput(new MouseWheelInput(KeyCode.E))
            .OnItemSelected((i, emote) => PlayEmote(emote))
            .Build();
    }

    void Update()
    {
        // 输入处理器会自动更新
    }

    void OnDestroy()
    {
        // 清理资源
        _itemWheel?.Dispose();
        _voiceWheel?.Dispose();
        _emoteWheel?.Dispose();
    }
}
```

---

## 完整示例

### 示例1：物品轮盘

```csharp
using QuickWheel.Core;
using QuickWheel.Utils;
using UnityEngine;

public class ItemWheelExample : MonoBehaviour
{
    private Wheel<Item> _itemWheel;
    private Inventory _inventory;

    void Start()
    {
        // 创建物品轮盘
        _itemWheel = new WheelBuilder<Item>()
            .WithConfig(config => {
                config.SlotCount = 8;
                config.EnablePersistence = true;
                config.PersistenceKey = "ItemWheel";
            })
            .WithAdapter(new ItemWheelAdapter())
            .WithDataProvider(new InventoryDataProvider(_inventory))
            .WithInput(new MouseWheelInput(KeyCode.Alpha1))
            .OnItemSelected(UseItem)
            .OnWheelShown(() => Time.timeScale = 0.5f)  // 慢动作
            .OnWheelHidden((_) => Time.timeScale = 1f)
            .Build();

        // 初始化槽位
        InitializeSlots();
    }

    void InitializeSlots()
    {
        var items = _inventory.GetItems();
        for (int i = 0; i < Mathf.Min(items.Count, 8); i++)
        {
            _itemWheel.SetSlot(i, items[i]);
        }
    }

    void UseItem(int index, Item item)
    {
        Debug.Log($"使用物品: {item.DisplayName}");
        item.Use();

        // 如果物品用完了，从槽位移除
        if (item.Count <= 0)
        {
            _itemWheel.RemoveSlot(index);
        }
    }

    void OnDestroy()
    {
        _itemWheel?.Dispose();
    }
}
```

### 示例2：语音轮盘

```csharp
using QuickWheel.Core;
using UnityEngine;

public class VoiceWheelExample : MonoBehaviour
{
    private Wheel<VoiceData> _voiceWheel;
    private AudioSource _audioSource;

    [SerializeField]
    private VoiceData[] _availableVoices;  // 在Inspector中配置

    void Start()
    {
        // 创建语音轮盘
        _voiceWheel = WheelBuilder.CreateSimple<VoiceData>()
            .WithConfig(config => {
                config.SlotCount = 6;
                config.SlotRadius = 100f;
            })
            .WithAdapter(new VoiceWheelAdapter())
            .WithInput(new MouseWheelInput(KeyCode.V))
            .OnItemSelected(PlayVoice)
            .Build();

        // 添加语音
        for (int i = 0; i < _availableVoices.Length; i++)
        {
            _voiceWheel.SetSlot(i, _availableVoices[i]);
        }
    }

    void PlayVoice(int index, VoiceData voice)
    {
        Debug.Log($"播放语音: {voice.DisplayName}");
        _audioSource.clip = voice.AudioClip;
        _audioSource.Play();

        // 显示字幕
        ShowSubtitle(voice.SubtitleText);
    }

    void OnDestroy()
    {
        _voiceWheel?.Dispose();
    }
}
```

### 示例3：手动控制轮盘（不使用输入处理器）

```csharp
using QuickWheel.Core;
using UnityEngine;

public class ManualWheelControl : MonoBehaviour
{
    private Wheel<Item> _wheel;

    void Start()
    {
        _wheel = WheelBuilder.CreateSimple<Item>()
            .WithAdapter(new ItemWheelAdapter())
            .Build();  // 不添加输入处理器

        // 添加物品
        _wheel.SetSlot(0, myItem1);
        _wheel.SetSlot(1, myItem2);
    }

    void Update()
    {
        // 自定义触发逻辑
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            _wheel.Show(Input.mousePosition);
        }

        if (_wheel.IsVisible)
        {
            // 根据鼠标位置更新hover
            int hoveredIndex = CalculateHoveredIndex(Input.mousePosition);
            _wheel.ManualSetHover(hoveredIndex);

            // 松开Tab键确认
            if (Input.GetKeyUp(KeyCode.Tab))
            {
                _wheel.ManualConfirm();
            }

            // Esc取消
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _wheel.ManualCancel();
            }
        }
    }

    int CalculateHoveredIndex(Vector2 mousePos)
    {
        // 自定义选择逻辑
        // 例如：根据鼠标角度计算
        // ...
        return calculatedIndex;
    }
}
```

---

## 常见问题

### Q1: 如何修改槽位数量？

**A**: 在配置中设置`SlotCount`（约束：3-8）

```csharp
.WithConfig(config => config.SlotCount = 6)
```

### Q2: 如何禁用拖拽交换功能？

**A**: 在配置中禁用

```csharp
.WithConfig(config => config.EnableDragSwap = false)
```

### Q3: 如何保存用户的轮盘布局？

**A**: 启用持久化

```csharp
.WithConfig(config => {
    config.EnablePersistence = true;
    config.PersistenceKey = "UniqueKey";  // 每个轮盘唯一
})
.WithPersistence(new JsonWheelPersistence<T>())  // 或其他实现
```

### Q4: 如何在轮盘显示时暂停游戏？

**A**: 订阅显示/隐藏事件

```csharp
.OnWheelShown(() => Time.timeScale = 0f)
.OnWheelHidden((_) => Time.timeScale = 1f)
```

### Q5: 如何检测轮盘是否正在显示？

**A**: 使用`IsVisible`属性

```csharp
if (wheel.IsVisible)
{
    // 轮盘正在显示
}
```

### Q6: 如何实现多个轮盘切换？

**A**: 创建多个轮盘实例，分别绑定不同按键

```csharp
var wheel1 = ... .WithInput(new MouseWheelInput(KeyCode.Alpha1)).Build();
var wheel2 = ... .WithInput(new MouseWheelInput(KeyCode.V)).Build();
```

### Q7: 如何自定义轮盘的视觉样式？

**A**: 修改Prefab或继承`WheelSlotView`

```csharp
public class MyCustomSlotView : WheelSlotView
{
    protected override void SetHovered(bool hovered)
    {
        base.SetHovered(hovered);
        // 添加自定义视觉效果
    }
}
```

### Q8: 如何在轮盘中显示自定义类型？

**A**: 实现`IWheelItemAdapter<T>`

```csharp
public class MyAdapter : IWheelItemAdapter<MyType>
{
    public IWheelItem ToWheelItem(MyType data)
    {
        return new WheelItemWrapper {
            Icon = data.Sprite,
            DisplayName = data.Name,
            IsValid = data != null
        };
    }

    public MyType FromWheelItem(IWheelItem item) => null;
}
```

### Q9: 如何处理空槽位？

**A**: 返回`null`或`IsValid=false`的`IWheelItem`

```csharp
public IWheelItem ToWheelItem(Item item)
{
    if (item == null) return null;  // 自动处理为空槽位
    // ...
}
```

### Q10: 如何获取所有槽位的数据？

**A**: 遍历槽位索引

```csharp
for (int i = 0; i < wheel.Config.SlotCount; i++)
{
    T item = wheel.GetSlot(i);
    if (item != null)
    {
        // 处理物品
    }
}
```

---

## 性能优化建议

### 1. 避免高频事件中的重操作

```csharp
// ❌ 不好的做法
wheel.OnSlotHovered += (index) => {
    // 高频事件中执行重操作
    ExpensiveOperation();
};

// ✅ 好的做法
wheel.OnSlotHovered += (index) => {
    // 轻量级操作
    _hoveredIndex = index;
};
```

### 2. 及时取消事件订阅

```csharp
void OnEnable()
{
    wheel.OnItemSelected += HandleItemSelected;
}

void OnDisable()
{
    wheel.OnItemSelected -= HandleItemSelected;  // 防止内存泄漏
}
```

### 3. 复用轮盘实例

```csharp
// ❌ 不好的做法
void ShowWheel()
{
    var wheel = WheelBuilder.CreateSimple<Item>().Build();  // 每次创建
    wheel.Show(mousePos);
}

// ✅ 好的做法
private Wheel<Item> _wheel;

void Start()
{
    _wheel = WheelBuilder.CreateSimple<Item>().Build();  // 创建一次
}

void ShowWheel()
{
    _wheel.Show(mousePos);  // 复用
}
```

### 4. 批量设置槽位

```csharp
// ❌ 不好的做法
for (int i = 0; i < items.Length; i++)
{
    wheel.SetSlot(i, items[i]);  // 每次触发事件
}

// ✅ 好的做法
wheel.SetSlots(items);  // 批量设置，一次触发事件
```

---

## 总结

QuickWheel提供了简洁而强大的API，支持：

- ✅ **3行代码快速开始**
- ✅ **流畅的链式配置**
- ✅ **丰富的事件系统**
- ✅ **高度可定制**
- ✅ **性能优化**

更多详细信息，请参阅：
- [架构设计文档](Architecture.md)
- [接口说明文档](Interfaces.md)
- [示例教程](Examples.md)

---

**文档版本**：v2.0
**最后更新**：2025-01-05
**维护者**：QuickWheel团队
