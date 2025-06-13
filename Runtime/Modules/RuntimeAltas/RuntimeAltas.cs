using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace LiteFramework.Module
{
    public enum PackingAlgorithm
    {
        Linear,         // 原始线性布局
        BestFit,        // 最佳适应算法
        SkyLine,        // 天际线算法
        MaxRects        // 最大矩形算法
    }

    public class RuntimeAtlas
    {
        private RenderTexture atlasRT;
        private Material conversionMaterial;
        private Material drawMaterial;
        private int atlasSize, padding;
        private bool isDisposed = false;
        private bool useDirectBlit = true;
        private bool forceUseBlit = false;

        // 布局算法相关
        private PackingAlgorithm packingAlgorithm = PackingAlgorithm.MaxRects;
        private List<Rect> freeRects = new List<Rect>();
        private List<Rect> usedRects = new List<Rect>();
        private List<SkyLineNode> skyLine = new List<SkyLineNode>();

        // 线性布局数据（兼容性）
        private int currentX = 0, currentY = 0, rowHeight = 0;

        public Texture Texture => atlasRT;
        public bool IsValid => atlasRT != null && atlasRT.IsCreated();
        public int UsedPixels => usedRects.Sum(r => (int)(r.width * r.height));
        public float Efficiency => (float)UsedPixels / (atlasSize * atlasSize) * 100f;


        public RuntimeAtlas(int size, int pad = 1, PackingAlgorithm algorithm = PackingAlgorithm.SkyLine, Material blitMaterial = null)
        {
            if (size <= 0 || pad < 0)
                throw new ArgumentException("Atlas size and padding must be positive values");

            atlasSize = size;
            padding = pad;
            packingAlgorithm = algorithm;

            useDirectBlit = SystemInfo.copyTextureSupport != CopyTextureSupport.None;

            Debug.Log($"Atlas initialized: {size}x{size}, padding: {padding}, algorithm: {algorithm}");

            atlasRT = new RenderTexture(size, size, 0, RenderTextureFormat.ARGB32)
            {
                name = "RuntimeAtlas",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            atlasRT.Create();

            CreateConversionMaterial();
            if (blitMaterial == null)
            {
                CreateDrawMaterial();
            }
            else
            {
                drawMaterial = blitMaterial;
                forceUseBlit = true;
            }
            InitializeAlgorithm();
        }

        private void InitializeAlgorithm()
        {
            switch (packingAlgorithm)
            {
                case PackingAlgorithm.MaxRects:
                    freeRects.Clear();
                    freeRects.Add(new Rect(0, 0, atlasSize, atlasSize));
                    break;
                case PackingAlgorithm.SkyLine:
                    skyLine.Clear();
                    skyLine.Add(new SkyLineNode(0, 0, atlasSize));
                    break;
                case PackingAlgorithm.Linear:
                case PackingAlgorithm.BestFit:
                    ResetLinearPacking();
                    break;
            }
            usedRects.Clear();
        }

        #region MaxRects Algorithm (推荐)

        private Rect FindMaxRectsPosition(int width, int height)
        {
            int bestShortSideFit = int.MaxValue;
            int bestLongSideFit = int.MaxValue;
            Rect bestRect = new Rect(-1, -1, 0, 0);

            foreach (var rect in freeRects)
            {
                if (rect.width >= width && rect.height >= height)
                {
                    int shortSideFit = Mathf.Min((int)(rect.width - width), (int)(rect.height - height));
                    int longSideFit = Mathf.Max((int)(rect.width - width), (int)(rect.height - height));

                    if (shortSideFit < bestShortSideFit ||
                        (shortSideFit == bestShortSideFit && longSideFit < bestLongSideFit))
                    {
                        bestRect = new Rect(rect.x, rect.y, width, height);
                        bestShortSideFit = shortSideFit;
                        bestLongSideFit = longSideFit;
                    }
                }
            }

            if (bestRect.x >= 0)
            {
                // 分割矩形
                SplitFreeRect(bestRect);
                PruneFreeRects();
            }

            return bestRect;
        }

        private void SplitFreeRect(Rect usedRect)
        {
            for (int i = freeRects.Count - 1; i >= 0; i--)
            {
                var freeRect = freeRects[i];
                if (!freeRect.Overlaps(usedRect))
                    continue;

                freeRects.RemoveAt(i);

                // 创建新的自由矩形
                if (freeRect.x < usedRect.x)
                {
                    freeRects.Add(new Rect(freeRect.x, freeRect.y, usedRect.x - freeRect.x, freeRect.height));
                }
                if (usedRect.x + usedRect.width < freeRect.x + freeRect.width)
                {
                    freeRects.Add(new Rect(usedRect.x + usedRect.width, freeRect.y,
                        freeRect.x + freeRect.width - (usedRect.x + usedRect.width), freeRect.height));
                }
                if (freeRect.y < usedRect.y)
                {
                    freeRects.Add(new Rect(freeRect.x, freeRect.y, freeRect.width, usedRect.y - freeRect.y));
                }
                if (usedRect.y + usedRect.height < freeRect.y + freeRect.height)
                {
                    freeRects.Add(new Rect(freeRect.x, usedRect.y + usedRect.height,
                        freeRect.width, freeRect.y + freeRect.height - (usedRect.y + usedRect.height)));
                }
            }
        }

        private void PruneFreeRects()
        {
            for (int i = freeRects.Count - 1; i >= 0; i--)
            {
                for (int j = i - 1; j >= 0; j--)
                {
                    if (IsContainedIn(freeRects[i], freeRects[j]))
                    {
                        freeRects.RemoveAt(i);
                        break;
                    }
                    if (IsContainedIn(freeRects[j], freeRects[i]))
                    {
                        freeRects.RemoveAt(j);
                        i--;
                    }
                }
            }
        }

        private bool IsContainedIn(Rect a, Rect b)
        {
            return a.x >= b.x && a.y >= b.y &&
                   a.x + a.width <= b.x + b.width &&
                   a.y + a.height <= b.y + b.height;
        }

        #endregion

        #region SkyLine Algorithm

        private struct SkyLineNode
        {
            public int x, y, width;
            public SkyLineNode(int x, int y, int width)
            {
                this.x = x;
                this.y = y;
                this.width = width;
            }
        }

        private Rect FindSkyLinePosition(int width, int height)
        {
            int bestHeight = int.MaxValue;
            int bestWidth = int.MaxValue;
            int bestIndex = -1;
            Rect bestRect = new Rect(-1, -1, 0, 0);

            for (int i = 0; i < skyLine.Count; i++)
            {
                int y = skyLine[i].y;
                if (RectangleFits(i, width, height, ref y))
                {
                    int wastedWidth = 0;
                    for (int j = i; j < skyLine.Count && skyLine[j].x < skyLine[i].x + width; j++)
                    {
                        wastedWidth += skyLine[j].width;
                    }

                    if (y < bestHeight || (y == bestHeight && wastedWidth < bestWidth))
                    {
                        bestHeight = y;
                        bestWidth = wastedWidth;
                        bestIndex = i;
                        bestRect = new Rect(skyLine[i].x, y, width, height);
                    }
                }
            }

            if (bestIndex != -1)
            {
                AddSkyLineLevel(bestIndex, bestRect);
            }

            return bestRect;
        }

        private bool RectangleFits(int index, int width, int height, ref int y)
        {
            int x = skyLine[index].x;
            if (x + width > atlasSize)
                return false;

            int widthLeft = width;
            int i = index;
            y = skyLine[index].y;

            while (widthLeft > 0)
            {
                if (i >= skyLine.Count)
                    return false;

                y = Mathf.Max(y, skyLine[i].y);
                if (y + height > atlasSize)
                    return false;

                widthLeft -= skyLine[i].width;
                i++;
            }

            return true;
        }

        private void AddSkyLineLevel(int index, Rect rect)
        {
            var newNode = new SkyLineNode((int)rect.x, (int)(rect.y + rect.height), (int)rect.width);

            skyLine.Insert(index, newNode);

            for (int i = index + 1; i < skyLine.Count; i++)
            {
                if (skyLine[i].x < skyLine[i - 1].x + skyLine[i - 1].width)
                {
                    int shrink = skyLine[i - 1].x + skyLine[i - 1].width - skyLine[i].x;
                    var modifiedNode = skyLine[i];
                    modifiedNode.x += shrink;
                    modifiedNode.width -= shrink;
                    skyLine[i] = modifiedNode;

                    if (skyLine[i].width <= 0)
                    {
                        skyLine.RemoveAt(i);
                        i--;
                    }
                    else
                        break;
                }
                else
                    break;
            }

            MergeSkyLines();
        }

        private void MergeSkyLines()
        {
            for (int i = 0; i < skyLine.Count - 1; i++)
            {
                if (skyLine[i].y == skyLine[i + 1].y)
                {
                    var mergedNode = skyLine[i];
                    mergedNode.width += skyLine[i + 1].width;
                    skyLine[i] = mergedNode;
                    skyLine.RemoveAt(i + 1);
                    i--;
                }
            }
        }

        #endregion

        #region BestFit Algorithm

        private Rect FindBestFitPosition(int width, int height)
        {
            var sortedRects = usedRects.OrderBy(r => r.x).ThenBy(r => r.y).ToList();

            // 尝试在已使用的矩形之间找到最佳位置
            for (int y = 0; y <= atlasSize - height; y += 10) // 步长优化
            {
                for (int x = 0; x <= atlasSize - width; x += 10)
                {
                    var candidate = new Rect(x, y, width, height);
                    if (!usedRects.Any(r => r.Overlaps(candidate)))
                    {
                        return candidate;
                    }
                }
            }

            return new Rect(-1, -1, 0, 0);
        }

        #endregion


        public bool TryGetPosition(int width, int height, out Rect position)
        {
            int paddedW = width + 2 * padding;
            int paddedH = height + 2 * padding;

            if (paddedW > atlasSize || paddedH > atlasSize)
            {
                position = new Rect(-1, -1, 0, 0);
                return false;
            }

            switch (packingAlgorithm)
            {
                case PackingAlgorithm.MaxRects:
                    position = FindMaxRectsPosition(paddedW, paddedH);
                    break;
                case PackingAlgorithm.SkyLine:
                    position = FindSkyLinePosition(paddedW, paddedH);
                    break;
                case PackingAlgorithm.BestFit:
                    position = FindBestFitPosition(paddedW, paddedH);
                    break;
                case PackingAlgorithm.Linear:
                default:
                    position = FindLinearPosition(paddedW, paddedH);
                    break;
            }

            return position.x >= 0;
        }
        public bool HasSpace(int width, int height)
        {
            if (width + 2 * padding > atlasSize || height + 2 * padding > atlasSize)
                return false;

            int paddedWidth = width + 2 * padding;
            int paddedHeight = height + 2 * padding;

            switch (packingAlgorithm)
            {
                case PackingAlgorithm.MaxRects:
                    return FindMaxRectsPosition(paddedWidth, paddedHeight).x >= 0;

                case PackingAlgorithm.SkyLine:
                    return FindSkyLinePosition(paddedWidth, paddedHeight).x >= 0;

                case PackingAlgorithm.BestFit:
                    return FindBestFitPosition(paddedWidth, paddedHeight).x >= 0;

                case PackingAlgorithm.Linear:
                default:
                    return FindLinearPosition(paddedWidth, paddedHeight).x >= 0;
            }
        }


        public bool TryAddTexture(Texture texture, out AtlasResult result)
        {
            try
            {
                result = AddTexture(texture);
            }
            catch
            {
                result = default;
                return false;
            }
            return true;
        }

        public AtlasResult AddTexture(Texture texture)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));

            if (isDisposed)
                throw new ObjectDisposedException(nameof(RuntimeAtlas));

            int w = texture.width;
            int h = texture.height;
            int paddedW = w + 2 * padding;
            int paddedH = h + 2 * padding;

            Rect position;

            switch (packingAlgorithm)
            {
                case PackingAlgorithm.MaxRects:
                    position = FindMaxRectsPosition(paddedW, paddedH);
                    break;

                case PackingAlgorithm.SkyLine:
                    position = FindSkyLinePosition(paddedW, paddedH);
                    break;

                case PackingAlgorithm.BestFit:
                    position = FindBestFitPosition(paddedW, paddedH);
                    break;

                case PackingAlgorithm.Linear:
                default:
                    position = FindLinearPosition(paddedW, paddedH);
                    break;
            }

            if (position.x < 0)
            {
                throw new InvalidOperationException(
                    $"Atlas is full! Cannot add {w}x{h} texture. " +
                    $"Current efficiency: {Efficiency:F1}%"
                );
            }

            // 添加填充
            int xPos = (int)position.x + padding;
            int yPos = (int)position.y + padding;

            BlitTextureToPosition(texture, xPos, yPos);

            // 记录使用的矩形
            usedRects.Add(position);

            // 计算UV坐标
            Rect uv = new Rect(
                (float)xPos / atlasSize,
                1f - (float)(yPos + h) / atlasSize,
                (float)w / atlasSize,
                (float)h / atlasSize
            );

            return new AtlasResult
            {
                texture = atlasRT,
                uv = uv,
                pixelRect = new Rect(xPos, yPos, w, h)
            };
        }

        private Rect FindLinearPosition(int width, int height)
        {
            if (currentX + width > atlasSize)
            {
                currentX = 0;
                currentY += rowHeight + padding;
                rowHeight = 0;
            }

            if (currentY + height > atlasSize)
                return new Rect(-1, -1, 0, 0);

            var rect = new Rect(currentX, currentY, width, height);
            currentX += width + padding;
            rowHeight = Mathf.Max(rowHeight, height);

            return rect;
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
            conversionMaterial.hideFlags = HideFlags.HideAndDontSave;
        }

        private void CreateDrawMaterial()
        {
            Shader drawShader = Shader.Find("Custom/DrawWithoutBlend");
            if (drawShader == null)
            {
                Debug.LogWarning("Could not find Custom/DrawWithoutBlend shader, falling back to UI/Default");
                drawShader = Shader.Find("UI/Default");
            }
            drawMaterial = new Material(drawShader);
            drawMaterial.hideFlags = HideFlags.HideAndDontSave;
        }


        private void BlitTextureToPosition(Texture sourceTexture, int destX, int destY)
        {
            int w = sourceTexture.width;
            int h = sourceTexture.height;
            bool isTransparent = HasTransparency(sourceTexture);

            if (!forceUseBlit && useDirectBlit && !isTransparent && CanUseCopyTexture(sourceTexture))
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

            BlitWithProperBlending(sourceTexture, destX, destY, w, h);
        }

        private void BlitWithProperBlending(Texture sourceTexture, int destX, int destY, int width, int height)
        {
            RenderTexture previousActive = RenderTexture.active;

            try
            {
                RenderTexture.active = atlasRT;
                GL.PushMatrix();
                GL.LoadPixelMatrix(0, atlasSize, atlasSize, 0);
                GL.Begin(GL.QUADS);

                drawMaterial.mainTexture = sourceTexture;
                drawMaterial.SetPass(0);

                GL.TexCoord2(0, 0); GL.Vertex3(destX, destY + height, 0);
                GL.TexCoord2(1, 0); GL.Vertex3(destX + width, destY + height, 0);
                GL.TexCoord2(1, 1); GL.Vertex3(destX + width, destY, 0);
                GL.TexCoord2(0, 1); GL.Vertex3(destX, destY, 0);

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
            return texture is RenderTexture;
        }

        private bool CanUseCopyTexture(Texture sourceTexture)
        {
            if (!(sourceTexture is Texture2D sourceTex2D))
                return false;

            if (!sourceTex2D.isReadable)
                return false;

            TextureFormat sourceFormat = sourceTex2D.format;
            switch (sourceFormat)
            {
                case TextureFormat.RGBA32:
                case TextureFormat.ARGB32:
                case TextureFormat.RGB24:
                    return true;
                default:
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

            InitializeAlgorithm();
            ResetLinearPacking();
        }

        private void ResetLinearPacking()
        {
            currentX = 0;
            currentY = 0;
            rowHeight = 0;
        }

        public void DebugLogStats()
        {
            Debug.Log($"Atlas Usage: {UsedPixels}/{atlasSize * atlasSize} pixels ({Efficiency:F1}% efficient)");
            Debug.Log($"Algorithm: {packingAlgorithm}, Used rectangles: {usedRects.Count}");
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

                if (conversionMaterial != null)
                {
                    UnityEngine.Object.DestroyImmediate(conversionMaterial);
                    conversionMaterial = null;
                }

                //排除配置表调用的
                if (!forceUseBlit && drawMaterial != null)
                {
                    UnityEngine.Object.DestroyImmediate(drawMaterial);
                    drawMaterial = null;
                }

                isDisposed = true;
            }
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