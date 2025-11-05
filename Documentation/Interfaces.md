# QuickWheel 接口说明文档

> 版本：v2.0
> 日期：2025-01-05
> 目标：详细说明所有接口的实现指南

---

## 📋 目录

1. [IWheelItem - UI显示接口](#iwheelitem---ui显示接口)
2. [IWheelDataProvider - 数据源接口](#iwheeldataprovider---数据源接口)
3. [IWheelItemAdapter - 适配器接口](#iwheelitemadapter---适配器接口)
4. [IWheelPersistence - 持久化接口](#iwheelpersistence---持久化接口)
5. [IWheelInputHandler - 输入处理接口](#iwheelinputhandler---输入处理接口)
6. [IWheelSelectionStrategy - 选择算法接口](#iwheelselectionstrategy---选择算法接口)
7. [接口实现示例](#接口实现示例)
8. [最佳实践](#最佳实践)

---

## IWheelItem - UI显示接口

### 接口定义

```csharp
/// <summary>
/// 轮盘项的UI显示接口
/// 所有显示在轮盘上的内容都必须实现此接口
/// </summary>
public interface IWheelItem
{
    /// <summary>
    /// 获取显示图标
    /// </summary>
    /// <returns>Sprite对象，null表示无图标</returns>
    Sprite GetIcon();

    /// <summary>
    /// 获取显示名称
    /// </summary>
    /// <returns>显示文本，null或空字符串表示无文本</returns>
    string GetDisplayName();

    /// <summary>
    /// 是否为有效项（用于处理null/空槽）
    /// </summary>
    /// <returns>true=有效显示，false=隐藏该槽位</returns>
    bool IsValid();
}
```

### 设计目的

- **解耦UI与业务数据**：UI层只认识`IWheelItem`，不关心具体的业务类型
- **统一显示规范**：所有类型的数据都通过统一接口提供显示信息
- **空槽位处理**：通过`IsValid()`统一处理空槽位逻辑

### 实现指南

#### 方式1：直接实现（业务类实现接口）

```csharp
public class VoiceData : IWheelItem
{
    public string VoiceID { get; set; }
    public string DisplayName { get; set; }
    public Sprite Icon { get; set; }

    // 实现IWheelItem
    public Sprite GetIcon() => Icon;
    public string GetDisplayName() => DisplayName;
    public bool IsValid() => !string.IsNullOrEmpty(VoiceID);
}
```

**优点**：简单直接，无需适配器
**缺点**：业务类需要依赖UI接口，耦合度略高

#### 方式2：通过适配器（推荐）

```csharp
// 业务类保持独立
public class VoiceData
{
    public string VoiceID { get; set; }
    public string DisplayName { get; set; }
    public Sprite Icon { get; set; }
}

// 适配器负责转换
public class VoiceWheelAdapter : IWheelItemAdapter<VoiceData>
{
    public IWheelItem ToWheelItem(VoiceData data)
    {
        if (data == null) return null;

        return new WheelItemWrapper
        {
            Icon = data.Icon,
            DisplayName = data.DisplayName,
            IsValid = !string.IsNullOrEmpty(data.VoiceID)
        };
    }

    public VoiceData FromWheelItem(IWheelItem item) => null;
}
```

**优点**：业务类不依赖UI，解耦更彻底
**缺点**：需要额外的适配器类

#### 方式3：使用默认包装类

```csharp
// 使用系统提供的WheelItemWrapper
var wheelItem = new WheelItemWrapper
{
    Icon = mySprite,
    DisplayName = "Hello",
    IsValid = true
};
```

**适用场景**：快速原型、简单数据

### 注意事项

1. **null处理**：`GetIcon()`和`GetDisplayName()`可以返回null，UI会自动处理
2. **IsValid()的语义**：
   - `true`：显示该槽位
   - `false`：隐藏该槽位（视觉上为空）
3. **性能考虑**：这些方法会被频繁调用，避免重操作

---

## IWheelDataProvider - 数据源接口

### 接口定义

```csharp
/// <summary>
/// 数据提供者接口
/// 负责提供轮盘的数据源，监听数据变化
/// </summary>
public interface IWheelDataProvider<T>
{
    /// <summary>
    /// 获取所有可用数据
    /// </summary>
    /// <returns>数据集合</returns>
    IEnumerable<T> GetAvailableItems();

    /// <summary>
    /// 数据添加事件
    /// </summary>
    event Action<T> OnItemAdded;

    /// <summary>
    /// 数据移除事件
    /// </summary>
    event Action<T> OnItemRemoved;

    /// <summary>
    /// 数据变更事件（旧数据, 新数据）
    /// </summary>
    event Action<T, T> OnItemChanged;

    /// <summary>
    /// 验证数据有效性
    /// </summary>
    /// <param name="item">要验证的数据</param>
    /// <returns>true=有效，false=无效</returns>
    bool IsValid(T item);
}
```

### 设计目的

- **动态数据源**：支持数据实时变化（背包物品增删、网络数据更新等）
- **自动同步**：数据变化时自动触发事件，轮盘自动更新
- **可选实现**：如果数据是静态的，可以不使用DataProvider

### 实现指南

#### 完整实现示例

```csharp
public class InventoryDataProvider : IWheelDataProvider<Item>
{
    private Inventory _inventory;  // 业务数据源

    public InventoryDataProvider(Inventory inventory)
    {
        _inventory = inventory;

        // 订阅业务事件
        _inventory.OnItemAdded += HandleItemAdded;
        _inventory.OnItemRemoved += HandleItemRemoved;
    }

    // 实现IWheelDataProvider
    public event Action<Item> OnItemAdded;
    public event Action<Item> OnItemRemoved;
    public event Action<Item, Item> OnItemChanged;

    public IEnumerable<Item> GetAvailableItems()
    {
        return _inventory.GetAllItems();
    }

    public bool IsValid(Item item)
    {
        return item != null && item.Count > 0;
    }

    // 业务事件处理
    private void HandleItemAdded(Item item)
    {
        OnItemAdded?.Invoke(item);  // 转发给轮盘
    }

    private void HandleItemRemoved(Item item)
    {
        OnItemRemoved?.Invoke(item);  // 转发给轮盘
    }

    // 清理
    public void Dispose()
    {
        _inventory.OnItemAdded -= HandleItemAdded;
        _inventory.OnItemRemoved -= HandleItemRemoved;
    }
}
```

#### 简单实现（静态数据）

```csharp
public class StaticVoiceProvider : IWheelDataProvider<VoiceData>
{
    private List<VoiceData> _voices;

    // 事件不使用（静态数据）
    public event Action<VoiceData> OnItemAdded;
    public event Action<VoiceData> OnItemRemoved;
    public event Action<VoiceData, VoiceData> OnItemChanged;

    public IEnumerable<VoiceData> GetAvailableItems() => _voices;

    public bool IsValid(VoiceData item) => item != null;
}
```

### 使用方式

```csharp
// 创建数据提供者
var dataProvider = new InventoryDataProvider(myInventory);

// 创建轮盘时传入
var wheel = new WheelBuilder<Item>()
    .WithAdapter(new ItemWheelAdapter())
    .WithDataProvider(dataProvider)  // 轮盘会自动订阅事件
    .Build();

// 数据变化时轮盘会自动更新
myInventory.AddItem(newItem);  // 触发OnItemAdded → 轮盘更新
```

### 注意事项

1. **事件线程安全**：如果数据变化发生在非主线程，需要切换到主线程再触发事件
2. **避免循环触发**：不要在事件处理中再次修改数据
3. **内存泄漏**：记得取消订阅业务事件

---

## IWheelItemAdapter - 适配器接口

### 接口定义

```csharp
/// <summary>
/// 轮盘项适配器接口
/// 负责将业务数据转换为UI可显示的IWheelItem
/// </summary>
public interface IWheelItemAdapter<T>
{
    /// <summary>
    /// 将业务数据转换为UI可显示对象
    /// </summary>
    /// <param name="data">业务数据</param>
    /// <returns>UI显示对象，null表示空槽位</returns>
    IWheelItem ToWheelItem(T data);

    /// <summary>
    /// 从UI对象还原为业务数据（可选实现）
    /// </summary>
    /// <param name="item">UI对象</param>
    /// <returns>业务数据，通常返回null</returns>
    T FromWheelItem(IWheelItem item);
}
```

### 设计目的

- **类型转换桥梁**：连接业务类型`T`与UI类型`IWheelItem`
- **保持业务类独立**：业务类不需要实现`IWheelItem`接口
- **集中转换逻辑**：所有转换逻辑集中管理

### 实现指南

#### 基础实现

```csharp
public class ItemWheelAdapter : IWheelItemAdapter<Item>
{
    public IWheelItem ToWheelItem(Item item)
    {
        // null处理
        if (item == null) return null;

        // 返回包装对象
        return new WheelItemWrapper
        {
            Icon = item.Icon,
            DisplayName = item.DisplayName,
            IsValid = item.Count > 0
        };
    }

    // 通常不需要反向转换
    public Item FromWheelItem(IWheelItem wheelItem)
    {
        return null;
    }
}
```

#### 高级实现（带缓存优化）

```csharp
public class CachedItemAdapter : IWheelItemAdapter<Item>
{
    // 缓存转换结果
    private Dictionary<Item, IWheelItem> _cache = new Dictionary<Item, IWheelItem>();

    public IWheelItem ToWheelItem(Item item)
    {
        if (item == null) return null;

        // 检查缓存
        if (_cache.TryGetValue(item, out var cached))
        {
            return cached;
        }

        // 创建新对象
        var wheelItem = new WheelItemWrapper
        {
            Icon = item.Icon,
            DisplayName = $"{item.DisplayName} x{item.Count}",
            IsValid = item.Count > 0
        };

        // 缓存
        _cache[item] = wheelItem;
        return wheelItem;
    }

    public Item FromWheelItem(IWheelItem wheelItem)
    {
        return null;
    }

    // 清理缓存
    public void ClearCache()
    {
        _cache.Clear();
    }
}
```

#### 复杂类型适配

```csharp
public class SkillWheelAdapter : IWheelItemAdapter<PlayerSkill>
{
    private IconAtlas _iconAtlas;  // 图标资源管理器

    public SkillWheelAdapter(IconAtlas iconAtlas)
    {
        _iconAtlas = iconAtlas;
    }

    public IWheelItem ToWheelItem(PlayerSkill skill)
    {
        if (skill == null) return null;

        return new WheelItemWrapper
        {
            Icon = _iconAtlas.GetIcon(skill.IconID),
            DisplayName = GetSkillDisplayName(skill),
            IsValid = skill.IsUnlocked && !skill.IsOnCooldown
        };
    }

    private string GetSkillDisplayName(PlayerSkill skill)
    {
        // 复杂的名称生成逻辑
        if (skill.IsOnCooldown)
            return $"{skill.Name} ({skill.RemainingCooldown}s)";
        else
            return skill.Name;
    }

    public PlayerSkill FromWheelItem(IWheelItem wheelItem)
    {
        return null;
    }
}
```

### 注意事项

1. **null安全**：务必处理`data`为null的情况
2. **性能考虑**：如果转换很重，考虑缓存结果
3. **反向转换**：`FromWheelItem()`通常不需要实现，返回null即可

---

## IWheelPersistence - 持久化接口

### 接口定义

```csharp
/// <summary>
/// 轮盘持久化接口（可选功能）
/// </summary>
public interface IWheelPersistence<T>
{
    /// <summary>
    /// 保存轮盘状态
    /// </summary>
    /// <param name="key">唯一键名</param>
    /// <param name="data">布局数据</param>
    void Save(string key, WheelLayoutData<T> data);

    /// <summary>
    /// 加载轮盘状态
    /// </summary>
    /// <param name="key">唯一键名</param>
    /// <returns>布局数据，null表示不存在</returns>
    WheelLayoutData<T> Load(string key);

    /// <summary>
    /// 检查是否存在保存数据
    /// </summary>
    /// <param name="key">唯一键名</param>
    /// <returns>true=存在，false=不存在</returns>
    bool Has(string key);

    /// <summary>
    /// 删除保存数据
    /// </summary>
    /// <param name="key">唯一键名</param>
    void Delete(string key);
}

/// <summary>
/// 持久化数据结构
/// 注意：只保存布局结构，不保存数据内容
/// </summary>
[Serializable]
public class WheelLayoutData<T>
{
    public int SlotCount;              // 槽位数量
    public int SelectedIndex;          // 选中索引
    public int[] SlotOrder;            // 槽位顺序（用于记录拖拽后的排列）

    // 注意：不包含T类型的数据内容
    // 数据内容由业务层负责管理
}
```

### 设计目的

- **可选功能**：不是所有轮盘都需要持久化
- **只保存布局**：不保存数据内容，避免数据同步问题
- **多种实现**：支持文件、PlayerPrefs、数据库等

### 实现指南

#### JSON文件持久化

```csharp
public class JsonWheelPersistence<T> : IWheelPersistence<T>
{
    private string _savePath;

    public JsonWheelPersistence(string savePath = "WheelLayouts")
    {
        _savePath = savePath;

        // 确保目录存在
        if (!Directory.Exists(_savePath))
        {
            Directory.CreateDirectory(_savePath);
        }
    }

    public void Save(string key, WheelLayoutData<T> data)
    {
        string filePath = GetFilePath(key);
        string json = JsonUtility.ToJson(data, prettyPrint: true);
        File.WriteAllText(filePath, json);

        Debug.Log($"轮盘布局已保存: {filePath}");
    }

    public WheelLayoutData<T> Load(string key)
    {
        string filePath = GetFilePath(key);

        if (!File.Exists(filePath))
            return null;

        string json = File.ReadAllText(filePath);
        return JsonUtility.FromJson<WheelLayoutData<T>>(json);
    }

    public bool Has(string key)
    {
        return File.Exists(GetFilePath(key));
    }

    public void Delete(string key)
    {
        string filePath = GetFilePath(key);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    private string GetFilePath(string key)
    {
        return Path.Combine(_savePath, $"{key}.json");
    }
}
```

#### PlayerPrefs持久化

```csharp
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
        string prefKey = $"Wheel_{key}";
        if (!PlayerPrefs.HasKey(prefKey))
            return null;

        string json = PlayerPrefs.GetString(prefKey);
        return JsonUtility.FromJson<WheelLayoutData<T>>(json);
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
```

#### 数据库持久化（示例）

```csharp
public class DatabaseWheelPersistence<T> : IWheelPersistence<T>
{
    private IDatabase _database;

    public DatabaseWheelPersistence(IDatabase database)
    {
        _database = database;
    }

    public void Save(string key, WheelLayoutData<T> data)
    {
        _database.Execute(
            "INSERT OR REPLACE INTO WheelLayouts (Key, Data) VALUES (@key, @data)",
            new { key, data = JsonUtility.ToJson(data) }
        );
    }

    public WheelLayoutData<T> Load(string key)
    {
        var json = _database.QuerySingle<string>(
            "SELECT Data FROM WheelLayouts WHERE Key = @key",
            new { key }
        );

        return json != null ? JsonUtility.FromJson<WheelLayoutData<T>>(json) : null;
    }

    public bool Has(string key)
    {
        var count = _database.QuerySingle<int>(
            "SELECT COUNT(*) FROM WheelLayouts WHERE Key = @key",
            new { key }
        );
        return count > 0;
    }

    public void Delete(string key)
    {
        _database.Execute("DELETE FROM WheelLayouts WHERE Key = @key", new { key });
    }
}
```

### 使用方式

```csharp
var wheel = new WheelBuilder<Item>()
    .WithAdapter(new ItemWheelAdapter())
    .WithConfig(config => {
        config.EnablePersistence = true;
        config.PersistenceKey = "MyItemWheel";  // 唯一键名
    })
    .WithPersistence(new JsonWheelPersistence<Item>())
    .Build();

// 启动时自动加载
// 拖拽交换时自动保存
```

### 注意事项

1. **唯一键名**：每个轮盘的PersistenceKey必须唯一
2. **只保存布局**：不要保存数据内容（如物品实例），只保存索引和顺序
3. **异常处理**：文件/数据库操作可能失败，需要妥善处理
4. **跨版本兼容**：考虑版本升级时的数据迁移

---

## IWheelInputHandler - 输入处理接口

### 接口定义

```csharp
/// <summary>
/// 轮盘输入处理接口（可选功能）
/// </summary>
public interface IWheelInputHandler
{
    /// <summary>
    /// 每帧更新（由轮盘调用）
    /// </summary>
    void OnUpdate();

    /// <summary>
    /// 位置变化事件（鼠标/摇杆位置）
    /// </summary>
    event Action<Vector2> OnPositionChanged;

    /// <summary>
    /// 确认选择事件
    /// </summary>
    event Action OnConfirm;

    /// <summary>
    /// 取消事件
    /// </summary>
    event Action OnCancel;
}
```

### 设计目的

- **可选功能**：支持不使用输入处理器，完全手动控制
- **多输入设备**：鼠标、手柄、触摸、VR等
- **自定义触发**：业务层可以自定义触发逻辑

### 实现指南

#### 鼠标输入（系统默认实现）

```csharp
public class MouseWheelInput : IWheelInputHandler
{
    public event Action<Vector2> OnPositionChanged;
    public event Action OnConfirm;
    public event Action OnCancel;

    private KeyCode _triggerKey;
    private bool _isPressed;

    public MouseWheelInput(KeyCode triggerKey = KeyCode.Alpha1)
    {
        _triggerKey = triggerKey;
    }

    public void OnUpdate()
    {
        // 按下触发键
        if (Input.GetKeyDown(_triggerKey))
        {
            _isPressed = true;
        }

        if (_isPressed)
        {
            // 持续发送鼠标位置
            OnPositionChanged?.Invoke(Input.mousePosition);

            // 松开确认
            if (Input.GetKeyUp(_triggerKey))
            {
                _isPressed = false;
                OnConfirm?.Invoke();
            }
        }

        // Esc取消
        if (Input.GetKeyDown(KeyCode.Escape) && _isPressed)
        {
            _isPressed = false;
            OnCancel?.Invoke();
        }
    }
}
```

#### 手柄输入

```csharp
public class GamepadWheelInput : IWheelInputHandler
{
    public event Action<Vector2> OnPositionChanged;
    public event Action OnConfirm;
    public event Action OnCancel;

    private bool _isActive;
    private Vector2 _screenCenter;

    public GamepadWheelInput()
    {
        _screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
    }

    public void OnUpdate()
    {
        // LB键激活轮盘
        if (Input.GetButtonDown("LeftBumper"))
        {
            _isActive = true;
        }

        if (_isActive)
        {
            // 右摇杆控制选择
            Vector2 rightStick = new Vector2(
                Input.GetAxis("RightStickX"),
                Input.GetAxis("RightStickY")
            );

            // 转换为屏幕坐标
            Vector2 screenPos = _screenCenter + rightStick * 100f;
            OnPositionChanged?.Invoke(screenPos);

            // A键确认
            if (Input.GetButtonDown("ButtonA"))
            {
                _isActive = false;
                OnConfirm?.Invoke();
            }

            // B键取消
            if (Input.GetButtonDown("ButtonB"))
            {
                _isActive = false;
                OnCancel?.Invoke();
            }
        }
    }
}
```

#### 触摸输入

```csharp
public class TouchWheelInput : IWheelInputHandler
{
    public event Action<Vector2> OnPositionChanged;
    public event Action OnConfirm;
    public event Action OnCancel;

    private bool _isTouching;
    private Vector2 _initialTouchPos;
    private float _longPressThreshold = 0.5f;
    private float _touchStartTime;

    public void OnUpdate()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    _initialTouchPos = touch.position;
                    _touchStartTime = Time.time;
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    // 长按激活轮盘
                    if (!_isTouching && Time.time - _touchStartTime > _longPressThreshold)
                    {
                        _isTouching = true;
                    }

                    if (_isTouching)
                    {
                        OnPositionChanged?.Invoke(touch.position);
                    }
                    break;

                case TouchPhase.Ended:
                    if (_isTouching)
                    {
                        _isTouching = false;
                        OnConfirm?.Invoke();
                    }
                    break;

                case TouchPhase.Canceled:
                    if (_isTouching)
                    {
                        _isTouching = false;
                        OnCancel?.Invoke();
                    }
                    break;
            }
        }
    }
}
```

### 使用方式

```csharp
// 方式1：使用默认鼠标输入
var wheel = new WheelBuilder<Item>()
    .WithAdapter(new ItemWheelAdapter())
    .WithInput(new MouseWheelInput(KeyCode.Alpha1))
    .Build();

// 方式2：不使用输入处理器，完全手动控制
var wheel = WheelBuilder.CreateSimple<Item>()
    .WithAdapter(new ItemWheelAdapter())
    .Build();  // 不添加输入处理器

// 手动控制
if (Input.GetKeyDown(KeyCode.Tab))
{
    wheel.Show(Input.mousePosition);
}

if (wheel.IsVisible)
{
    int hovered = CalculateHoveredIndex(Input.mousePosition);
    wheel.ManualSetHover(hovered);

    if (Input.GetKeyUp(KeyCode.Tab))
    {
        wheel.ManualConfirm();
    }
}
```

### 注意事项

1. **Update调用**：轮盘会在自己的Update中调用`OnUpdate()`
2. **事件频率**：`OnPositionChanged`是高频事件，避免重操作
3. **状态管理**：记得重置输入状态（如_isPressed）

---

## IWheelSelectionStrategy - 选择算法接口

### 接口定义

```csharp
/// <summary>
/// 轮盘选择策略接口
/// </summary>
public interface IWheelSelectionStrategy
{
    /// <summary>
    /// 根据输入位置计算选中的槽位索引
    /// </summary>
    /// <param name="wheelCenter">轮盘中心位置</param>
    /// <param name="inputPosition">输入位置（鼠标/摇杆）</param>
    /// <param name="slotCount">槽位数量</param>
    /// <param name="slotAngles">槽位角度数组</param>
    /// <returns>槽位索引，-1表示无选中（死区内）</returns>
    int GetSlotIndexFromPosition(
        Vector2 wheelCenter,
        Vector2 inputPosition,
        int slotCount,
        float[] slotAngles
    );

    /// <summary>
    /// 判断是否在死区内
    /// </summary>
    /// <param name="wheelCenter">轮盘中心位置</param>
    /// <param name="inputPosition">输入位置</param>
    /// <param name="deadZoneRadius">死区半径</param>
    /// <returns>true=在死区内，false=在死区外</returns>
    bool IsInDeadZone(
        Vector2 wheelCenter,
        Vector2 inputPosition,
        float deadZoneRadius
    );
}
```

### 设计目的

- **可替换算法**：支持不同的选择逻辑
- **适应不同布局**：圆形、扇形、不规则布局
- **优化体验**：针对不同场景优化选择体验

### 实现指南

#### 角度选择策略（系统默认）

```csharp
public class AngleSelectionStrategy : IWheelSelectionStrategy
{
    public int GetSlotIndexFromPosition(
        Vector2 wheelCenter, Vector2 inputPosition,
        int slotCount, float[] slotAngles)
    {
        // 计算方向向量
        Vector2 direction = inputPosition - wheelCenter;

        // 计算角度（-180到180）
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        // 计算每个槽位的角度范围
        float angleStep = 360f / slotCount;
        float halfStep = angleStep / 2f;

        // 找到最接近的槽位
        for (int i = 0; i < slotCount; i++)
        {
            float slotAngle = slotAngles != null ? slotAngles[i] : (i * angleStep);

            // 计算角度范围
            float lowerBound = (slotAngle - halfStep + 360f) % 360f;
            float upperBound = (slotAngle + halfStep) % 360f;

            // 处理跨越0度的情况
            if (lowerBound > upperBound)
            {
                if (angle >= lowerBound || angle <= upperBound)
                    return i;
            }
            else
            {
                if (angle >= lowerBound && angle <= upperBound)
                    return i;
            }
        }

        return -1;  // 理论上不会到达
    }

    public bool IsInDeadZone(Vector2 wheelCenter, Vector2 inputPosition, float deadZoneRadius)
    {
        return Vector2.Distance(wheelCenter, inputPosition) < deadZoneRadius;
    }
}
```

#### 距离选择策略

```csharp
public class DistanceSelectionStrategy : IWheelSelectionStrategy
{
    private float _slotRadius = 120f;

    public DistanceSelectionStrategy(float slotRadius = 120f)
    {
        _slotRadius = slotRadius;
    }

    public int GetSlotIndexFromPosition(
        Vector2 wheelCenter, Vector2 inputPosition,
        int slotCount, float[] slotAngles)
    {
        float minDistance = float.MaxValue;
        int closestIndex = -1;

        for (int i = 0; i < slotCount; i++)
        {
            // 计算槽位的屏幕位置
            float angle = slotAngles != null ? slotAngles[i] : (i * 360f / slotCount);
            Vector2 slotPos = GetSlotPosition(wheelCenter, angle, _slotRadius);

            // 计算距离
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

    private Vector2 GetSlotPosition(Vector2 center, float angle, float radius)
    {
        float rad = angle * Mathf.Deg2Rad;
        return new Vector2(
            center.x + Mathf.Cos(rad) * radius,
            center.y + Mathf.Sin(rad) * radius
        );
    }
}
```

### 使用方式

```csharp
// 使用默认角度策略
var wheel = new WheelBuilder<Item>()
    .WithAdapter(new ItemWheelAdapter())
    .Build();  // 默认使用AngleSelectionStrategy

// 使用自定义策略
var wheel = new WheelBuilder<Item>()
    .WithAdapter(new ItemWheelAdapter())
    .WithSelectionStrategy(new DistanceSelectionStrategy(radius: 150f))
    .Build();
```

---

## 接口实现示例

### 完整示例：语音轮盘

```csharp
// 1. 业务数据类
public class VoiceData
{
    public string VoiceID;
    public string DisplayName;
    public Sprite Icon;
    public AudioClip AudioClip;
}

// 2. 适配器
public class VoiceWheelAdapter : IWheelItemAdapter<VoiceData>
{
    public IWheelItem ToWheelItem(VoiceData voice)
    {
        if (voice == null) return null;

        return new WheelItemWrapper
        {
            Icon = voice.Icon,
            DisplayName = voice.DisplayName,
            IsValid = !string.IsNullOrEmpty(voice.VoiceID)
        };
    }

    public VoiceData FromWheelItem(IWheelItem item) => null;
}

// 3. 数据提供者
public class VoiceDataProvider : IWheelDataProvider<VoiceData>
{
    private List<VoiceData> _voices = new List<VoiceData>();

    public event Action<VoiceData> OnItemAdded;
    public event Action<VoiceData> OnItemRemoved;
    public event Action<VoiceData, VoiceData> OnItemChanged;

    public IEnumerable<VoiceData> GetAvailableItems() => _voices;

    public bool IsValid(VoiceData voice) =>
        voice != null && !string.IsNullOrEmpty(voice.VoiceID);

    public void AddVoice(VoiceData voice)
    {
        _voices.Add(voice);
        OnItemAdded?.Invoke(voice);
    }

    public void RemoveVoice(VoiceData voice)
    {
        _voices.Remove(voice);
        OnItemRemoved?.Invoke(voice);
    }
}

// 4. 使用
public class VoiceWheelManager : MonoBehaviour
{
    private Wheel<VoiceData> _wheel;
    private VoiceDataProvider _dataProvider;
    private AudioSource _audioSource;

    void Start()
    {
        // 创建数据提供者
        _dataProvider = new VoiceDataProvider();

        // 创建轮盘
        _wheel = new WheelBuilder<VoiceData>()
            .WithConfig(config => {
                config.SlotCount = 6;
                config.EnablePersistence = true;
                config.PersistenceKey = "VoiceWheel";
            })
            .WithAdapter(new VoiceWheelAdapter())
            .WithDataProvider(_dataProvider)
            .WithInput(new MouseWheelInput(KeyCode.V))
            .WithPersistence(new JsonWheelPersistence<VoiceData>())
            .OnItemSelected(PlayVoice)
            .Build();

        // 加载语音数据
        LoadVoices();
    }

    void LoadVoices()
    {
        // 从资源加载
        var voiceClips = Resources.LoadAll<AudioClip>("Voices");
        foreach (var clip in voiceClips)
        {
            _dataProvider.AddVoice(new VoiceData
            {
                VoiceID = clip.name,
                DisplayName = clip.name,
                Icon = GetVoiceIcon(clip.name),
                AudioClip = clip
            });
        }
    }

    void PlayVoice(int index, VoiceData voice)
    {
        _audioSource.clip = voice.AudioClip;
        _audioSource.Play();
        Debug.Log($"播放语音: {voice.DisplayName}");
    }

    void OnDestroy()
    {
        _wheel?.Dispose();
    }
}
```

---

## 最佳实践

### 1. 接口实现的性能优化

- ✅ 缓存重复的转换结果（适配器）
- ✅ 避免在高频事件（OnSlotHovered）中执行重操作
- ✅ 使用对象池减少GC压力

### 2. 异常处理

- ✅ 所有接口方法都应处理null参数
- ✅ 持久化操作要捕获IO异常
- ✅ 使用try-catch保护关键代码

### 3. 内存管理

- ✅ 及时取消事件订阅
- ✅ 实现IDisposable清理资源
- ✅ 避免循环引用

### 4. 测试友好

- ✅ 接口可以方便地进行Mock测试
- ✅ 提供测试用的Stub实现
- ✅ 保持接口简洁明确

---

**文档版本**：v2.0
**最后更新**：2025-01-05
**维护者**：QuickWheel团队

## IWheelView<T>

- ��װ���� UI ��ͼ�������ڣ�Attach/Detach����
- ���ղ�λ���¡�ѡ�С�Hover ���¼���ҵ����Զ������Ч����
- Ĭ��ʵ��λ�� QuickWheel.UI ģ�飨DefaultWheelView<T>����
