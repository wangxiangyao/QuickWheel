using System.IO;
using UnityEngine;

namespace QuickWheel.Utils
{
    /// <summary>
    /// Sprite加载工具类
    /// 用于从文件系统加载PNG图片为Sprite（Mod专用）
    /// </summary>
    public static class SpriteLoader
    {
        /// <summary>
        /// 从文件加载PNG为Sprite
        /// </summary>
        /// <param name="filePath">PNG文件的完整路径</param>
        /// <param name="pixelsPerUnit">每单位像素数，默认100</param>
        /// <returns>加载的Sprite，失败返回null</returns>
        public static Sprite LoadFromFile(string filePath, float pixelsPerUnit = 100f)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"[SpriteLoader] File not found: {filePath}");
                return null;
            }

            try
            {
                // 读取PNG文件
                byte[] fileData = File.ReadAllBytes(filePath);

                // 创建Texture2D
                Texture2D texture = new Texture2D(2, 2);
                texture.filterMode = FilterMode.Bilinear;
                texture.wrapMode = TextureWrapMode.Clamp;

                // 加载PNG数据到Texture
                if (!texture.LoadImage(fileData))
                {
                    Debug.LogError($"[SpriteLoader] Failed to load image data: {filePath}");
                    return null;
                }

                // 创建Sprite
                Sprite sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),  // pivot点在中心
                    pixelsPerUnit
                );

                Debug.Log($"[SpriteLoader] Successfully loaded sprite: {filePath} ({texture.width}x{texture.height})");
                return sprite;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SpriteLoader] Exception loading sprite: {filePath}\n{e}");
                return null;
            }
        }
    }
}
