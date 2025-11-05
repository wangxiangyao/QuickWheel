using UnityEngine;
using QuickWheel;

namespace QuickWheel.Examples
{
    /// <summary>
    /// 简单使用示例 - 展示其他mod如何使用QuickWheel
    /// </summary>
    public class SimpleUsageExample : MonoBehaviour
    {
        void Start()
        {
            // === 创建物品轮盘 ===
            CreateItemWheel();

            // === 创建语音轮盘 ===
            CreateVoiceWheel();
        }

        /// <summary>
        /// 创建物品轮盘 - 展示完整的链式配置
        /// </summary>
        private void CreateItemWheel()
        {
            var itemWheel = QuickWheel.Create<Item>()
                .WithConfig(config => {
                    config.SlotCount = 8;
                    config.SlotRadius = 120f;
                    config.EnableDragSwap = true;
                    config.EnablePersistence = true;
                    config.PersistenceKey = "ItemWheel";
                })
                .WithAdapter(new ItemWheelAdapter())
                .WithInput(new MouseWheelInput(KeyCode.Q))
                .WithPersistence(new JsonWheelPersistence<Item>())
                .OnItemSelected((index, item) => Debug.Log($"使用物品: {item?.Name}"))
                .OnWheelShown(() => Debug.Log("物品轮盘显示"))
                .OnWheelHidden((index) => Debug.Log($"物品轮盘隐藏，选择了: {index}"))
                .Build();

            // 设置物品数据
            itemWheel.SetSlots(
                new Item { Name = "生命药水", Icon = null },
                new Item { Name = "魔法药水", Icon = null },
                new Item { Name = "面包", Icon = null }
            );

            Debug.Log("物品轮盘创建完成，按Q键使用");
        }

        /// <summary>
        /// 创建语音轮盘 - 展示简化配置
        /// </summary>
        private void CreateVoiceWheel()
        {
            var voiceWheel = QuickWheel.Create<VoiceData>()
                .WithAdapter(new VoiceWheelAdapter())
                .WithInput(new MouseWheelInput(KeyCode.V))
                .OnItemSelected((index, voice) => PlayVoice(voice))
                .Build();

            // 设置语音数据
            voiceWheel.SetSlots(
                new VoiceData { DisplayName = "你好", VoiceID = "hello" },
                new VoiceData { DisplayName = "谢谢", VoiceID = "thanks" },
                new VoiceData { DisplayName = "抱歉", VoiceID = "sorry" }
            );

            Debug.Log("语音轮盘创建完成，按V键使用");
        }

        void Update()
        {
            // 这里应该轮盘实例的Update调用
            // itemWheel.Update();
            // voiceWheel.Update();
        }

        void PlayVoice(VoiceData voice)
        {
            if (voice != null)
            {
                Debug.Log($"播放语音: {voice.DisplayName}");
                // 实际播放语音的逻辑
            }
        }

        void UseItem(Item item)
        {
            if (item != null)
            {
                Debug.Log($"使用物品: {item.Name}");
                // 实际使用物品的逻辑
            }
        }
    }

    #region 示例数据类和适配器

    public class Item
    {
        public string Name { get; set; }
        public Sprite Icon { get; set; }
    }

    public class VoiceData
    {
        public string DisplayName { get; set; }
        public string VoiceID { get; set; }
    }

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
            return null;
        }
    }

    public class VoiceWheelAdapter : IWheelItemAdapter<VoiceData>
    {
        public IWheelItem ToWheelItem(VoiceData voice)
        {
            if (voice == null || string.IsNullOrEmpty(voice.VoiceID))
                return null;

            return new WheelItemWrapper
            {
                Icon = null,
                DisplayName = voice.DisplayName,
                IsValid = true
            };
        }

        public VoiceData FromWheelItem(IWheelItem item)
        {
            return null;
        }
    }

    #endregion
}