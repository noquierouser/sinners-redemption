#region File Description
//-----------------------------------------------------------------------------
// Enemy.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace DemoTest
{
    /// <summary>
    /// Facing direction along the X axis.
    /// </summary>
    enum FaceDirection
    {
        Left = -1,
        Right = 1,
    }

    /// <summary>
    /// A monster who is impeding the progress of our fearless adventurer.
    /// </summary>
    public class Enemy
    {
        public int hitPoints;
        public int maxHitPoints;
        public int str;
        public int dex;
        public int vit;

        private SoundEffect killedSound;

        public Level Level
        {
            get { return level; }
        }
        Level level;

        /// <summary>
        /// Position in world space of the bottom center of this enemy.
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
            set { position = value;  }
        }
        Vector2 position;

        public bool Invulnerable
        {
            get { return invulnerable; }
            set { invulnerable = value; }
        }
        bool invulnerable;
        double invulTime;

        private Rectangle localBounds;
        /// <summary>
        /// Gets a rectangle which bounds this enemy in world space.
        /// </summary>
        public Rectangle BoundingRectangle
        {
            get
            {
                int left = (int)Math.Round(Position.X - sprite.Origin.X) + localBounds.X;
                int top = (int)Math.Round(Position.Y - sprite.Origin.Y) + localBounds.Y;

                return new Rectangle(left, top, localBounds.Width, localBounds.Height);
            }
        }

        // Animations
        private Animation runAnimation;
        private Animation idleAnimation;
        private Animation deathAnimation;
        private AnimationPlayer sprite;

        public bool isAlive { get; set; }
        private const float deathTimeMax = 1.0f;
        public float deathTime = deathTimeMax;

        /// <summary>
        /// The direction this enemy is facing and moving along the X axis.
        /// </summary>
        private FaceDirection direction = FaceDirection.Left;

        /// <summary>
        /// How long this enemy has been waiting before turning around.
        /// </summary>
        private float waitTime;

        /// <summary>
        /// How long to wait before turning around.
        /// </summary>
        private const float MaxWaitTime = 0.5f;

        /// <summary>
        /// The speed at which this enemy moves along the X axis.
        /// </summary>
        private const float MoveSpeed = 84.0f;

        /// <summary>
        /// Constructs a new Enemy.
        /// </summary>
        public Enemy(Level level, Vector2 position, string spriteSet)
        {
            this.level = level;
            this.position = position;
            this.isAlive = true;
            LoadContent(spriteSet);
            if (spriteSet == "EnemyA")
            {
                this.maxHitPoints = 3;
                this.hitPoints = this.maxHitPoints;
                this.str = 5;
                this.dex = 0;
                this.vit = 0;
            }
            if (spriteSet == "EnemyB")
            {
                this.maxHitPoints = 10;
                this.hitPoints = this.maxHitPoints;
                this.str = 25;
                this.dex = 0;
                this.vit = 2;
            }
            if (spriteSet == "EnemyC")
            {
                this.maxHitPoints = 8;
                this.str = 10;
                this.dex = 0;
                this.vit = 100;
            }
        }

        /// <summary>
        /// Loads a particular enemy sprite sheet and sounds.
        /// </summary>
        public void LoadContent(string spriteSet)
        {
            // Load animations.
            spriteSet = "Sprites/Enemies/" + spriteSet + "/";
            runAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "walk.fw"), 0.1f, true);
            idleAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Idle"), 0.15f, true);
            deathAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "dying.fw"), 0.15f, false);
            sprite.PlayAnimation(idleAnimation);

            // Calculate bounds within texture size.
            int width = (int)(idleAnimation.FrameWidth * 0.35);
            int left = (idleAnimation.FrameWidth - width) / 2;
            int height = (int)(idleAnimation.FrameWidth * 0.7);
            int top = idleAnimation.FrameHeight - height;
            localBounds = new Rectangle(left, top, width, height);

            killedSound = Level.Content.Load<SoundEffect>("Sounds/death");
        }


        /// <summary>
        /// Paces back and forth along a platform, waiting at either end.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (!isAlive)
                deathTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (hitPoints == 0 && isAlive)
                OnKilled();

            // Calculate tile position based on the side we are walking towards.
            float posX = Position.X + localBounds.Width / 2 * (int)direction;
            int tileX = (int)Math.Floor(posX / Tile.Width) - (int)direction;
            int tileY = (int)Math.Floor(Position.Y / Tile.Height);

            if (waitTime > 0 && isAlive)
            {
                // Wait for some amount of time.
                waitTime = Math.Max(0.0f, waitTime - (float)gameTime.ElapsedGameTime.TotalSeconds);
                if (waitTime <= 0.0f)
                {
                    // Then turn around.
                    direction = (FaceDirection)(-(int)direction);
                }
            }
            else
            {
                if (isAlive)
                {
                    // If we are about to run into a wall or off a cliff, start waiting.
                    if (Level.GetCollision(tileX + (int)direction, tileY - 1) == TileCollision.Impassable ||
                        Level.GetCollision(tileX + (int)direction, tileY - 2) == TileCollision.Impassable ||
                        Level.GetCollision(tileX + (int)direction, tileY) == TileCollision.Passable)
                    {
                        waitTime = MaxWaitTime;
                    }
                    else
                    {
                        // Move in the current direction.
                        Vector2 velocity = new Vector2((int)direction * MoveSpeed * elapsed, 0.0f);
                        position = position + velocity;
                    }
                }
            }

            // Invulnerability time
            if (gameTime.TotalGameTime.TotalMilliseconds >= invulTime)
            {
                invulnerable = false;
                invulTime = gameTime.TotalGameTime.TotalMilliseconds + 1000;
            }

            if (invulnerable)
                waitTime = MaxWaitTime;
        }

        /// <summary>
        /// Draws the animated enemy.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (Global.isPaused && level.Player.IsAlive)
                sprite.PlayAnimation(idleAnimation);
            else
            {
                if (deathTime < deathTimeMax)
                    sprite.PlayAnimation(deathAnimation);
                // Stop running when the game is paused or before turning around.
                else if (waitTime > 0)
                {
                    sprite.PlayAnimation(idleAnimation);

                }
                else
                {
                    sprite.PlayAnimation(runAnimation);
                }
            }

            // Draw facing the way the enemy is moving.
            SpriteEffects flip = direction < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            sprite.Draw(gameTime, spriteBatch, Position, flip);
        }

        public void OnKilled()
        {
            isAlive = false;
            killedSound.Play(Global.sound/10,0f,0f);
        }
    }
}
