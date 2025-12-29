using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using gametankz.Graphics;

namespace gametankz
{
    public class Tank
    {
        // ===== POSITION & MOVEMENT =====
        public Vector2 Position;
        private float moveSpeed = 120f;

        // ===== ROTATION =====
        private int currentDir = 0;
        private int goalDir = 0;
        private float rotateSpeed = 50f;
        private float rotateAccum = 0f;

        // ===== HEALTH =====
        public int Health;
        private int maxHealth = 100;

        // ===== SPRITES =====
        private TextureRegion[] spikeFrames;
        private TextureRegion bulletUp;
        private TextureRegion bulletDown;
        private TextureRegion bulletLeft;
        private TextureRegion bulletRight;
        private TextureRegion healthBarSprite;

        // ===== MAP COLLISION =====
        private Map map;
        private const int TANK_SIZE = 38;

        public Tank(Vector2 startPos, TextureRegion[] frames, TextureRegion spriteUp, 
                    TextureRegion spriteDown, TextureRegion spriteLeft, TextureRegion spriteRight,
                    TextureRegion healthBar, Map gameMap)
        {
            Position = startPos;
            Health = maxHealth;
            spikeFrames = frames;
            bulletUp = spriteUp;
            bulletDown = spriteDown;
            bulletLeft = spriteLeft;
            bulletRight = spriteRight;
            healthBarSprite = healthBar;
            map = gameMap;
        }

        public void Update(float dt, KeyboardState keyboard)
        {
            // ===== SET GOAL DIR =====
            if (keyboard.IsKeyDown(Keys.Left) || keyboard.IsKeyDown(Keys.A)) goalDir = 4;
            else if (keyboard.IsKeyDown(Keys.Right) || keyboard.IsKeyDown(Keys.D)) goalDir = 12;
            else if (keyboard.IsKeyDown(Keys.Up) || keyboard.IsKeyDown(Keys.W)) goalDir = 8;
            else if (keyboard.IsKeyDown(Keys.Down) || keyboard.IsKeyDown(Keys.S)) goalDir = 0;

            // ===== ROTATE SMOOTH =====
            rotateAccum += rotateSpeed * dt;
            if (rotateAccum >= 1f)
            {
                rotateAccum = 0f;
                int diff = (goalDir - currentDir + 16) % 16;

                if (diff != 0)
                {
                    if (diff <= 8) currentDir = (currentDir + 1) % 16;
                    else currentDir = (currentDir - 1 + 16) % 16;
                }
            }

            // ===== MOVE (ONLY WHEN ROTATED) =====
            if (currentDir == goalDir)
            {
                Vector2 move = Vector2.Zero;

                if (keyboard.IsKeyDown(Keys.Left) || keyboard.IsKeyDown(Keys.A)) move = new Vector2(-1, 0);
                else if (keyboard.IsKeyDown(Keys.Right) || keyboard.IsKeyDown(Keys.D)) move = new Vector2(1, 0);
                else if (keyboard.IsKeyDown(Keys.Up) || keyboard.IsKeyDown(Keys.W)) move = new Vector2(0, -1);
                else if (keyboard.IsKeyDown(Keys.Down) || keyboard.IsKeyDown(Keys.S)) move = new Vector2(0, 1);

                // Thử di chuyển với collision
                if (move != Vector2.Zero)
                {
                    Vector2 newPos = Position + move * moveSpeed * dt;
                    Rectangle tankRect = new Rectangle((int)newPos.X + 8, (int)newPos.Y + 8, TANK_SIZE - 16, TANK_SIZE - 16);
                    
                    if (!map.IsWall(tankRect))
                    {
                        Position = newPos;
                    }
                }
            }
        }

        public void TakeDamage(int damage)
        {
            Health -= damage;
        }

        public bool IsAlive()
        {
            return Health > 0;
        }

        public void GetBulletData(out Vector2 dir, out TextureRegion sprite)
        {
            dir = Vector2.Zero;
            sprite = null;

            switch (currentDir)
            {
                case 0:  dir = new Vector2(0, 1);  sprite = bulletDown;  break;
                case 4:  dir = new Vector2(-1, 0); sprite = bulletLeft;  break;
                case 8:  dir = new Vector2(0, -1); sprite = bulletUp;    break;
                case 12: dir = new Vector2(1, 0);  sprite = bulletRight; break;
            }
        }

        public int GetCurrentDir()
        {
            return currentDir;
        }

        public int GetGoalDir()
        {
            return goalDir;
        }

        public Rectangle GetCollisionRect()
        {
            return new Rectangle((int)Position.X + 8, (int)Position.Y + 8, TANK_SIZE - 16, TANK_SIZE - 16);
        }

        public void Draw(SpriteBatch sb)
        {
            // Vẽ tank
            spikeFrames[currentDir].Draw(
                sb,
                Position,
                Color.White,
                0f,
                Vector2.One,
                0.1f,
                SpriteEffects.None,
                0f
            );

            // Vẽ thanh máu
            float healthPercent = Health / (float)maxHealth;
            Color healthColor = Health > 50 ? Color.LimeGreen : (Health > 25 ? Color.Orange : Color.Red);
            
            healthBarSprite.Draw(
                sb,
                new Vector2(Position.X, Position.Y - 20),
                healthColor,
                0f,
                new Vector2(0.5f, 0),
                new Vector2(healthPercent / 10, 0.1f),
                SpriteEffects.None,
                0f
            );
        }
        public void Reset()
        {
            Position = new Vector2(640, 352);
            Health = maxHealth;
            currentDir = 0;
            goalDir = 0;
            rotateAccum = 0f;
        }
    }
}
