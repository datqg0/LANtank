using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using gametankz.Graphics;

namespace gametankz
{
    public class Bullet
    {
        public Vector2 Position;
        public Vector2 Direction;
        public float Speed = 500f;
        public bool Alive = true;
        public TextureRegion Sprite;

        public Bullet(Vector2 pos, Vector2 dir, TextureRegion sprite)
        {
            Position = pos;
            Direction = dir;
            Sprite = sprite;
        }

        public void Update(float dt)
        {
            Position += Direction * Speed * dt;

            if (Position.X < -20 || Position.X > 1400 ||
                Position.Y < -20 || Position.Y > 800)
            {
                Alive = false;
            }
        }

        public void Draw(SpriteBatch sb)
        {
            Sprite.Draw(
                sb,
                Position,
                Color.White,
                0f,
                Vector2.One,
                0.1f,
                SpriteEffects.None,
                0f
            );
        }
    }
}
