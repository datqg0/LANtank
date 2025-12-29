using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace gametankz.Graphics;

/// <summary>
/// Represents a rectangular region within a texture.
/// </summary>
public class TextureRegion 
{
    public Texture2D Texture { get; set; }
    public Rectangle rect { get; set; }
    public int Width => Texture.Width;
    public int Height => Texture.Height;
    public TextureRegion(Texture2D texture, int x, int y, int width, int height)
    {
        Texture = texture;
        rect = new Rectangle(x, y, width, height);
    }
    public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color)
    {
        Draw(spriteBatch, position, color, 0.0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0.0f);
    }
    public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
    {
        Draw(
            spriteBatch,
            position,
            color,
            rotation,
            origin,
            new Vector2(scale, scale),
            effects,
            layerDepth
        );
    }
    public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
    {
        spriteBatch.Draw(
            Texture,
            position,
            rect,
            color,
            rotation,
            origin,
            scale,
            effects,
            layerDepth
        );
    }
}
