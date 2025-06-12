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

            useDirectBlit = SystemInfo.copyTextureSupport != CopyTextureSupport.None;

            Debug.Log($"Atlas initialized: {size}x{size}, padding: {padding}");

            atlasRT = new RenderTexture(size, size, 0, RenderTextureFormat.ARGB32)
            {
                name = "RuntimeAtlas",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            atlasRT.Create();

            CreateConversionMaterial();
            CreateDrawMaterial();

            ResetPacking();
        }

        private void CreateConversionMaterial()
        {
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
            // 创建专门用于透明纹理绘制的材质
            Shader drawShader = Shader.Find("Custom/DrawWithoutBlend");
            if (drawShader == null)
            {
                Debug.LogWarning("Could not find Custom/DrawWithoutBlend shader, falling back to UI/Default");
                drawShader = Shader.Find("UI/Default");
            }

            drawMaterial = new Material(drawShader);
            drawMaterial.hideFlags = HideFlags.HideAndDontSave;

            // 确保材质设置为不混合模式，直接覆盖像素
            drawMaterial.SetInt("_SrcBlend", (int)BlendMode.One);
            drawMaterial.SetInt("_DstBlend", (int)BlendMode.Zero);
            drawMaterial.SetInt("_ZWrite", 1);
        }

        public bool HasSpace(int width, int height)
        {
            if (width + 2 * padding > atlasSize || height + 2 * padding > atlasSize)
            {
                Debug.LogWarning($"Texture size {width}x{height} exceeds atlas capacity with padding");
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

            if (!HasSpace(w, h))
            {
                throw new InvalidOperationException(
                    $"Atlas is full! Cannot add {w}x{h} texture. " +
                    $"Current usage: {UsedWidth}x{UsedHeight}/{atlasSize}x{atlasSize} pixels ({Efficiency:F1}% efficient)"
                );
            }

            if (currentX + padding + w > atlasSize)
            {
                currentX = 0;
                currentY += rowHeight + padding;
                rowHeight = 0;
            }

            int xPos = currentX;
            int yPos = currentY;

            // 使用专门的透明纹理处理方法
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

            currentX += w + padding;
            rowHeight = Mathf.Max(rowHeight, h);

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

            // 对于透明纹理，避免使用CopyTexture，因为它可能不正确处理透明度
            bool isTransparent = HasTransparency(sourceTexture);

            if (useDirectBlit && !isTransparent && CanUseCopyTexture(sourceTexture))
            {
                try
                {
                    Graphics.CopyTexture(sourceTexture, 0, 0, 0, 0, w, h, atlasRT, 0, 0, destX, destY);
                    return;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to use CopyTexture: {e.Message}. Falling back to blit method.");
                    useDirectBlit = false;
                }
            }

            // 使用专门的透明纹理绘制方法
            BlitWithProperBlending(sourceTexture, destX, destY, w, h);
        }

        private void BlitWithProperBlending(Texture sourceTexture, int destX, int destY, int width, int height)
        {
            RenderTexture previousActive = RenderTexture.active;

            try
            {
                RenderTexture.active = atlasRT;

                // 设置正确的渲染状态
                GL.PushMatrix();
                GL.LoadPixelMatrix(0, atlasSize, atlasSize, 0);

                // 禁用混合，直接覆盖像素
                GL.Begin(GL.QUADS);

                // 设置材质属性
                drawMaterial.mainTexture = sourceTexture;
                drawMaterial.SetPass(0);

                // 绘制四边形，确保UV坐标正确
                GL.TexCoord2(0, 0); GL.Vertex3(destX, destY + height, 0);         // 左上
                GL.TexCoord2(1, 0); GL.Vertex3(destX + width, destY + height, 0); // 右上
                GL.TexCoord2(1, 1); GL.Vertex3(destX + width, destY, 0);          // 右下
                GL.TexCoord2(0, 1); GL.Vertex3(destX, destY, 0);                  // 左下

                GL.End();
                GL.PopMatrix();
            }
            finally
            {
                RenderTexture.active = previousActive;
            }
        }

        private bool HasTransparency(Texture texture)
        {
            // 检查纹理是否包含透明度信息
            if (texture is Texture2D tex2D)
            {
                TextureFormat format = tex2D.format;
                switch (format)
                {
                    case TextureFormat.RGBA32:
                    case TextureFormat.ARGB32:
                    case TextureFormat.RGBA4444:
                    case TextureFormat.ARGB4444:
                    case TextureFormat.Alpha8:
                    case TextureFormat.DXT5:
                    case TextureFormat.BC7:
                        return true;
                    default:
                        return false;
                }
            }

            // RenderTexture默认可能包含透明度
            return texture is RenderTexture;
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