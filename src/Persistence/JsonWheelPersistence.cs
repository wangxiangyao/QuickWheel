using System;
using System.IO;
using QuickWheel.Core.Interfaces;
using UnityEngine;

namespace QuickWheel.Persistence
{
    /// <summary>
    /// JSON文件持久化实现
    /// 将轮盘布局保存为JSON文件
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public class JsonWheelPersistence<T> : IWheelPersistence<T>
    {
        private string _savePath;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="savePath">保存路径（相对于Application.persistentDataPath）</param>
        public JsonWheelPersistence(string savePath = "WheelLayouts")
        {
            // 使用持久化数据路径
            _savePath = Path.Combine(Application.persistentDataPath, savePath);

            // 确保目录存在
            if (!Directory.Exists(_savePath))
            {
                Directory.CreateDirectory(_savePath);
                Debug.Log($"[JsonWheelPersistence] Created directory: {_savePath}");
            }
        }

        /// <summary>
        /// 保存轮盘状态
        /// </summary>
        public void Save(string key, WheelLayoutData<T> data)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("[JsonWheelPersistence] Key cannot be null or empty");
                return;
            }

            if (data == null)
            {
                Debug.LogError("[JsonWheelPersistence] Data cannot be null");
                return;
            }

            try
            {
                string filePath = GetFilePath(key);
                string json = JsonUtility.ToJson(data, prettyPrint: true);
                File.WriteAllText(filePath, json);

                Debug.Log($"[JsonWheelPersistence] Saved to: {filePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JsonWheelPersistence] Failed to save: {ex.Message}");
            }
        }

        /// <summary>
        /// 加载轮盘状态
        /// </summary>
        public WheelLayoutData<T> Load(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("[JsonWheelPersistence] Key cannot be null or empty");
                return null;
            }

            try
            {
                string filePath = GetFilePath(key);

                if (!File.Exists(filePath))
                {
                    Debug.Log($"[JsonWheelPersistence] File not found: {filePath}");
                    return null;
                }

                string json = File.ReadAllText(filePath);
                var data = JsonUtility.FromJson<WheelLayoutData<T>>(json);

                Debug.Log($"[JsonWheelPersistence] Loaded from: {filePath}");
                return data;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JsonWheelPersistence] Failed to load: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 检查是否存在保存数据
        /// </summary>
        public bool Has(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            string filePath = GetFilePath(key);
            return File.Exists(filePath);
        }

        /// <summary>
        /// 删除保存数据
        /// </summary>
        public void Delete(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("[JsonWheelPersistence] Key cannot be null or empty");
                return;
            }

            try
            {
                string filePath = GetFilePath(key);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Debug.Log($"[JsonWheelPersistence] Deleted: {filePath}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JsonWheelPersistence] Failed to delete: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取文件路径
        /// </summary>
        private string GetFilePath(string key)
        {
            // 移除非法字符
            string safeKey = key.Replace("\\", "_").Replace("/", "_").Replace(":", "_");
            return Path.Combine(_savePath, $"{safeKey}.json");
        }
    }
}
