using QuickWheel;
using UnityEngine;

namespace OtherModExample
{
    /// <summary>
    /// 演示其他mod如何使用QuickWheel
    /// 这就是一个典型的其他mod项目代码
    /// </summary>
    public class OtherModExample : MonoBehaviour
    {
        [Header("轮盘配置")]
        [SerializeField] private KeyCode _itemWheelKey = KeyCode.Q;
        [SerializeField] private KeyCode _voiceWheelKey = KeyCode.V;
        [SerializeField] private KeyCode _skillWheelKey = KeyCode.F;

        // 轮盘实例
        private Wheel<Item> _itemWheel;
        private Wheel<VoiceData> _voiceWheel;
        private Wheel<Skill> _skillWheel;

        void Start()
        {
            Debug.Log("[OtherModExample] 初始化QuickWheel轮盘系统");

            // 创建三个不同用途的轮盘
            CreateItemWheel();
            CreateVoiceWheel();
            CreateSkillWheel();
        }

        /// <summary>
        /// 创建物品轮盘
        /// </summary>
        private void CreateItemWheel()
        {
            _itemWheel = QuickWheel.Create<Item>()
                .WithConfig(config => {
                    config.SlotCount = 8;
                    config.SlotRadius = 120f;
                    config.EnableDragSwap = true;
                    config.EnablePersistence = true;
                    config.PersistenceKey = "OtherMod_ItemWheel";
                })
                .WithAdapter(new ItemWheelAdapter())
                .WithMouseInput(_itemWheelKey)
                .WithPersistence("OtherMod_ItemWheel")
                .OnItemSelected(UseItem)
                .OnWheelShown(() => Debug.Log("[物品轮盘] 显示"))
                .OnWheelHidden((index) => Debug.Log($"[物品轮盘] 隐藏，选择了索引: {index}"))
                .Build();

            // 设置物品数据（这里用示例数据）
            _itemWheel.SetSlots(
                new Item { Name = "生命药水", Icon = null },
                new Item { Name = "魔法药水", Icon = null },
                new Item { Name = "面包", Icon = null },
                new Item { Name = "水", Icon = null }
            );

            Debug.Log("[OtherModExample] 物品轮盘创建完成，按 " + _itemWheelKey + " 使用");
        }

        /// <summary>
        /// 创建语音轮盘
        /// </summary>
        private void CreateVoiceWheel()
        {
            _voiceWheel = QuickWheel.Create<VoiceData>()
                .WithConfig(config => {
                    config.SlotCount = 6;
                    config.SlotRadius = 100f;
                    config.EnableDragSwap = false;
                    config.EnablePersistence = true;
                    config.PersistenceKey = "OtherMod_VoiceWheel";
                })
                .WithAdapter(new VoiceWheelAdapter())
                .WithMouseInput(_voiceWheelKey)
                .WithPersistence("OtherMod_VoiceWheel")
                .OnItemSelected(PlayVoice)
                .Build();

            // 设置语音数据
            _voiceWheel.SetSlots(
                new VoiceData { DisplayName = "你好", VoiceID = "hello" },
                new VoiceData { DisplayName = "谢谢", VoiceID = "thanks" },
                new VoiceData { DisplayName = "求助", VoiceID = "help" },
                new VoiceData { DisplayName = "抱歉", VoiceID = "sorry" }
            );

            Debug.Log("[OtherModExample] 语音轮盘创建完成，按 " + _voiceWheelKey + " 使用");
        }

        /// <summary>
        /// 创建技能轮盘
        /// </summary>
        private void CreateSkillWheel()
        {
            _skillWheel = QuickWheel.Create<Skill>()
                .WithConfig(config => {
                    config.SlotCount = 4;
                    config.SlotRadius = 90f;
                    config.EnableDragSwap = true;
                    config.EnablePersistence = false; // 技能轮盘不持久化
                })
                .WithAdapter(new SkillWheelAdapter())
                .WithMouseInput(_skillWheelKey)
                .OnSelected(UseSkill)
                .Build();

            // 设置技能数据
            _skillWheel.SetSlots(
                new Skill { Name = "火球术", Icon = null, Cooldown = 3f },
                new Skill { Name = "冰霜护盾", Icon = null, Cooldown = 5f },
                new Skill { Name = "治疗术", Icon = null, Cooldown = 4f },
                new Skill { Name = "闪电链", Icon = null, Cooldown = 6f }
            );

            Debug.Log("[OtherModExample] 技能轮盘创建完成，按 " + _skillWheelKey + " 使用");
        }

        void Update()
        {
            // 更新所有轮盘
            _itemWheel?.Update();
            _voiceWheel?.Update();
            _skillWheel?.Update();
        }

        void OnDestroy()
        {
            // 清理资源
            _itemWheel?.Dispose();
            _voiceWheel?.Dispose();
            _skillWheel?.Dispose();
            Debug.Log("[OtherModExample] 轮盘资源已清理");
        }

        #region 事件处理

        /// <summary>
        /// 使用物品
        /// </summary>
        private void UseItem(int index, Item item)
        {
            if (item == null) return;

            Debug.Log($"[OtherModExample] 使用物品: {item.Name}");

            // 这里是你的物品使用逻辑
            switch (item.Name)
            {
                case "生命药水":
                    // 恢复生命值逻辑
                    break;
                case "魔法药水":
                    // 恢复魔法值逻辑
                    break;
                case "面包":
                case "水":
                    // 消耗品逻辑
                    break;
            }
        }

        /// <summary>
        /// 播放语音
        /// </summary>
        private void PlayVoice(int index, VoiceData voice)
        {
            if (voice == null) return;

            Debug.Log($"[OtherModExample] 播放语音: {voice.DisplayName} (ID: {voice.VoiceID})");

            // 这里是你的语音播放逻辑
            // 例如调用游戏内的语音系统
        }

        /// <summary>
        /// 使用技能
        /// </summary>
        private void UseSkill(int index, Skill skill)
        {
            if (skill == null) return;

            Debug.Log($"[OtherModExample] 使用技能: {skill.Name} (冷却: {skill.Cooldown}秒)");

            // 这里是你的技能使用逻辑
            // 检查冷却、消耗法力、执行技能效果等
        }

        #endregion

        #region 测试方法

        /// <summary>
        /// 测试方法：动态添加物品到轮盘
        /// </summary>
        [ContextMenu("测试添加物品")]
        private void TestAddItem()
        {
            var newItem = new Item { Name = "测试物品", Icon = null };

            // 找到空槽位添加
            for (int i = 0; i < 8; i++)
            {
                if (_itemWheel.GetSlot(i) == null)
                {
                    _itemWheel.SetSlot(i, newItem);
                    Debug.Log($"[测试] 添加物品到槽位 {i}: {newItem.Name}");
                    return;
                }
            }

            Debug.Log("[测试] 没有空槽位可用");
        }

        /// <summary>
        /// 测试方法：清空所有轮盘
        /// </summary>
        [ContextMenu("测试清空轮盘")]
        private void TestClearWheels()
        {
            _itemWheel.ClearSlots();
            _voiceWheel.ClearSlots();
            _skillWheel.ClearSlots();
            Debug.Log("[测试] 已清空所有轮盘");
        }

        #endregion
    }

    #region 示例数据类

    /// <summary>
    /// 物品类
    /// </summary>
    public class Item
    {
        public string Name { get; set; }
        public Sprite Icon { get; set; }
    }

    /// <summary>
    /// 语音数据类
    /// </summary>
    public class VoiceData
    {
        public string DisplayName { get; set; }
        public string VoiceID { get; set; }
    }

    /// <summary>
    /// 技能类
    /// </summary>
    public class Skill
    {
        public string Name { get; set; }
        public Sprite Icon { get; set; }
        public float Cooldown { get; set; }
    }

    #endregion

    #region 示例适配器

    /// <summary>
    /// 物品适配器
    /// </summary>
    public class ItemWheelAdapter : IWheelItemAdapter<Item>
    {
        public IWheelItem ToWheelItem(Item item)
        {
            if (item == null) return null;

            return new WheelItemWrapper
            {
                Icon = item.Icon,
                DisplayName = item.Name,
                IsValid = true
            };
        }

        public Item FromWheelItem(IWheelItem item)
        {
            return null; // 通常不需要反向转换
        }
    }

    /// <summary>
    /// 语音适配器
    /// </summary>
    public class VoiceWheelAdapter : IWheelItemAdapter<VoiceData>
    {
        public IWheelItem ToWheelItem(VoiceData voice)
        {
            if (voice == null || string.IsNullOrEmpty(voice.VoiceID))
                return null;

            return new WheelItemWrapper
            {
                Icon = null, // 语音可以用默认图标
                DisplayName = voice.DisplayName,
                IsValid = true
            };
        }

        public VoiceData FromWheelItem(IWheelItem item)
        {
            return null;
        }
    }

    /// <summary>
    /// 技能适配器
    /// </summary>
    public class SkillWheelAdapter : IWheelItemAdapter<Skill>
    {
        public IWheelItem ToWheelItem(Skill skill)
        {
            if (skill == null) return null;

            return new WheelItemWrapper
            {
                Icon = skill.Icon,
                DisplayName = $"{skill.Name} ({skill.Cooldown}s)",
                IsValid = true
            };
        }

        public Skill FromWheelItem(IWheelItem item)
        {
            return null;
        }
    }

    #endregion
}