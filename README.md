# QuickWheel - 閫氱敤杞洏妗嗘灦

閫氱敤鐨?瀹牸杞洏绯荤粺妗嗘灦锛岀敤浜嶶nity Mod寮€鍙戙€?

## 鐗规€?

- **鍐呯疆9瀹牸UI** - 寮€绠卞嵆鐢紝鏃犻渶鎵嬪姩鍒涘缓UI
- **鍥哄畾9瀹牸甯冨眬** - 8涓柟鍚戞Ы浣?+ 1涓腑蹇冪┖浣?
- **娉涘瀷璁捐** - 鏀寔浠绘剰鏁版嵁绫诲瀷锛坄Wheel<T>`锛?
- **鏋佺畝API** - 3琛屼唬鐮佸垱寤哄畬鏁磋疆鐩?
- **鐭㈤噺閫夋嫨** - 鏍规嵁榧犳爣鏂瑰悜瑙掑害閫夋嫨鏍煎瓙
- **鍙€夋寔涔呭寲** - JSON鏂囦欢淇濆瓨甯冨眬
- **浜嬩欢椹卞姩** - 鐏垫椿鐨勪簨浠剁郴缁?

## 蹇€熷紑濮?

### 鏈€灏忕ず渚嬶紙3琛屼唬鐮?+ 鍐呯疆UI锛?

```csharp
// 1. 鍒涘缓杞洏锛堣嚜鍔ㄥ垱寤?瀹牸UI锛?
var wheel = new WheelBuilder<YourDataType>()
    .WithAdapter(new YourAdapter())
    .Build();

// 2. 娣诲姞鏁版嵁
wheel.SetSlot(0, data1);
wheel.SetSlot(1, data2);

// 3. 鏄剧ず/闅愯棌
wheel.Show();  // 鑷姩鏄剧ず鍦ㄩ紶鏍囦綅缃?
wheel.Hide();  // 鑷姩闅愯棌
```

### 瀹屾暣绀轰緥锛堝惈UI鏇存柊锛?

```csharp
using UnityEngine;
using QuickWheel.Core;
using QuickWheel.Utils;
using QuickWheel.Selection;

public class VoiceWheelExample : MonoBehaviour
{
    private Wheel<VoiceData> _wheel;
    private GridSelectionStrategy _selectionStrategy;

    void Start()
    {
        // 鍒涘缓杞洏锛堣嚜鍔ㄥ垱寤哄唴缃甎I锛?
        _wheel = new WheelBuilder<VoiceData>()
            .WithAdapter(new VoiceWheelAdapter())
            .WithSelectionStrategy(new GridSelectionStrategy())
            .OnItemSelected((index, voice) => Debug.Log($"閫変腑: {voice.DisplayName}"))
            .Build();

        _selectionStrategy = new GridSelectionStrategy();

        // 娣诲姞鏁版嵁鍒?涓Ы浣?
        _wheel.SetSlot(0, voiceData1);  // 宸︿腑
        _wheel.SetSlot(1, voiceData2);  // 鍙充腑
        // ... 鏇村妲戒綅
    }

    void Update()
    {
        // 1. 澶勭悊鏄剧ず/闅愯棌
        bool keyPressed = Input.GetKey(KeyCode.V);

        if (keyPressed && !_wheel.IsVisible)
            _wheel.Show();  // 鑷姩鏄剧ずUI
        else if (!keyPressed && _wheel.IsVisible)
            _wheel.Hide();  // 鑷姩闅愯棌UI

        // 2. 鏇存柊閫夋嫨锛堟牴鎹紶鏍囨柟鍚戯級
        if (_wheel.IsVisible)
        {
            var rect = _wheel.GetUIContainerRect();
            Vector2 wheelCenter = rect.position;
            Vector2 mousePos = Input.mousePosition;

            // 妫€鏌ユ鍖?
            if (!_selectionStrategy.IsInDeadZone(wheelCenter, mousePos, 20f))
            {
                int index = _selectionStrategy.GetSlotIndexFromPosition(
                    wheelCenter, mousePos, 9, null
                );
                _wheel.UpdateUISelection(index == 8 ? -1 : index);
            }
        }
    }

    void OnDestroy()
    {
        _wheel?.Dispose();
    }
}
```

### 閫傞厤鍣ㄧず渚?

```csharp
public class VoiceWheelAdapter : IWheelItemAdapter<VoiceData>
{
    public IWheelItem ToWheelItem(VoiceData voice)
    {
        if (voice == null) return null;

        return new WheelItemWrapper
        {
            Icon = voice.Icon,
            DisplayName = voice.DisplayName,
            IsValid = true
        };
    }

    public VoiceData FromWheelItem(IWheelItem item) => null;
}
```

## 9瀹牸甯冨眬

```
[7] [2] [6]    宸︿笂  涓婁腑  鍙充笂
[0] [ ] [1]    宸︿腑  涓績  鍙充腑
[4] [3] [5]    宸︿笅  涓嬩腑  鍙充笅
```

- **绱㈠紩 0-7**: 8涓彲鐢ㄦЫ浣?
- **绱㈠紩 8**: 涓績锛堜繚鐣欙紝涓嶄娇鐢級

## 鐩綍缁撴瀯

```
QuickWheel/
鈹溾攢鈹€ src/
鈹?  鈹溾攢鈹€ Core/                    # 鏍稿績灞?
鈹?  鈹?  鈹溾攢鈹€ Wheel.cs            # 涓荤被 Wheel<T>
鈹?  鈹?  鈹溾攢鈹€ WheelConfig.cs      # 閰嶇疆绫伙紙鍥哄畾9妲戒綅锛?
鈹?  鈹?  鈹溾攢鈹€ WheelEventBus.cs    # 浜嬩欢鎬荤嚎
鈹?  鈹?  鈹斺攢鈹€ States/             # 鐘舵€佺鐞?
鈹?  鈹溾攢鈹€ Interfaces/             # 鎺ュ彛瀹氫箟
鈹?  鈹?  鈹溾攢鈹€ IWheelItem.cs       # UI鏁版嵁鎺ュ彛
鈹?  鈹?  鈹溾攢鈹€ IWheelItemAdapter.cs # 閫傞厤鍣ㄦ帴鍙?
鈹?  鈹?  鈹溾攢鈹€ IWheelSelectionStrategy.cs # 閫夋嫨绛栫暐鎺ュ彛
鈹?  鈹?  鈹溾攢鈹€ IWheelPersistence.cs # 鎸佷箙鍖栨帴鍙?
鈹?  鈹?  鈹斺攢鈹€ IWheelInputHandler.cs # 杈撳叆澶勭悊鎺ュ彛
鈹?  鈹溾攢鈹€ Selection/              # 閫夋嫨绛栫暐
鈹?  鈹?  鈹斺攢鈹€ GridSelectionStrategy.cs # 9瀹牸閫夋嫨鍣?
鈹?  鈹溾攢鈹€ Input/                  # 杈撳叆澶勭悊
鈹?  鈹?  鈹斺攢鈹€ MouseWheelInput.cs  # 榧犳爣杈撳叆
鈹?  鈹溾攢鈹€ Persistence/            # 鎸佷箙鍖?
鈹?  鈹?  鈹斺攢鈹€ JsonWheelPersistence.cs # JSON鎸佷箙鍖?
鈹?  鈹斺攢鈹€ Utils/                  # 宸ュ叿绫?
鈹?      鈹溾攢鈹€ WheelBuilder.cs     # Builder妯″紡
鈹?      鈹斺攢鈹€ WheelItemWrapper.cs # 榛樿IWheelItem瀹炵幇
鈹溾攢鈹€ Examples/                   # 绀轰緥浠ｇ爜
鈹?  鈹溾攢鈹€ VoiceData.cs           # 绀轰緥鏁版嵁绫?
鈹?  鈹溾攢鈹€ VoiceWheelAdapter.cs   # 绀轰緥閫傞厤鍣?
鈹?  鈹斺攢鈹€ GridExample/           # 瀹屾暣9瀹牸绀轰緥
鈹?      鈹溾攢鈹€ GridWheelExample.cs # 涓荤ず渚?
鈹?      鈹溾攢鈹€ GridWheelDisplay.cs # UI鏄剧ず缁勪欢
鈹?      鈹斺攢鈹€ GridWheelSlot.cs   # 妲戒綅鏁版嵁缁撴瀯
鈹斺攢鈹€ Documentation/              # 璇︾粏鏂囨。
    鈹溾攢鈹€ Architecture.md         # 鏋舵瀯璁捐
    鈹溾攢鈹€ API.md                 # API鎵嬪唽
    鈹斺攢鈹€ Interfaces.md          # 鎺ュ彛璇存槑
```

## 鏍稿績姒傚康

### 1. 娉涘瀷杞洏 `Wheel<T>`

鏀寔浠绘剰鏁版嵁绫诲瀷鐨勮疆鐩樼郴缁燂細

```csharp
Wheel<VoiceData> voiceWheel;
Wheel<Item> itemWheel;
Wheel<Skill> skillWheel;
```

### 2. 閫傞厤鍣ㄦā寮?

閫氳繃 `IWheelItemAdapter<T>` 灏嗕笟鍔℃暟鎹浆鎹负UI鏁版嵁锛?

```csharp
public interface IWheelItemAdapter<T>
{
    IWheelItem ToWheelItem(T data);
    T FromWheelItem(IWheelItem item);
}
```

### 3. 9瀹牸閫夋嫨绛栫暐

`GridSelectionStrategy` 鏍规嵁榧犳爣鐩稿杞洏涓績鐨勮搴﹂€夋嫨8涓柟鍚戯細

- 鍙充腑: 337.5掳 - 22.5掳
- 鍙充笅: 22.5掳 - 67.5掳
- 涓嬩腑: 67.5掳 - 112.5掳
- 宸︿笅: 112.5掳 - 157.5掳
- 宸︿腑: 157.5掳 - 202.5掳
- 宸︿笂: 202.5掳 - 247.5掳
- 涓婁腑: 247.5掳 - 292.5掳
- 鍙充笂: 292.5掳 - 337.5掳

### 4. 浜嬩欢绯荤粺

```csharp
wheel.OnItemSelected += (index, data) => { /* 閫変腑浜嬩欢 */ };
wheel.OnWheelShown += () => { /* 鏄剧ず浜嬩欢 */ };
wheel.OnWheelHidden += (finalIndex) => { /* 闅愯棌浜嬩欢 */ };
```

### 5. 鍙€夋寔涔呭寲

```csharp
.WithConfig(config =>
{
    config.EnablePersistence = true;
    config.PersistenceKey = "MyWheel";
})
.WithPersistence(new JsonWheelPersistence<T>())
```

## UI瀹炵幇

鏈鏋?*鍐呯疆9瀹牸UI**锛屾棤闇€鎵嬪姩鍒涘缓锛?

### 浣跨敤鍐呯疆UI锛堥粯璁わ級

```csharp
// 鍒涘缓杞洏鏃惰嚜鍔ㄥ垱寤篣I
var wheel = new WheelBuilder<T>()
    .WithAdapter(adapter)
    .Build();  // 鉁?鑷姩鍒涘缓9瀹牸UI

// 鏄剧ず/闅愯棌UI
wheel.Show();  // 鑷姩鏄剧ず鍦ㄩ紶鏍囦綅缃?
wheel.Hide();  // 鑷姩闅愯棌

// 鏇存柊UI閫変腑鐘舵€?
wheel.UpdateUISelection(index);
```

### 鑷畾涔夋垨鍚敤榛樿 UI

QuickWheel 鏍稿績榛樿涓嶉檮甯?UI锛宍QuickWheel.UI` 妯″潡鎻愪緵浜嗕竴涓?9 瀹牸鐨勯粯璁ゅ疄鐜帮紝鍙寜闇€閫夋嫨锛?
```csharp
// 浣跨敤榛樿 9 瀹牸 UI锛堥渶寮曞叆 QuickWheel.UI锛?var wheel = new WheelBuilder<T>()
    .WithAdapter(adapter)
    .UseDefaultView()   // 鉁?缁戝畾榛樿 UI 瑙嗗浘
    .Build();

// 瀹屽叏鑷畾涔?UI锛氫笉璋冪敤 UseDefaultView锛岃嚜琛屾彁渚?IWheelView<T>
var customWheel = new WheelBuilder<T>()
    .WithAdapter(adapter)
    .Build();           // 榛樿娌℃湁瑙嗗浘

customWheel.SetView(new MyWheelView()); // 瀹炵幇 IWheelView<T>
```

`QuickWheel.UI` 妯″潡鍖呭惈锛?- `DefaultWheelView<T>`锛氶粯璁?9 瀹牸瑙嗗浘
- `WheelUIManager<T>`锛氳礋璐ｇ鐞嗛粯璁よ鍥剧殑甯冨眬鍜岃緭鍏?- `WheelSlotDisplay`锛氬崟鏍兼樉绀虹粍浠讹紙鍥炬爣銆佹枃瀛椼€佹偓鍋滃姩鐢伙級

## 閰嶇疆閫夐」

```csharp
public class WheelConfig
{
    // 鍥哄畾9妲戒綅
    public const int SLOT_COUNT = 9;

    // 甯冨眬閰嶇疆
    public float GridCellSize = 40f;     // 鏍煎瓙澶у皬
    public float GridSpacing = 5f;       // 鏍煎瓙闂磋窛

    // 浜や簰閰嶇疆
    public bool EnableDragSwap = true;   // 鍚敤鎷栨嫿浜ゆ崲
    public bool EnableClickSelect = true; // 鍚敤鐐瑰嚮閫変腑
    public float DeadZoneRadius = 20f;   // 姝诲尯鍗婂緞

    // 瑙嗚閰嶇疆
    public float HoverScaleMultiplier = 1.15f; // Hover缂╂斁
    public float AnimationDuration = 0.1f;     // 鍔ㄧ敾鏃堕暱

    // 鎸佷箙鍖栭厤缃?
    public bool EnablePersistence = false;
    public string PersistenceKey = "";
}
```

## API鍙傝€?

### Wheel<T> 涓昏鏂规硶

```csharp
// 妲戒綅鎿嶄綔
T GetSlot(int index)
void SetSlot(int index, T item)
bool SwapSlots(int index1, int index2)

// 鏄剧ず鎺у埗
void Show()  // 鑷姩鏄剧ず鍐呯疆UI鍦ㄩ紶鏍囦綅缃?
void Hide(bool executeSelection = true)  // 鑷姩闅愯棌鍐呯疆UI
bool IsVisible { get; }

// UI鎺у埗锛堝唴缃甎I锛?
void EnableUI(Transform parent = null)  // 鎵嬪姩鍚敤UI
void UpdateUISelection(int selectedIndex)  // 鏇存柊UI閫変腑鐘舵€?
RectTransform GetUIContainerRect()  // 鑾峰彇UI瀹瑰櫒Rect

// 浜嬩欢璁㈤槄
EventBus.OnSlotDataChanged += (index, item) => { }
EventBus.OnSlotsSwapped += (i1, i2) => { }

// 鍙€夌粍浠?
void SetSelectionStrategy(IWheelSelectionStrategy strategy)
void SetPersistence(IWheelPersistence<T> persistence)
void SetInputHandler(IWheelInputHandler handler)
void SetView(IWheelView<T> view)`r`nvoid SetView(IWheelView<T> view)
```

### WheelBuilder<T> 娴佸紡API

```csharp
new WheelBuilder<T>()\r\n    .WithConfig(config => { /* 配置 */ })\r\n    .WithAdapter(adapter)  // ✅ 必需\r\n    .WithSelectionStrategy(strategy)  // 可选\r\n    .WithPersistence(persistence)  // 可选\r\n    .WithInput(inputHandler)  // 可选\r\n    .UseDefaultView()  // ✅ 使用默认 9 宫格视图（需引用 QuickWheel.UI）\r\n    .OnItemSelected((i, data) => { })\r\n    .OnWheelShown(() => { })\r\n    .OnWheelHidden((i) => { })\r\n    .Build();  // 鉁?鑷姩鍒涘缓9瀹牸UI
```

## 鏈€浣冲疄璺?

1. **浣跨敤鍐呯疆UI** - 榛樿鍚敤鍐呯疆UI锛屽紑绠卞嵆鐢?
2. **鍥哄畾9妲戒綅** - 8涓柟鍚戞Ы浣?+ 1涓腑蹇冪┖浣?
3. **閫傞厤鍣ㄥ垎绂?* - 涓氬姟鏁版嵁鍜孶I鏁版嵁閫氳繃閫傞厤鍣ㄨВ鑰?
4. **浜嬩欢椹卞姩** - 浣跨敤浜嬩欢鑰岄潪杞鎻愰珮鎬ц兘
5. **绠€鍗曠ず渚?* - 鍙傝€?`Examples/SimpleExample.cs` 蹇€熶笂鎵?
6. **鍙€夌粍浠?* - 鎸夐渶娣诲姞鎸佷箙鍖栥€佽緭鍏ュ鐞嗙瓑鍔熻兘

## 鎵╁睍鎸囧崡

### 鑷畾涔夐€夋嫨绛栫暐

```csharp
public class MySelectionStrategy : IWheelSelectionStrategy
{
    public int GetSlotIndexFromPosition(
        Vector2 wheelCenter,
        Vector2 inputPosition,
        int slotCount,
        float[] slotAngles)
    {
        // 鑷畾涔夐€夋嫨閫昏緫
        return selectedIndex;
    }

    public bool IsInDeadZone(
        Vector2 wheelCenter,
        Vector2 inputPosition,
        float deadZoneRadius)
    {
        return Vector2.Distance(wheelCenter, inputPosition) < deadZoneRadius;
    }
}
```

### 鑷畾涔夋寔涔呭寲

```csharp
public class MyPersistence<T> : IWheelPersistence<T>
{
    public void Save(string key, WheelLayoutData<T> data) { }
    public WheelLayoutData<T> Load(string key) { }
    public bool Has(string key) { }
    public void Delete(string key) { }
}
```

## 璁稿彲璇?

MIT License

## 鑱旂郴鏂瑰紡

GitHub Issues: [鎶ュ憡闂](https://github.com/your-repo/issues)















