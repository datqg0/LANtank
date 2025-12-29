using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using gametankz.Graphics;

public class Map
{
    public const int TILE = 32;

    int[,] grid;
    int w, h;

    TextureRegion floor;
    TextureRegion wall;

    public Map(TextureAtlas floorAtlas, TextureAtlas wallAtlas, int width, int height)
    {
        w = width;
        h = height;

        floor = floorAtlas.GetRegion("floor");
        wall  = wallAtlas.GetRegion("wall");

        Generate();
    }

    void Generate()
    {
        
        grid = new int[h, w];

        int pathY = h / 2;

        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
            grid[y, x] = 0;

        // Tạo tường bao quanh
        for (int x = 0; x < w; x++)
        {
            grid[0, x] = 1;
            grid[h - 1, x] = 1;
        }
        for (int y = 0; y < h; y++)
        {
            grid[y, 0] = 1;
            grid[y, w - 1] = 1;
        }

        // Tạo map cố định - lưới tường thưa hơn (8 tiles spacing)
        for (int y = 4; y < h - 2; y += 8)
        for (int x = 4; x < w - 2; x += 8)
        {
            grid[y, x]     = 1;
            grid[y+1, x]   = 1;
            grid[y, x+1]   = 1;
            grid[y+1, x+1] = 1;
        }
    }


    // ===== COLLISION =====
    public bool IsWall(Rectangle rect)
    {
        int left   = rect.Left   / TILE;
        int right  = rect.Right  / TILE;
        int top    = rect.Top    / TILE;
        int bottom = rect.Bottom / TILE;

        for (int y = top; y <= bottom; y++)
        for (int x = left; x <= right; x++)
        {
            if (x < 0 || y < 0 || x >= w || y >= h) continue;
            if (grid[y, x] == 1) return true;
        }
        return false;
    }

    // ===== BULLET HIT =====
    public bool HitWall(Vector2 pos)
    {
        // Check exact tile at bullet position
        int tx = (int)(pos.X / TILE);
        int ty = (int)(pos.Y / TILE);

        if (tx < 0 || ty < 0 || tx >= w || ty >= h) return false;
        return grid[ty, tx] == 1;
    }

    public void Draw(SpriteBatch sb)
    {
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            TextureRegion r = grid[y, x] == 1 ? wall : floor;
            r.Draw(
                sb,
                new Vector2(x * TILE, y * TILE),
                Color.White,
                0f,
                Vector2.Zero,
                0.1f,
                SpriteEffects.None,
                0f
            );
        }
    }
}
