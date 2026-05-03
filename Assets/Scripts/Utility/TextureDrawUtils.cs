using UnityEngine;

/// <summary>
/// Shared pixel-level drawing utilities for Texture2D-based graphs.
/// </summary>
public static class TextureDrawUtils
{
    /// <summary>
    /// Draw a line on a Color32 pixel buffer using Bresenham's algorithm.
    /// </summary>
    public static void DrawLine(Color32[] pixels, int width, int height, int x0, int y0, int x1, int y1, Color32 colour, int thickness = 1)
    {
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            for (int t = 0; t < thickness; t++)
            {
                int py = y0 + t;
                if (x0 >= 0 && x0 < width && py >= 0 && py < height)
                {
                    pixels[py * width + x0] = colour;
                }
            }

            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
    }

    /// <summary>
    /// Draw a line on a Texture2D using SetPixel (for smaller graphs).
    /// </summary>
    public static void DrawLine(Texture2D texture, int x0, int y0, int x1, int y1, Color colour, int thickness = 1)
    {
        int width = texture.width;
        int height = texture.height;
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            for (int t = 0; t < thickness; t++)
            {
                int py = y0 + t;
                if (x0 >= 0 && x0 < width && py >= 0 && py < height)
                {
                    texture.SetPixel(x0, py, colour);
                }
            }

            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
    }
}
