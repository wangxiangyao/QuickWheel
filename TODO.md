# QuickWheel TODO 项目

## 🎯 已完成的任务

### ✅ GridSelectionStrategy 角度计算修复
- [x] 修复 Unity 坐标系下的角度方向问题
- [x] 移除不必要的 Y 轴翻转
- [x] 更新注释与代码逻辑匹配
- [x] 删除误导性的未使用变量
- [x] 编译验证修复效果

### ✅ 架构分析
- [x] 深入分析 QuickWheel 事件流转机制
- [x] 发现 ItemWheel 调用不存在方法的问题
- [x] 识别 QuickWheel 设计缺陷（HandleInputPositionChanged 未实现）

## 🚧 进行中的任务

### 当前状态
- ItemWheel 编译错误（调用不存在的 UpdateInput 方法）
- GridSelectionStrategy 角度计算已修复但需要验证效果
- QuickWheel 架构问题需要系统性解决

## 📋 待完成的任务

### 🔧 高优先级 - QuickWheel 核心完善

#### 1. 实现 HandleInputPositionChanged 方法
**文件**: `src/Core/Wheel.cs`
**问题**: 当前方法为空实现，注释说"业务层会调用 ManualSetHover"
**解决方案**:
```csharp
private void HandleInputPositionChanged(Vector2 position)
{
    if (_stateManager.CurrentState != WheelState.Active) return;
    if (_selectionStrategy == null || _view == null) return;

    // 需要轮盘中心位置信息才能计算
    // 可能需要修改 InputHandler 接口设计
}
```

#### 2. 修复 ItemWheel 集成
**文件**: `ItemWheel/ItemWheelSystem.cs`
**当前问题**:
- 调用不存在的 `UpdateInput` 方法
- 没有正确使用 QuickWheel 的事件系统

**方案选择**:
- 方案A: 完善 QuickWheel 内部输入处理
- 方案B: 像 GridWheelExample 一样手动管理
- **建议**: 先用方案B快速修复，后续再优化方案A

#### 3. 添加缺失的事件处理方法
**需要实现**:
- 点击事件处理 (`OnSlotClicked`)
- 拖拽交换事件处理 (`OnSlotDragSwapped`)
- 完整的交互逻辑

### 🔍 中优先级 - 架构改进

#### 4. 重新设计 InputHandler 接口
**当前问题**:
- 只传递鼠标位置，缺少轮盘中心信息
- 无法计算相对位置和角度

**新接口设计**:
```csharp
public interface IWheelInputHandler
{
    void OnUpdate();
    event Action<Vector2> OnPositionChanged;        // 当前：只有鼠标位置
    event Action<Vector2, Vector2> OnRelativePositionChanged; // 建议：轮盘中心 + 鼠标位置
    event Action OnConfirm;
    event Action OnCancel;
    event Action<int> OnSlotClicked;              // 新增：点击事件
    event Action<int, int> OnSlotDragSwap;        // 新增：拖拽交换
}
```

#### 5. 提供默认输入处理器实现
**目标**: 开箱即用的体验
**需要实现**:
- `MouseWheelInput` (已存在，但可能需要调整)
- `TouchWheelInput` (移动端支持)
- `GamepadWheelInput` (手柄支持)

### 🧪 低优先级 - 测试和验证

#### 6. 完善测试用例
**需要添加**:
- GridSelectionStrategy 单元测试
- 角度计算边界测试
- 事件流集成测试

#### 7. 性能优化
**关注点**:
- 高频调用优化（如 OnSlotHovered）
- 内存使用优化
- 事件系统性能

## 🐛 已知问题

### 立即修复
1. **编译错误**: ItemWheel 调用不存在的 `UpdateInput` 方法
2. **方向问题**: 需要验证修复后的 GridSelectionStrategy 在实际游戏中是否正确

### 设计问题
1. **事件处理不完整**: 缺少点击、拖拽等交互支持
2. **接口设计局限**: InputHandler 无法提供足够信息给选择策略
3. **文档不一致**: 代码注释与实际行为不匹配

## 📝 技术决策记录

### 架构设计选择
1. **QuickWheel 设计理念**: 事件驱动 + 策略模式 + 状态管理
2. **当前实现问题**: 策略需要手动调用，违反了自动化期望
3. **改进方向**: 完善内部自动化，同时保留手动控制选项

### 坐标系决策
1. **Unity 坐标系**: 屏幕空间，Y轴向下为正
2. **角度计算**: 0°在右侧，Atan2 返回逆时针角度
3. **方向映射**: 上中=270°, 下中=90°, 左中=180°, 右中=0°

## 🎯 下一步行动计划

### 短期（明天）
1. **修复编译错误**: 让 ItemWheel 正常工作
2. **验证方向修复**: 确认 GridSelectionStrategy 在游戏中正确工作
3. **完善基础集成**: 确保 ItemWheel 使用正确的 QuickWheel 流程

### 中期（本周内）
1. **完善 QuickWheel 核心**: 实现缺失的事件处理
2. **改进输入处理**: 重新设计 InputHandler 接口
3. **添加默认实现**: 提供开箱即用的输入处理器

### 长期（未来版本）
1. **性能优化**: 高频调用和内存使用优化
2. **扩展支持**: 更多输入方式和设备
3. **文档完善**: 架构说明和最佳实践指南

---

**最后更新**: 2025-11-06
**状态**: GridSelectionStrategy 已修复，ItemWheel 集成待完善