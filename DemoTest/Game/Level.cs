using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;

namespace DemoTest
{
    public class Level
    {
        public List<Message> messages;

        private Tile[,] tiles;
        TimeSpan MaxAgeMessage = new TimeSpan(0, 0, 0, 0, 500);

        private Layer[] layers;
        private const int EntityLayer = 2;

        public ContentManager Content
        {
            get { return content; }
        }
        ContentManager content;

        public Player Player
        {
            get { return player; }
        }
        Player player;

        public bool ReachedExit
        {
            get { return reachedExit; }
        }
        bool reachedExit;

        // Key locations in the level.        
        private Vector2 start;
        private Point exit = InvalidPosition;
        private static readonly Point InvalidPosition = new Point(-1, -1);

        // Level game state
        public float cameraPositionX;
        public float cameraPositionY;

        public List<Enemy> enemies = new List<Enemy>();
        

        public Level(IServiceProvider serviceProvider, Stream fileStream, int levelIndex)
        {
            // Create a new content manager to load content used just by this level.
            content = new ContentManager(serviceProvider, "Content");           

            LoadTiles(fileStream);

            messages = new List<Message>();
            layers = new Layer[1];
            layers[0] = new Layer(Content, "Backgrounds/Layer4", 0.6f);
        }

        private void LoadTiles(Stream fileStream)
        {
            // Load the level and ensure all of the lines are the same length.
            int width;
            List<string> lines = new List<string>();
            using (StreamReader reader = new StreamReader(fileStream))
            {
                string line = reader.ReadLine();
                width = line.Length;
                while (line != null)
                {
                    lines.Add(line);
                    if (line.Length != width)
                        throw new Exception(String.Format("The length of line {0} is different from all preceeding lines.", lines.Count));
                    line = reader.ReadLine();
                }
            }

            // Allocate the tile grid.
            tiles = new Tile[width, lines.Count];

            // Loop over every tile position,
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    // to load each tile.
                    char tileType = lines[y][x];
                    tiles[x, y] = LoadTile(tileType, x, y);
                }
            }

        }

        private Tile LoadTile(char tileType, int x, int y)
        {
            switch (tileType) {
                #region Stage Build Blocks
                // Blank space
                case '.':
                    return new Tile(null, TileCollision.Passable);

                // Ground block
                case '#':
                    return LoadTile("scenery/hard_ground", TileCollision.Impassable);

                #region Building
                case 'O':
                    return LoadTile("building/01_top_walk", TileCollision.Platform);

                case 'T':
                    return LoadTile("building/02_sustain", TileCollision.Passable);

                case 'I':
                    return LoadTile("building/03_middle", TileCollision.Passable);

                case 'K':
                    return LoadTile("building/04_from_ground", TileCollision.Passable);

                case 'L':
                    return LoadTile("building/05_bottom", TileCollision.Passable);
                #endregion

                #region Stage elements
                // Hard blocks
                case 'H':
                    return LoadTile("scenery/hard_crate", TileCollision.Impassable);

                // Boxes
                case 'C':
                    return LoadTile("scenery/soft_crate", TileCollision.Platform);
                #endregion
                #endregion

                #region Spawn points and Teleports
                // Player 1 start point
                case '1':
                    return LoadStartTile(x, y);

                // Enemy A spawn point
                case 'A':
                    return LoadEnemyTile(x, y, "EnemyA");

                case 'B':
                    return LoadEnemyTile(x, y, "EnemyB");

                case 'D':
                    return LoadEnemyTile(x, y, "EnemyC");
                
                // Exit
                case 'X':
                    return LoadExitTile(x, y);
                #endregion

                // Unknown tile type character
                default:
                    throw new NotSupportedException(String.Format("Unsupported tile type character '{0}' at position {1}, {2}.", tileType, x, y));
            }
        }

        private Tile LoadTile(string name, TileCollision collision)
        {
            name = "Tiles/" + name;
            return new Tile(Content.Load<Texture2D>(name), collision);
        }

        private Tile LoadStartTile(int x, int y)
        {
            if (Player != null)
                throw new NotSupportedException("A level may only have one starting point.");

            start = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
            player = new Player(this, start);

            return new Tile(null, TileCollision.Passable);
        }

        /// <summary>
        /// Instantiates an enemy and puts him in the level.
        /// </summary>
        private Tile LoadEnemyTile(int x, int y, string spriteSet)
        {
            Vector2 position = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
            enemies.Add(new Enemy(this, position, spriteSet));

            return new Tile(null, TileCollision.Passable);
        }

        private Tile LoadExitTile(int x, int y)
        {
            if (exit != InvalidPosition)
                throw new NotSupportedException("A level may only have one exit.");

            exit = GetBounds(x, y).Center;

            return new Tile(null, TileCollision.Passable);
        }

        public void Dispose()
        {
            Content.Unload();
        }

        public int Width
        {
            get { return tiles.GetLength(0); }
        }

        public int Height
        {
            get { return tiles.GetLength(1); }
        }

        public TileCollision GetCollision(int x, int y)
        {
            // Prevent escaping past the level ends.
            if (x < 0 || x >= Width)
                return TileCollision.Impassable;
            // Allow jumping past the level top and falling through the bottom.
            if (y < 0 || y >= Height)
                return TileCollision.Passable;

            return tiles[x, y].Collision;
        }

        public Rectangle GetBounds(int x, int y)
        {
            return new Rectangle(x * Tile.Width, y * Tile.Height, Tile.Width, Tile.Height);
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, SpriteFont dmgFont)
        {
            spriteBatch.Begin();
            layers[0].Draw(spriteBatch, cameraPositionX, cameraPositionY);
            spriteBatch.End();

            ScrollCamera(spriteBatch.GraphicsDevice.Viewport);
            Matrix cameraTransform = Matrix.CreateTranslation(-cameraPositionX, -cameraPositionY, 0.0f);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null, cameraTransform);

            DrawTiles(spriteBatch);
            Player.Draw(gameTime, spriteBatch);

            foreach (Enemy enemy in enemies)
                if(enemy.isAlive || enemy.deathTime > 0)
                    enemy.Draw(gameTime, spriteBatch);

            foreach (var message in messages)
            {
                spriteBatch.DrawString(dmgFont, message.Text, message.Position, message.MessageColor, 0.0f, new Vector2(), 0.5f, SpriteEffects.None, 0);
            }

            spriteBatch.End();

            spriteBatch.Begin();
            for (int i = EntityLayer + 1; i < layers.Length; ++i)
                layers[i].Draw(spriteBatch, cameraPositionX, cameraPositionY);
            spriteBatch.End();

            
        }

        public void Update(
            GameTime gameTime,
            KeyboardState keyboardState,
            GamePadState gamePadState)
        {            
            Player.Update(gameTime, keyboardState, gamePadState);
            UpdateEnemies(gameTime);            

            if (Player.IsAlive &&
                   Player.IsOnGround &&
                   Player.BoundingRectangle.Contains(exit))
            {
                OnExitReached();
            }

            // Falling off the bottom of the level kills the player.
            if (Player.BoundingRectangle.Top >= Height * Tile.Height && Player.IsAlive)
            {
                OnPlayerKilled();
            }

            foreach (var message in messages)
            {
                message.Position.Y = message.Position.Y - 1;
            }

            while (messages.Count > 0 &&
                   messages[0].Appeared + MaxAgeMessage < gameTime.TotalGameTime)
                messages.RemoveAt(0);
            
        }

        public void UpdateEnemies(GameTime gameTime)
        {
            foreach (Enemy enemy in enemies)
            {
                enemy.Update(gameTime);
                int playerDmg;
                int enemyDmg;

                Vector2 pos = player.position;
                pos.Y = pos.Y - 90;

                // Enemy damages player
                // Touching an enemy depletes the player hitpoints
                if (enemy.BoundingRectangle.Intersects(Player.BoundingRectangle))
                {
                    if (enemy.isAlive)
                    {
                        if (!player.Invulnerable && !player.isAttacking)
                        {
                            enemyDmg = enemy.str - player.vit;
                            if (enemyDmg <= 0)
                                enemyDmg = 1;
                            player.hitPoints = player.hitPoints - enemyDmg;
                            if (player.hitPoints < 0)
                                player.hitPoints = 0;

                            messages.Add(new Message() 
                            { 
                                Text = enemyDmg.ToString(),
                                Appeared = gameTime.TotalGameTime,
                                Position = pos,
                                MessageColor = Color.Red
                            });

                            player.Invulnerable = true;
                        }
                    }
                }

                if (enemy.isAlive && enemy.BoundingRectangle.Intersects(Player.MeleeRectangle))
                {
                    if (Player.isAttacking && !enemy.Invulnerable)
                    {
                        playerDmg = player.str - enemy.vit;
                        if (playerDmg <= 0)
                            playerDmg = 0;
                        enemy.hitPoints = enemy.hitPoints - playerDmg;
                        if (enemy.hitPoints < 0)
                            enemy.hitPoints = 0;

                        messages.Add(new Message()
                        {
                            Text = playerDmg.ToString(),
                            Appeared = gameTime.TotalGameTime,
                            Position = pos,
                            MessageColor = Color.White
                        });

                        enemy.Invulnerable = true;
                    }
                }
            }
        }

        private void OnEnemyKilled(Enemy enemy, Player killedBy)
        {
            enemy.OnKilled(killedBy);
        }

        private void DrawTiles(SpriteBatch spriteBatch)
        {
            int left = (int)Math.Floor(cameraPositionX / Tile.Width);
            int right = left + spriteBatch.GraphicsDevice.Viewport.Width / Tile.Width;
            right = Math.Min(right, Width - 1);

            // For each tile position
            for (int y = 0; y < Height; ++y)
            {
                for (int x = left; x <= right; ++x)
                {
                    // If there is a visible tile in that position
                    Texture2D texture = tiles[x, y].Texture;
                    if (texture != null)
                    {
                        // Draw it in screen space.
                        Vector2 position = new Vector2(x, y) * Tile.Size;
                        spriteBatch.Draw(texture, position, Color.White);
                    }
                }
            }
        }

        private void ScrollCamera(Viewport viewport)
        {    
            
            float cameraMovementX = 0.0f;
            float cameraMovementY = 0.0f;
            const float ViewMarginX = 0.5f;

            // Calculate the edges of the screen.
            float marginWidth = viewport.Width * ViewMarginX;
            float marginLeft = cameraPositionX + marginWidth;
            float marginRight = cameraPositionX + viewport.Width - marginWidth;

            const float ViewMarginTop = 0.5f;
            const float ViewMarginBottom = 0.5f;

            float marginTop = cameraPositionY + viewport.Height * ViewMarginTop;
            float marginBottom = cameraPositionY + viewport.Height - viewport.Height * ViewMarginBottom;

            // Calculate how far to scroll when the player is near the edges of the screen.
            
            if (Player.Position.X < marginLeft)
                cameraMovementX = Player.Position.X - marginLeft;
            else if (Player.Position.X > marginRight)
                cameraMovementX = Player.Position.X - marginRight;

            float maxCameraPositionX;
            maxCameraPositionX = Tile.Width * Width - viewport.Width;
            cameraPositionX = MathHelper.Clamp(cameraPositionX + cameraMovementX, 0.0f, maxCameraPositionX);

            
            if (Player.Position.Y < marginTop)
                cameraMovementY = player.Position.Y - marginTop;
            else if (player.Position.Y > marginBottom)
                cameraMovementY = player.Position.Y - marginBottom;

            float maxCameraPositionY;
            maxCameraPositionY = Tile.Height * Height - viewport.Height;
            cameraPositionY = MathHelper.Clamp(cameraPositionY + cameraMovementY, 0.0f, maxCameraPositionY);
        }

        private void OnPlayerKilled()
        {
            Player.OnKilled();
        }

        private void OnExitReached()
        {
            reachedExit = true;
        }

    }
}
