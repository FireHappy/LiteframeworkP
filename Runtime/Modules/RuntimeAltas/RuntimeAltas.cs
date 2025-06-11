using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace LiteFramework.Module
{
    public class RuntimeAtlas : IDisposable
    {
        private RenderTexture atlasRT;
        private Material blitMaterial;
        private Material conversionMaterial;
        private Material drawMaterial;
        private int atlasSize, padding;
        private int currentX = 0, currentY = 0, rowHeight = 0;
        private bool isDisposed = false;
        private bool useDirectBlit = true; // 标记是否可以直接blit

        public Texture Texture => atlasRT;
        public bool IsValid => atlasRT != null && atlasRT.IsCreated();
        public int UsedWidth => currentX;
        public int UsedHeight => currentY + rowHeight;
        public float Efficiency => (float)UsedHeight / atlasSize * 100f;

        public RuntimeAtlas(int size, Material blitMat, int pad = 1)
        {
            if (size <= 0 || pad < 0)
                throw new ArgumentException("Atlas size and padding must be positive values");

            atlasSize = size;
            padding = pad;
            blitMaterial = blitMat;

            // 检查是否可以直接blit
            useDirectBlit = SystemInfo.copyTextureSupport != CopyTextureSupport.None;

            Debug.Log($"Atlas initialized: {size}x{size}, padding: {padding}");

            atlasRT = new RenderTexture(size, size, 0, RenderTextureFormat.ARGB32)
            {
                name = "RuntimeAtlas",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            atlasRT.Create();

            // 创建用于格式转换的材质
            CreateConversionMaterial();
            CreateDrawMaterial();

            ResetPacking();
        }

        private void CreateConversionMaterial()
        {
            // 创建一个简单的copy shader，支持基本的格式转换
            Shader copyShader = Shader.Find("Hidden/TextureCopy");
            if (copyShader == null)
            {
                Debug.LogWarning("Could not find Hidden/TextureCopy shader, falling back to UI/Default");
                copyShader = Shader.Find("UI/Default");
            }

            conversionMaterial = new Material(copyShader);
        }

        private void CreateDrawMaterial()
        {
            // 创建一个用于绘制纹理的材质
            Shader shader = Shader.Find("Hidden/BlitCopy");
            if (shader == null)
            {
                Debug.LogWarning("Could not find Hidden/BlitCopy shader, falling back to UI/Default");
                shader = Shader.Find("UI/Default");
            }

            drawMaterial = new Material(shader);
            drawMaterial.hideFlags = HideFlags.HideAndDontSave;
        }

        /// <summary>
        /// 检查图集是否有足够空间容纳指定尺寸的纹理
        /// </summary>
        public bool HasSpace(int width, int height)
        {
            // 检查纹理尺寸是否超过图集大小
            if (width + 2 * padding > atlasSize || height + 2 * padding > atlasSize)
            {
                Debug.LogError($"Texture size {width}x{height} exceeds atlas capacity with padding");
                return false;
            }

            // 检查当前行是否有空间
            if (currentX + width + padding <= atlasSize)
                return true;

            // 检查新行是否有空间
            int newY = currentY + rowHeight + padding;
            return newY + height + padding <= atlasSize;
        }

        /// <summary>
        /// 向图集中添加纹理
        /// </summary>
        /// <exception cref="InvalidOperationException">当图集空间不足时抛出</exception>
        public AtlasResult AddTexture(Texture texture)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));

            if (isDisposed)
                throw new ObjectDisposedException(nameof(RuntimeAtlas));

            int w = texture.width;
            int h = texture.height;

            // 严格检查空间是否足够
            if (!HasSpace(w, h))
            {
                throw new InvalidOperationException(
                    $"Atlas is full! Cannot add {w}x{h} texture. " +
                    $"Current usage: {UsedWidth}x{UsedHeight}/{atlasSize}x{atlasSize} pixels ({Efficiency:F1}% efficient)"
                );
            }

            int xPos = currentX;
            int yPos = currentY;

            // 优化的纹理复制方法
            BlitTextureToPosition(texture, xPos, yPos);

            // 计算UV坐标（已修正Y轴翻转）
            Rect uv = new Rect(
                (float)xPos / atlasSize,                 // 左边界
                1f - (float)(yPos + h) / atlasSize,      // 下边界（翻转处理）
                (float)w / atlasSize,                    // UV宽度
                (float)h / atlasSize                     // UV高度
            );

            var result = new AtlasResult
            {
                texture = atlasRT,
                uv = uv,
                pixelRect = new Rect(xPos, yPos, w, h)
            };

            // 更新下一个纹理的位置
            currentX += w + padding;
            rowHeight = Mathf.Max(rowHeight, h);

            // 如果当前行放不下下一个纹理，则换行
            if (currentX + padding > atlasSize)
            {
                currentX = 0;
                currentY += rowHeight + padding;
                rowHeight = 0;
            }

            return result;
        }

        private void BlitTextureToPosition(Texture sourceTexture, int destX, int destY)
        {
            int w = sourceTexture.width;
            int h = sourceTexture.height;

            // 尝试直接复制纹理
            if (useDirectBlit && CanUseCopyTexture(sourceTexture))
            {
                try
                {
                    // 使用CopyTexture进行直接像素复制
                    Graphics.CopyTexture(sourceTexture, 0, 0, 0, 0, w, h, atlasRT, 0, 0, destX, destY);
                    return;
                }
                catch (Exception e)
                {
                    // 如果出错，回退到blit方法
                    Debug.LogWarning($"Failed to use CopyTexture: {e.Message}. Falling back to blit method.");
                    useDirectBlit = false;
                }
            }

            // 如果不能直接复制，使用blit方法
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture temp = null;

            try
            {
                // 检查是否需要格式转换
                bool needsConversion = NeedsFormatConversion(sourceTexture);

                if (blitMaterial != null || sourceTexture is RenderTexture || needsConversion)
                {
                    // 需要材质处理、源是RenderTexture或需要格式转换时使用临时RT
                    temp = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32);

                    // 设置临时RT的过滤模式与源纹理匹配
                    temp.filterMode = sourceTexture.filterMode;

                    // 第一步：将源纹理复制到临时RT
                    RenderTexture.active = temp;

                    Material materialToUse = blitMaterial;

                    // 如果需要格式转换且没有提供材质，使用默认转换材质
                    if (needsConversion && blitMaterial == null)
                    {
                        materialToUse = conversionMaterial;
                    }

                    if (materialToUse != null)
                    {
                        Graphics.Blit(sourceTexture, temp, materialToUse);
                    }
                    else
                    {
                        Graphics.Blit(sourceTexture, temp);
                    }

                    // 第二步：将临时RT的内容复制到图集的指定位置
                    RenderTexture.active = atlasRT;
                    DrawTextureAtPosition(temp, destX, destY, w, h);
                }
                else
                {
                    // 直接从Texture2D blit到RenderTexture的指定位置
                    RenderTexture.active = atlasRT;
                    DrawTextureAtPosition(sourceTexture, destX, destY, w, h);
                }
            }
            finally
            {
                RenderTexture.active = previousActive;
                if (temp != null)
                {
                    RenderTexture.ReleaseTemporary(temp);
                }
            }
        }

        private void DrawTextureAtPosition(Texture texture, int destX, int destY, int width, int height)
        {
            // 使用预创建的材质，避免重复创建
            drawMaterial.mainTexture = texture;

            // 开始绘制
            drawMaterial.SetPass(0);

            GL.PushMatrix();
            GL.LoadPixelMatrix(0, atlasSize, atlasSize, 0);

            // 绘制四边形（修正UV坐标顺序）
            GL.Begin(GL.QUADS);
            // 左下 (0,1)
            GL.TexCoord2(0, 1);
            GL.Vertex3(destX, destY, 0);
            // 右下 (1,1)
            GL.TexCoord2(1, 1);
            GL.Vertex3(destX + width, destY, 0);
            // 右上 (1,0)
            GL.TexCoord2(1, 0);
            GL.Vertex3(destX + width, destY + height, 0);
            // 左上 (0,0)
            GL.TexCoord2(0, 0);
            GL.Vertex3(destX, destY + height, 0);
            GL.End();

            GL.PopMatrix();
        }

        private bool CanUseCopyTexture(Texture sourceTexture)
        {
            // 检查是否可以使用Graphics.CopyTexture
            if (!(sourceTexture is Texture2D))
                return false;

            Texture2D sourceTex2D = sourceTexture as Texture2D;

            // 检查源纹理是否可读
            if (!sourceTex2D.isReadable)
                return false;

            // 检查格式兼容性
            TextureFormat sourceFormat = sourceTex2D.format;
            switch (sourceFormat)
            {
                case TextureFormat.RGBA32:
                case TextureFormat.ARGB32:
                case TextureFormat.RGB24:
                    return true;
                default:
                    // 其他格式可能不兼容，需要转换
                    return false;
            }
        }

        private bool NeedsFormatConversion(Texture sourceTexture)
        {
            if (!(sourceTexture is Texture2D))
                return true;

            Texture2D sourceTex2D = sourceTexture as Texture2D;
            TextureFormat sourceFormat = sourceTex2D.format;

            // 检查是否需要转换为ARGB32
            switch (sourceFormat)
            {
                case TextureFormat.ARGB32:
                    return false; // 已经是ARGB32
                case TextureFormat.RGBA32:
                case TextureFormat.RGB24:
                    return false; // 可以直接复制或简单转换
                default:
                    // 其他格式需要转换
                    return true;
            }
        }

        public void Clear()
        {
            if (atlasRT != null && atlasRT.IsCreated())
            {
                RenderTexture previousActive = RenderTexture.active;
                RenderTexture.active = atlasRT;
                GL.Clear(true, true, Color.clear);
                RenderTexture.active = previousActive;
            }

            ResetPacking();
        }

        private void ResetPacking()
        {
            currentX = 0;
            currentY = 0;
            rowHeight = 0;
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                if (atlasRT != null)
                {
                    atlasRT.Release();
                    UnityEngine.Object.DestroyImmediate(atlasRT);
                    atlasRT = null;
                }

                if (blitMaterial != null && blitMaterial.name.Contains("(Instance)"))
                {
                    UnityEngine.Object.DestroyImmediate(blitMaterial);
                    blitMaterial = null;
                }

                if (conversionMaterial != null)
                {
                    UnityEngine.Object.DestroyImmediate(conversionMaterial);
                    conversionMaterial = null;
                }

                if (drawMaterial != null)
                {
                    UnityEngine.Object.DestroyImmediate(drawMaterial);
                    drawMaterial = null;
                }

                isDisposed = true;
            }
        }

        ~RuntimeAtlas()
        {
            Dispose();
        }

        public void DebugLogStats()
        {
            Debug.Log($"Atlas Usage: {UsedWidth}x{UsedHeight}/{atlasSize}x{atlasSize} pixels ({Efficiency:F1}% efficient)");
        }
    }

    public struct AtlasResult
    {
        public Texture texture;
        public Rect uv;
        public Rect pixelRect;

        public Vector2 uvMin => new Vector2(uv.x, uv.y);
        public Vector2 uvMax => new Vector2(uv.x + uv.width, uv.y + uv.height);
    }
}