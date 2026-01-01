using System;
using System.Collections.Generic;
using System.Linq;
using gametankz.Graphics;
using gametankz.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary;

namespace gametankz
{
    public class Game1 : Core
    {
        public static Texture2D tankpic;

        int framecount = 16;
        int Width = 384;
        int Height = 384;

        TextureRegion[] spikeFrames;
        TextureRegion[] tankFrames = new TextureRegion[4]; // 0=down, 1=left, 2=up, 3=right

        // ===== NETWORK =====
        NetworkClient networkClient;
        Map map;
        int playerTankId = -1; // ID của tank của player
        
        // ===== FONT =====
        SpriteFont font;

        // ===== MENU STATE =====
        enum GameState { Menu, Connecting, Playing }
        GameState gameState = GameState.Menu;
        string ipInput = "127.0.0.1";
        string roomCodeInput = "3636";
        int selectedField = 0; // 0 = IP, 1 = Room Code
        KeyboardState prevKeyboard;

        // ===== SPRITES =====
        TextureRegion bulletUp;
        TextureRegion bulletDown;
        TextureRegion bulletLeft;
        TextureRegion bulletRight;
        TextureRegion healthBarSprite;
        TextureRegion[] healthSprite = new TextureRegion[3];
        // ===== INPUT THROTTLE =====
        double inputThrottle = 0.02; // 10ms threshold
        double timeSinceLastInput = 0;

        public Game1() : base("tank", 1280, 720, false) { }

        protected override void LoadContent()
        {
            // ===== FONT =====
            try
            {
                font = Content.Load<SpriteFont>("Arial");
            }
            catch
            {
                font = null; // Nếu không tìm thấy, dùng DrawText fallback
            }
            // ===== HEALTH ===
            Texture2D HealthTex = Content.Load<Texture2D>("asset/powerups_upscayl_16x_remacri-4x");
            TextureAtlas hatlas = new TextureAtlas(HealthTex);
            int hw = HealthTex.Width / 20;
            int hh = HealthTex.Height;
            for (int i=0;i<3;i++)
            {
                hatlas.AddRegion($"h_{i+9}",(i+9)*hw,0,hw,hh);
                healthSprite[i] = hatlas.GetRegion($"h_{i+9}");
            }
            // ===== TANK =====
            tankpic = Content.Load<Texture2D>("asset/light");
            TextureAtlas atlas = new TextureAtlas(tankpic);

            spikeFrames = new TextureRegion[framecount];
            for (int i = 0; i < framecount; i++)
            {
                atlas.AddRegion($"dir_{i}", i * Width, 0, Width, Height);
                spikeFrames[i] = atlas.GetRegion($"dir_{i}");
            }

            // ===== BULLET SPRITE =====
            Texture2D bulletTex = Content.Load<Texture2D>("asset/bullet");
            TextureAtlas batlas = new TextureAtlas(bulletTex);

            int bw = bulletTex.Width / 23;
            int bh = bulletTex.Height;
            batlas.AddRegion("down",  0 * bw, 0, bw, bh);
            batlas.AddRegion("left",  1 * bw, 0, bw, bh);
            batlas.AddRegion("up",    2 * bw, 0, bw, bh);
            batlas.AddRegion("right", 3 * bw, 0, bw, bh);

            bulletDown  = batlas.GetRegion("down");
            bulletLeft  = batlas.GetRegion("left");
            bulletUp    = batlas.GetRegion("up");
            bulletRight = batlas.GetRegion("right");

            // ===== HEALTH BAR SPRITE =====
            Texture2D healthTex = Content.Load<Texture2D>("asset/health_1_upscayl_16x_remacri-4x");
            TextureAtlas healthAtlas = new TextureAtlas(healthTex);
            healthAtlas.AddRegion("health", 11*healthTex.Width/12, 0, healthTex.Height*6, healthTex.Height);
            healthBarSprite = healthAtlas.GetRegion("health");

            // ===== MAP =====
            Texture2D groundTex = Content.Load<Texture2D>("asset/part_2");
            Texture2D wallsTex = Content.Load<Texture2D>("asset/walls");
            int ww = wallsTex.Width / 18;
            int wh = wallsTex.Height ;
            int gw = groundTex.Width / 2;
            int gh = groundTex.Height ;
            TextureAtlas groundAtlas = new TextureAtlas(groundTex);
            groundAtlas.AddRegion("floor", 0*gw, 0, groundTex.Width, groundTex.Height);
            
            TextureAtlas wallsAtlas = new TextureAtlas(wallsTex);
            wallsAtlas.AddRegion("wall", 8*ww, 0, wallsTex.Width, wallsTex.Height);

            // Tạo map 40x22 tiles
            map = new Map(groundAtlas, wallsAtlas, 40, 22);

            // ===== TANK FRAMES MAPPING =====
            tankFrames[0] = spikeFrames[0];   // down
            tankFrames[1] = spikeFrames[4];   // left
            tankFrames[2] = spikeFrames[8];   // up
            tankFrames[3] = spikeFrames[12];  // right

            // Menu sẽ kết nối khi người dùng nhấn Enter
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            KeyboardState k = Keyboard.GetState();
            double dt = gameTime.ElapsedGameTime.TotalSeconds;

            if (gameState == GameState.Menu)
            {
                UpdateMenuInput(k);
            }
            else if (gameState == GameState.Playing)
            {
                // Update playerTankId nếu chưa có - lấy tank có ID lớn nhất (tank của mình vừa tạo)
                if (playerTankId == -1 && networkClient != null && networkClient.CurrentState != null && networkClient.CurrentState.tanks.Count > 0)
                {
                    playerTankId = networkClient.CurrentState.tanks.Max(t => t.id);
                }

                timeSinceLastInput += dt;

                // ===== SEND INPUT TO SERVER (throttled 0.02s) =====
                if (timeSinceLastInput >= inputThrottle)
                {
                    // WASD
                    if (k.IsKeyDown(Keys.W) || k.IsKeyDown(Keys.Up))
                        networkClient.SendInput("1");
                    else if (k.IsKeyDown(Keys.A) || k.IsKeyDown(Keys.Left))
                        networkClient.SendInput("2");
                    else if (k.IsKeyDown(Keys.S) || k.IsKeyDown(Keys.Down))
                        networkClient.SendInput("3");
                    else if (k.IsKeyDown(Keys.D) || k.IsKeyDown(Keys.Right))
                        networkClient.SendInput("4");
                    
                    if (k.IsKeyDown(Keys.Space))
                        networkClient.SendInput("0");

                    timeSinceLastInput = 0;
                }
            }

            prevKeyboard = k;
            base.Update(gameTime);
        }

        void UpdateMenuInput(KeyboardState k)
        {
            // Tab để chuyển field
            if (k.IsKeyDown(Keys.Tab) && !prevKeyboard.IsKeyDown(Keys.Tab))
            {
                selectedField = (selectedField + 1) % 2;
            }

            // Backspace để xóa
            if (k.IsKeyDown(Keys.Back) && !prevKeyboard.IsKeyDown(Keys.Back))
            {
                if (selectedField == 0 && ipInput.Length > 0)
                    ipInput = ipInput.Substring(0, ipInput.Length - 1);
                else if (selectedField == 1 && roomCodeInput.Length > 0)
                    roomCodeInput = roomCodeInput.Substring(0, roomCodeInput.Length - 1);
            }

            // Xử lý input ký tự
            Keys[] pressedKeys = k.GetPressedKeys();
            foreach (Keys key in pressedKeys)
            {
                if (!prevKeyboard.IsKeyDown(key))
                {
                    char c = KeyToChar(key);
                    if (c != '\0')
                    {
                        if (selectedField == 0)
                            ipInput += c;
                        else if (selectedField == 1 && char.IsDigit(c))
                            roomCodeInput += c;
                    }
                }
            }

            // Enter để kết nối
            if (k.IsKeyDown(Keys.Enter) && !prevKeyboard.IsKeyDown(Keys.Enter))
            {
                gameState = GameState.Connecting;
                if (int.TryParse(roomCodeInput, out int port))
                {
                    networkClient = new NetworkClient(ipInput, port);
                    if (networkClient.IsConnected)
                    {
                        gameState = GameState.Playing;
                        // Lấy tank ID của player (tank đầu tiên trong danh sách)
                        if (networkClient.CurrentState != null && networkClient.CurrentState.tanks.Count > 0)
                        {
                            playerTankId = networkClient.CurrentState.tanks[0].id;
                        }
                        Console.WriteLine($"Connected to {ipInput}:{port}");
                    }
                    else
                    {
                        gameState = GameState.Menu;
                        Console.WriteLine("Failed to connect!");
                    }
                }
            }
        }

        char KeyToChar(Keys key)
        {
            // Numbers
            if (key >= Keys.D0 && key <= Keys.D9)
                return (char)('0' + (key - Keys.D0));
            // Letters
            if (key >= Keys.A && key <= Keys.Z)
                return char.ToLower((char)('A' + (key - Keys.A)));
            // Special chars
            if (key == Keys.OemPeriod) return '.';
            if (key == Keys.OemMinus) return '-';
            return '\0';
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

            if (gameState == GameState.Menu)
            {
                DrawMenu();
            }
            else if (gameState == GameState.Connecting)
            {
                DrawText("Đang kết nối...", 640, 360,Color.White);
            }
            else if (gameState == GameState.Playing)
            {
                DrawGameplay();
            }

            SpriteBatch.End();
            base.Draw(gameTime);
        }

        void DrawMenu()
        {
            // Nền
            SpriteBatch.Draw(
                new Texture2D(GraphicsDevice, 1, 1),
                new Rectangle(0, 0, 1280, 720),
                Color.Black
            );

            // Tiêu đề
            DrawText("JOIN GAME ROOM", 640, 80, Color.White, 1f);

            // IP Input
            Color ipColor = selectedField == 0 ? Color.Yellow : Color.White;
            DrawText("IP Address:", 400, 250, Color.White, 1f);
            DrawInputBox(700, 250, ipInput, ipColor);

            // Port Input
            Color codeColor = selectedField == 1 ? Color.Yellow : Color.White;
            DrawText("Port:", 400, 350, Color.White, 1f);
            DrawInputBox(700, 350, roomCodeInput, codeColor);

            // Join Button
            bool isHoveringButton = false; // Có thể mở rộng sau
            Color buttonColor = isHoveringButton ? Color.LimeGreen : Color.White;
            DrawButton(640, 470, "[ENTER] JOIN ROOM", buttonColor);

            // Hướng dẫn
            DrawText("TAB: Switch | BACKSPACE: Delete | ENTER: Connect", 640, 610, Color.White, 1f);
        }
        int healthState=0;
        void DrawGameplay()
        {
            // Vẽ map
            map.Draw(SpriteBatch);

            // Vẽ tanks từ server
            if (networkClient.IsConnected && networkClient.CurrentState != null)
            {
                foreach (var tankData in networkClient.CurrentState.tanks)
                {
                    // Bỏ qua tank nếu hết máu
                    if (tankData.hp <= 0) continue;

                    // Xác định frame dựa trên direction
                    int frameIdx = 0;
                    if (tankData.dirX == 0 && tankData.dirY == -1) frameIdx = 2;      // up
                    else if (tankData.dirX == -1 && tankData.dirY == 0) frameIdx = 1; // left
                    else if (tankData.dirX == 0 && tankData.dirY == 1) frameIdx = 0;  // down
                    else if (tankData.dirX == 1 && tankData.dirY == 0) frameIdx = 3;  // right

                    // Vẽ tank
                    tankFrames[frameIdx].Draw(
                        SpriteBatch,
                        new Vector2(tankData.x, tankData.y),
                        Color.White,
                        0f,
                        Vector2.One,
                        0.1f,
                        SpriteEffects.None,
                        0f
                    );
                    // Vẽ health bar
                    float healthPercent = Math.Max(0, tankData.hp / 100f);
                    // Tank của player: xanh, tank khác: đỏ
                    Color healthColor;
                    if (tankData.id == playerTankId)
                    {
                        // Tank của mình: xanh
                        healthColor = Color.LimeGreen;
                    }
                    else
                    {
                        // Tank khác: đỏ
                        healthColor = Color.Red;
                    }
                    
                    healthBarSprite.Draw(
                        SpriteBatch,
                        new Vector2(tankData.x, tankData.y - 20),
                        healthColor,
                        0f,
                        new Vector2(0.5f, 0),
                        new Vector2(healthPercent / 10, 0.1f),
                        SpriteEffects.None,
                        0f
                    );
                }
                
                // Vẽ bullets từ server
                foreach (var bulletData in networkClient.CurrentState.bullets)
                {
                    // Xác định sprite dựa trên direction
                    TextureRegion bulletSprite = bulletDown;
                    if (bulletData.dirX == 0 && bulletData.dirY == -1) bulletSprite = bulletUp;
                    else if (bulletData.dirX == -1 && bulletData.dirY == 0) bulletSprite = bulletLeft;
                    else if (bulletData.dirX == 0 && bulletData.dirY == 1) bulletSprite = bulletDown;
                    else if (bulletData.dirX == 1 && bulletData.dirY == 0) bulletSprite = bulletRight;

                    bulletSprite.Draw(
                        SpriteBatch,
                        new Vector2(bulletData.x, bulletData.y),
                        Color.White,
                        0f,
                        Vector2.One,
                        0.1f,
                        SpriteEffects.None,
                        0f
                    );
                }
                //vẽ hồi máu
                foreach (var healthData in networkClient.CurrentState.healths)
                {
                    // Xác định sprite dựa trên direction
                    TextureRegion HealthSprite = healthSprite[healthState];
                    healthState+=1;
                    healthState%=3;
                    HealthSprite.Draw(
                        SpriteBatch,
                        new Vector2(healthData.x, healthData.y),
                        Color.White,
                        0f,
                        Vector2.One,
                        0.1f,
                        SpriteEffects.None,
                        0f
                    );
                }
                // Vẽ điểm của player ở góc trên cùng
                if (playerTankId != -1)
                {
                    var playerTank = networkClient.CurrentState.tanks.FirstOrDefault(t => t.id == playerTankId);
                    if (playerTank != null)
                    {
                        DrawText($"SCORE: {playerTank.score}", 100, 30, Color.White, 1f);
                    }
                }
            }
        }
        
        void DrawText(string text, int x, int y, Color color, float scale = 1f)
        {
            if (font != null)
            {
                SpriteBatch.DrawString(font, text, new Vector2(x - font.MeasureString(text).X / 2 * scale, y), color, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
            }
            else
            {
                // Fallback: vẽ text bằng hình vuông nếu không có font
                DrawTextFallback(text, x, y, color);
            }
        }
        
        void DrawTextFallback(string text, int x, int y, Color color)
        {
            // Vẽ text đơn giản bằng pixel
            int charWidth = 8;
            int charHeight = 12;
            int xOffset = x - (text.Length * charWidth / 2);
            
            for (int i = 0; i < text.Length; i++)
            {
                // Vẽ mỗi ký tự như hình chữ nhật màu
                SpriteBatch.Draw(
                    new Texture2D(GraphicsDevice, 1, 1),
                    new Rectangle(xOffset + i * charWidth, y, charWidth, charHeight),
                    color
                );
            }
        }

        void DrawInputBox(int x, int y, string text, Color color)
        {
            // Vẽ hộp input nền
            SpriteBatch.Draw(
                new Texture2D(GraphicsDevice, 1, 1),
                new Rectangle(x - 150, y - 20, 300, 40),
                Color.DarkGray
            );
            
            // Vẽ border
            DrawRectangle(x - 150, y - 20, 300, 40, color);
            
            // Vẽ text trong box
            if (font != null)
            {
                SpriteBatch.DrawString(font, text, new Vector2(x - 140, y - 15), color, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);
            }
            else
            {
                // Fallback: vẽ text đơn giản
                int charWidth = 6;
                for (int i = 0; i < text.Length && i < 30; i++)
                {
                    SpriteBatch.Draw(
                        new Texture2D(GraphicsDevice, 1, 1),
                        new Rectangle(x - 140 + i * charWidth, y - 10, 5, 10),
                        color
                    );
                }
            }
        }
        
        void DrawButton(int x, int y, string text, Color color)
        {
            int width = 250;
            int height = 50;
            
            // Nền button
            SpriteBatch.Draw(
                new Texture2D(GraphicsDevice, 1, 1),
                new Rectangle(x - width / 2, y - height / 2, width, height),
                Color.DarkGreen * 0.7f
            );
            
            // Border button
            DrawRectangle(x - width / 2, y - height / 2, width, height, color);
            
            // Text trên button
            if (font != null)
            {
                Vector2 textSize = font.MeasureString(text);
                SpriteBatch.DrawString(font, text, new Vector2(x - textSize.X / 2, y - textSize.Y / 2), color, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);
            }
            else
            {
                // Fallback: vẽ text đơn giản
                int charWidth = 6;
                int textX = x - (text.Length * charWidth / 2);
                for (int i = 0; i < text.Length; i++)
                {
                    SpriteBatch.Draw(
                        new Texture2D(GraphicsDevice, 1, 1),
                        new Rectangle(textX + i * charWidth, y - 6, 5, 10),
                        color
                    );
                }
            }
        }
        
        void DrawRectangle(int x, int y, int width, int height, Color color)
        {
            // Top
            SpriteBatch.Draw(new Texture2D(GraphicsDevice, 1, 1), new Rectangle(x, y, width, 2), color);
            // Bottom
            SpriteBatch.Draw(new Texture2D(GraphicsDevice, 1, 1), new Rectangle(x, y + height - 2, width, 2), color);
            // Left
            SpriteBatch.Draw(new Texture2D(GraphicsDevice, 1, 1), new Rectangle(x, y, 2, height), color);
            // Right
            SpriteBatch.Draw(new Texture2D(GraphicsDevice, 1, 1), new Rectangle(x + width - 2, y, 2, height), color);
        }
    }
}
