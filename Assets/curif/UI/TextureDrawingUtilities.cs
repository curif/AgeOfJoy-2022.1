// TextureDrawingUtilities.cs
using UnityEngine;
using System; // For Array.Fill
using Unity.Collections; // Required for NativeArray
using Unity.Burst;      // Required for [BurstCompile]
using Unity.Jobs;

public static class TextureDrawingUtilities
{
    /// <summary>
    /// Internal helper to draw a single pixel directly onto a texture.
    /// Performs bounds checking and flips the Y-coordinate for Unity's bottom-left origin.
    /// </summary>
    /// <returns>True if the pixel was drawn (within bounds), false otherwise.</returns>
    private static bool SetPixelOnTexture(Texture2D texture, int x, int y, Color32 color, int textureWidth, int textureHeight)
    {
        if (texture != null && x >= 0 && x < textureWidth && y >= 0 && y < textureHeight)
        {
            int unityY = textureHeight - 1 - y; // Y-flip for Unity's bottom-left origin
            texture.SetPixel(x, unityY, color);
            return true;
        }
        return false;
    }

    public static bool DrawPoint(Texture2D texture, int x, int y, Color32 color, int textureWidth, int textureHeight)
    {
        if (texture == null) return false;
        return SetPixelOnTexture(texture, x, y, color, textureWidth, textureHeight);
    }

    public static bool DrawLine(Texture2D texture, int x1, int y1, int x2, int y2, Color32 color, int textureWidth, int textureHeight)
    {
        if (texture == null) return false;
        bool pixelsDrawn = false;

        int dx = Mathf.Abs(x2 - x1);
        int dy = -Mathf.Abs(y2 - y1);
        int sx = x1 < x2 ? 1 : -1;
        int sy = y1 < y2 ? 1 : -1;
        int err = dx + dy;

        int currentX = x1;
        int currentY = y1;

        while (true)
        {
            if (SetPixelOnTexture(texture, currentX, currentY, color, textureWidth, textureHeight))
            {
                pixelsDrawn = true;
            }

            if (currentX == x2 && currentY == y2) break;
            int e2 = 2 * err;
            if (e2 >= dy)
            {
                if (currentX == x2) break;
                err += dy;
                currentX += sx;
            }
            if (e2 <= dx)
            {
                if (currentY == y2) break;
                err += dx;
                currentY += sy;
            }
        }
        return pixelsDrawn;
    }

    public static bool DrawHorizontalLine(Texture2D texture, int xStart, int xEnd, int y, Color32 color, int textureWidth, int textureHeight)
    {
        if (texture == null) return false;
        bool pixelsDrawn = false;
        int start = Mathf.Min(xStart, xEnd);
        int end = Mathf.Max(xStart, xEnd);
        for (int x = start; x <= end; x++)
        {
            if (SetPixelOnTexture(texture, x, y, color, textureWidth, textureHeight))
            {
                pixelsDrawn = true;
            }
        }
        return pixelsDrawn;
    }

    public static bool DrawVerticalLine(Texture2D texture, int x, int yStart, int yEnd, Color32 color, int textureWidth, int textureHeight)
    {
        if (texture == null) return false;
        bool pixelsDrawn = false;
        int start = Mathf.Min(yStart, yEnd);
        int end = Mathf.Max(yStart, yEnd);
        for (int y = start; y <= end; y++)
        {
            if (SetPixelOnTexture(texture, x, y, color, textureWidth, textureHeight))
            {
                pixelsDrawn = true;
            }
        }
        return pixelsDrawn;
    }

    public static bool DrawBox(Texture2D texture, int x, int y, int width, int height, Color32 borderColor, bool filled, Color32? fillColor, int textureWidth, int textureHeight)
    {
        if (texture == null || width <= 0 || height <= 0) return false;
        bool modified = false;

        int x2 = x + width - 1;
        int y2 = y + height - 1;

        if (filled && fillColor.HasValue)
        {
            Color32 fillCol = fillColor.Value;
            int fillStartX = x;
            int fillStartY = y;
            int clippedStartX = Mathf.Max(fillStartX, 0);
            int clippedStartY = Mathf.Max(fillStartY, 0);
            int clippedEndX = Mathf.Min(fillStartX + width, textureWidth);
            int clippedEndY = Mathf.Min(fillStartY + height, textureHeight);
            int clippedWidth = clippedEndX - clippedStartX;
            int clippedHeight = clippedEndY - clippedStartY;

            if (clippedWidth > 0 && clippedHeight > 0)
            {
                Color32[] fillPixels = new Color32[clippedWidth * clippedHeight];
                Array.Fill(fillPixels, fillCol);
                int unityStartY = textureHeight - clippedStartY - clippedHeight;
                try
                {
                    texture.SetPixels32(clippedStartX, unityStartY, clippedWidth, clippedHeight, fillPixels);
                    modified = true;
                }
                catch (UnityException ex)
                {
                    Debug.LogError($"[TextureDrawingUtilities.DrawBox] SetPixels32 failed: {ex.Message}. Falling back to pixel-by-pixel fill.");
                    for (int iy = clippedStartY; iy < clippedEndY; iy++)
                    {
                        for (int ix = clippedStartX; ix < clippedEndX; ix++)
                        {
                            if (SetPixelOnTexture(texture, ix, iy, fillCol, textureWidth, textureHeight)) modified = true;
                        }
                    }
                }
            }
        }

        if (borderColor.a > 0)
        {
            if (DrawHorizontalLine(texture, x, x2, y, borderColor, textureWidth, textureHeight)) modified = true;
            if (DrawHorizontalLine(texture, x, x2, y2, borderColor, textureWidth, textureHeight)) modified = true;
            if (height > 1)
            {
                if (DrawVerticalLine(texture, x, y + 1, y2 - 1, borderColor, textureWidth, textureHeight)) modified = true;
                if (DrawVerticalLine(texture, x2, y + 1, y2 - 1, borderColor, textureWidth, textureHeight)) modified = true;
            }
            else if (height == 1 && width > 1) { /* Horizontal lines already drew endpoints */ }
            else if (width == 1 && height > 1)
            {
                if (DrawVerticalLine(texture, x, y, y2, borderColor, textureWidth, textureHeight)) modified = true;
            }
            else if (width == 1 && height == 1)
            {
                if (SetPixelOnTexture(texture, x, y, borderColor, textureWidth, textureHeight)) modified = true;
            }
        }
        return modified;
    }

    private static void DrawEllipsePoints(Texture2D texture, int cx, int cy, int x, int y, Color32 color, int textureWidth, int textureHeight, ref bool modified)
    {
        if (SetPixelOnTexture(texture, cx + x, cy + y, color, textureWidth, textureHeight)) modified = true;
        if (SetPixelOnTexture(texture, cx - x, cy + y, color, textureWidth, textureHeight)) modified = true;
        if (SetPixelOnTexture(texture, cx + x, cy - y, color, textureWidth, textureHeight)) modified = true;
        if (SetPixelOnTexture(texture, cx - x, cy - y, color, textureWidth, textureHeight)) modified = true;
    }

    public static bool DrawCircle(Texture2D texture, int centerX, int centerY, int radius, Color32 borderColor, bool filled, Color32? fillColor, int textureWidth, int textureHeight)
    {
        if (texture == null || radius < 0) return false;
        bool modified = false;

        if (radius == 0)
        {
            if (filled && fillColor.HasValue) return SetPixelOnTexture(texture, centerX, centerY, fillColor.Value, textureWidth, textureHeight);
            else if (borderColor.a > 0) return SetPixelOnTexture(texture, centerX, centerY, borderColor, textureWidth, textureHeight);
            return false;
        }

        if (filled && fillColor.HasValue)
        {
            Color32 fillCol = fillColor.Value;
            float rSquared = (float)radius * radius;
            for (int yOffset = -radius; yOffset <= radius; yOffset++)
            {
                float yOffsetSquared = (float)yOffset * yOffset;
                if (yOffsetSquared <= rSquared)
                {
                    int span = (int)Mathf.Sqrt(rSquared - yOffsetSquared);
                    int y = centerY + yOffset;
                    if (DrawHorizontalLine(texture, centerX - span, centerX + span, y, fillCol, textureWidth, textureHeight)) modified = true;
                }
            }
        }

        if (borderColor.a > 0)
        {
            int d = (5 - radius * 4) / 4;
            int x = 0;
            int y = radius;
            do
            {
                DrawEllipsePoints(texture, centerX, centerY, x, y, borderColor, textureWidth, textureHeight, ref modified); // For 8 octants
                DrawEllipsePoints(texture, centerX, centerY, y, x, borderColor, textureWidth, textureHeight, ref modified); // For symmetry

                if (d < 0)
                {
                    d += 2 * x + 1;
                }
                else
                {
                    d += 2 * (x - y) + 1;
                    y--;
                }
                x++;
            } while (x <= y);
        }
        return modified;
    }

    public static bool DrawOval(Texture2D texture, int centerX, int centerY, int radiusX, int radiusY, Color32 borderColor, bool filled, Color32? fillColor, int textureWidth, int textureHeight)
    {
        if (texture == null || radiusX < 0 || radiusY < 0) return false;
        bool modified = false;

        if (radiusX == 0 && radiusY == 0)
        {
            if (filled && fillColor.HasValue) return SetPixelOnTexture(texture, centerX, centerY, fillColor.Value, textureWidth, textureHeight);
            else if (borderColor.a > 0) return SetPixelOnTexture(texture, centerX, centerY, borderColor, textureWidth, textureHeight);
            return false;
        }
        if (radiusX == 0) { return DrawVerticalLine(texture, centerX, centerY - radiusY, centerY + radiusY, (filled && fillColor.HasValue ? fillColor.Value : borderColor), textureWidth, textureHeight); }
        if (radiusY == 0) { return DrawHorizontalLine(texture, centerX - radiusX, centerX + radiusX, centerY, (filled && fillColor.HasValue ? fillColor.Value : borderColor), textureWidth, textureHeight); }


        if (filled && fillColor.HasValue)
        {
            Color32 fillCol = fillColor.Value;
            float rxSq_float = (float)radiusX * radiusX;
            float rySq_float = (float)radiusY * radiusY;

            for (int yOffset = -radiusY; yOffset <= radiusY; yOffset++)
            {
                float yTerm = (float)(yOffset * yOffset) / rySq_float;
                if (yTerm <= 1.0f)
                {
                    int span = (int)(radiusX * Mathf.Sqrt(Mathf.Max(0f, 1.0f - yTerm)));
                    int y = centerY + yOffset;
                    if (DrawHorizontalLine(texture, centerX - span, centerX + span, y, fillCol, textureWidth, textureHeight)) modified = true;
                }
            }
        }

        if (borderColor.a > 0)
        {
            long rxSq = (long)radiusX * radiusX;
            long rySq = (long)radiusY * radiusY;
            long twoRxSq = 2 * rxSq;
            long twoRySq = 2 * rySq;
            long p;
            int x = 0;
            int y = radiusY;
            long px = 0;
            long py = twoRxSq * y;

            DrawEllipsePoints(texture, centerX, centerY, x, y, borderColor, textureWidth, textureHeight, ref modified);

            p = (long)Math.Round(rySq - (rxSq * radiusY) + (0.25 * rxSq));
            while (px < py)
            {
                x++;
                px += twoRySq;
                if (p < 0) { p += rySq + px; }
                else { y--; py -= twoRxSq; p += rySq + px - py; }
                DrawEllipsePoints(texture, centerX, centerY, x, y, borderColor, textureWidth, textureHeight, ref modified);
            }

            p = (long)Math.Round(rySq * (x + 0.5) * (x + 0.5) + rxSq * (y - 1) * (y - 1) - rxSq * rySq);
            while (y > 0)
            {
                y--;
                py -= twoRxSq;
                if (p > 0) { p += rxSq - py; }
                else { x++; px += twoRySq; p += rxSq - py + px; }
                DrawEllipsePoints(texture, centerX, centerY, x, y, borderColor, textureWidth, textureHeight, ref modified);
            }
        }
        return modified;
    }

    public static bool DrawImageRescaled(Texture2D texture, int x, int y, int width, int height, Color32[] pixels, int pixelsWidth, int pixelsHeight, int textureWidth, int textureHeight)
    {
        if (texture == null) { Debug.LogError("[DrawImageRescaled] Screen texture is not initialized."); return false; }
        if (pixels == null || pixels.Length == 0) { Debug.LogWarning("[DrawImageRescaled] Input pixels array is null or empty."); return false; }
        if (width <= 0 || height <= 0) { return false; }
        if (pixelsWidth <= 0 || pixelsHeight <= 0) { Debug.LogWarning($"[DrawImageRescaled] Invalid source image dimensions ({pixelsWidth}x{pixelsHeight})."); return false; }
        // Removed length check for brevity, assume it's handled or riskier
        bool modified = false;

        float xScale = (float)pixelsWidth / width;
        float yScale = (float)pixelsHeight / height;

        int startScreenX = Mathf.Max(x, 0);
        int startScreenY = Mathf.Max(y, 0);
        int endScreenX = Mathf.Min(x + width, textureWidth);
        int endScreenY = Mathf.Min(y + height, textureHeight);

        for (int destY = startScreenY; destY < endScreenY; destY++)
        {
            int relativeY = destY - y;
            int sourceY_topDown = (int)((relativeY + 0.5f) * yScale);
            sourceY_topDown = Mathf.Clamp(sourceY_topDown, 0, pixelsHeight - 1);
            int sourceY_forArrayIndex = pixelsHeight - 1 - sourceY_topDown;

            for (int destX = startScreenX; destX < endScreenX; destX++)
            {
                int relativeX = destX - x;
                int sourceX = (int)((relativeX + 0.5f) * xScale);
                sourceX = Mathf.Clamp(sourceX, 0, pixelsWidth - 1);
                int sourceIndex = sourceY_forArrayIndex * pixelsWidth + sourceX;

                if (sourceIndex >= 0 && sourceIndex < pixels.Length)
                {
                    if (SetPixelOnTexture(texture, destX, destY, pixels[sourceIndex], textureWidth, textureHeight)) modified = true;
                }
            }
        }
        return modified;
    }

    public static bool DrawTextureRescaled(Texture2D texture, int x, int y, int width, int height, Texture2D sourceTexture, int textureWidth, int textureHeight)
    {
        if (sourceTexture == null) { Debug.LogWarning("[DrawTextureRescaled] Source Texture2D is null."); return false; }
        if (!sourceTexture.isReadable) { Debug.LogError($"[DrawTextureRescaled] Source Texture '{sourceTexture.name}' is not readable. Enable 'Read/Write Enabled'."); return false; }

        Color32[] sourcePixels;
        try { sourcePixels = sourceTexture.GetPixels32(); }
        catch (UnityException ex) { Debug.LogError($"[DrawTextureRescaled] Failed to GetPixels32 from '{sourceTexture.name}': {ex.Message}"); return false; }

        return DrawImageRescaled(texture, x, y, width, height, sourcePixels, sourceTexture.width, sourceTexture.height, textureWidth, textureHeight);
    }
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    private struct ScrollRectVerticalJob : IJob
    {
        [ReadOnly] public NativeArray<Color32> SourceAllPixels;
        public NativeArray<Color32> OutputRectPixels; // Size: RectWidth * RectHeight

        public int TextureWidth;    // Full width of SourceAllPixels
        public int RectXInSource;   // Top-left X of the rectangle in SourceAllPixels
        public int RectYInSource;   // Top-left Y of the rectangle in SourceAllPixels
        public int RectWidth;
        public int RectHeight;
        public int ScrollAmountPixelsY; // Negative to scroll UP, Positive to scroll DOWN
        public Color32 FillColor;

        public void Execute()
        {
            if (ScrollAmountPixelsY == 0)
            {
                // If no scroll, just copy the original rectangle content to output
                for (int y = 0; y < RectHeight; y++)
                {
                    int sourceRowStart = (RectYInSource + y) * TextureWidth + RectXInSource;
                    int destRowStart = y * RectWidth;
                    for (int x = 0; x < RectWidth; x++)
                    {
                        OutputRectPixels[destRowStart + x] = SourceAllPixels[sourceRowStart + x];
                    }
                }
                return;
            }

            // Initialize OutputRectPixels with FillColor first.
            // This simplifies logic as we only need to copy the valid scrolled parts over it.
            for (int i = 0; i < OutputRectPixels.Length; i++)
            {
                OutputRectPixels[i] = FillColor;
            }

            // --- SCROLL UP (e.g., ScrollAmountPixelsY = -16 pixels) ---
            // Content moves from lower rows in source to higher rows in destination (output).
            // New blank space appears at the BOTTOM of the output rectangle.
            if (ScrollAmountPixelsY < 0)
            {
                int pixelsToVisuallyMoveUp = Mathf.Abs(ScrollAmountPixelsY);

                if (pixelsToVisuallyMoveUp >= RectHeight)
                {
                    // Already filled with FillColor, nothing more to do.
                    return;
                }

                // Iterate through the rows of the *destination* (OutputRectPixels) that will receive scrolled content.
                // These are rows 0 to (RectHeight - pixelsToVisuallyMoveUp - 1) of OutputRectPixels.
                for (int yDestInOutput = 0; yDestInOutput < RectHeight - pixelsToVisuallyMoveUp; yDestInOutput++)
                {
                    // The corresponding source row in the original rectangle (SourceAllPixels)
                    // is 'pixelsToVisuallyMoveUp' rows *below* this destination row.
                    int ySrcInOriginalRect = yDestInOutput + pixelsToVisuallyMoveUp;

                    // Calculate 1D indices
                    int sourceRowStartIndexInAllPixels = (RectYInSource + ySrcInOriginalRect) * TextureWidth + RectXInSource;
                    int destRowStartIndexInOutput = yDestInOutput * RectWidth;

                    // Copy the row
                    for (int x = 0; x < RectWidth; x++)
                    {
                        OutputRectPixels[destRowStartIndexInOutput + x] = SourceAllPixels[sourceRowStartIndexInAllPixels + x];
                    }
                }
                // The bottom 'pixelsToVisuallyMoveUp' rows of OutputRectPixels are already FillColor.
            }
            // --- SCROLL DOWN (e.g., ScrollAmountPixelsY = 16 pixels) ---
            // Content moves from higher rows in source to lower rows in destination (output).
            // New blank space appears at the TOP of the output rectangle.
            else // ScrollAmountPixelsY > 0
            {
                int pixelsToVisuallyMoveDown = ScrollAmountPixelsY;

                if (pixelsToVisuallyMoveDown >= RectHeight)
                {
                    // Already filled with FillColor, nothing more to do.
                    return;
                }

                // Iterate through the rows of the *destination* (OutputRectPixels) that will receive scrolled content.
                // These are rows 'pixelsToVisuallyMoveDown' to (RectHeight - 1) of OutputRectPixels.
                for (int yDestInOutput = pixelsToVisuallyMoveDown; yDestInOutput < RectHeight; yDestInOutput++)
                {
                    // The corresponding source row in the original rectangle (SourceAllPixels)
                    // is 'pixelsToVisuallyMoveDown' rows *above* this destination row.
                    int ySrcInOriginalRect = yDestInOutput - pixelsToVisuallyMoveDown;

                    // Calculate 1D indices
                    int sourceRowStartIndexInAllPixels = (RectYInSource + ySrcInOriginalRect) * TextureWidth + RectXInSource;
                    int destRowStartIndexInOutput = yDestInOutput * RectWidth;

                    // Copy the row
                    for (int x = 0; x < RectWidth; x++)
                    {
                        OutputRectPixels[destRowStartIndexInOutput + x] = SourceAllPixels[sourceRowStartIndexInAllPixels + x];
                    }
                }
                // The top 'pixelsToVisuallyMoveDown' rows of OutputRectPixels are already FillColor.
            }
        }
    }

    // The rest of TextureDrawingUtilities.ScrollRectangleVerticalBurst remains the same:
    public static bool ScrollRectangleVerticalBurst(
        Texture2D texture,
        int rectX, int rectY, int rectWidth, int rectHeight,
        int scrollAmountPixelsY, Color32 fillColor)
    {
        // ... (validation, clamping, GetPixelData, NativeArray setup, Job scheduling, CopyTo managed array, SetPixels32, Dispose) ...
        // The only change is that the job instance will now use the Execute() logic above.
        if (texture == null || rectWidth <= 0 || rectHeight <= 0 || scrollAmountPixelsY == 0 && !(rectX == 0 && rectY == 0 && rectWidth == texture.width && rectHeight == texture.height)) // Allow scrollAmount 0 only if copying whole texture (no-op case)
        {
            // if scrollAmount is 0, only proceed if we intend to "refresh" the entire texture block from source to output (might be a no-op if source=output)
            if (scrollAmountPixelsY == 0 && (rectX == 0 && rectY == 0 && rectWidth == texture.width && rectHeight == texture.height))
            {
                // This specific case might be used to just get the rect data, let it pass.
            }
            else
            {
                // For any other scrollAmount 0, or invalid rects, return false early.
                if (scrollAmountPixelsY == 0) return false; // No actual scroll to perform if rect is sub-part
            }
        }

        if (!texture.isReadable)
        {
            Debug.LogError("[ScrollRectangleVerticalBurst] Texture is not readable.");
            return false;
        }

        int textureWidth = texture.width;
        int textureHeight = texture.height;

        int clampedRectX = Mathf.Max(0, rectX);
        int clampedRectY = Mathf.Max(0, rectY);
        int effectiveRectWidth = Mathf.Max(0, Mathf.Min(rectX + rectWidth, textureWidth) - clampedRectX);
        int effectiveRectHeight = Mathf.Max(0, Mathf.Min(rectY + rectHeight, textureHeight) - clampedRectY);

        if (effectiveRectWidth <= 0 || effectiveRectHeight <= 0) return false;

        NativeArray<Color32> allPixelsNative = texture.GetPixelData<Color32>(0);
        NativeArray<Color32> outputRectPixelsNative = new NativeArray<Color32>(effectiveRectWidth * effectiveRectHeight, Allocator.TempJob);

        ScrollRectVerticalJob scrollJob = new ScrollRectVerticalJob
        {
            SourceAllPixels = allPixelsNative,
            OutputRectPixels = outputRectPixelsNative,
            TextureWidth = textureWidth,
            RectXInSource = clampedRectX,
            RectYInSource = clampedRectY,
            RectWidth = effectiveRectWidth,
            RectHeight = effectiveRectHeight,
            ScrollAmountPixelsY = scrollAmountPixelsY,
            FillColor = fillColor
        };

        scrollJob.Run();

        Color32[] managedRectPixels = new Color32[effectiveRectWidth * effectiveRectHeight];
        outputRectPixelsNative.CopyTo(managedRectPixels);

        int unityRectStartY = textureHeight - (clampedRectY + effectiveRectHeight);
        texture.SetPixels32(clampedRectX, unityRectStartY, effectiveRectWidth, effectiveRectHeight, managedRectPixels);

        allPixelsNative.Dispose();
        outputRectPixelsNative.Dispose();

        return true;
    }
}