#region Using Statements
using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;
using DemoTest.Game.ScreenManagers;
using DemoTest.Game.Screens;
#endregion

namespace DemoTest 
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class DemoTest : Microsoft.Xna.Framework.Game
    {

        ScreenManager screenManager;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Vector2 baseScreenSize = new Vector2(800, 600);

        SaveLoadData load = new SaveLoadData();
         
        private SpriteFont hudFont;
        private SpriteFont gameFont;

        private int levelIndex = -1;        

        private const int numberOfLevels = 3;
        private Level level;

        private GamePadState gamePadState;
        private KeyboardState keyboardState;
        private KeyboardState oldState;

        public DemoTest()
            : base()
        {                        
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.IsFullScreen = false;
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;

            screenManager = new ScreenManager(this);
            Components.Add(screenManager);
            Components.Add(new GamerServicesComponent(this));

            //Activate first screens
            screenManager.AddScreen(new BackgroundScreen(), null);
            screenManager.AddScreen(new MainMenuScreenX(), null);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            Global.isPaused = true;
            Global.newGame = true;
            Global.continueGame = false;
            Global.sound = 100;
            Global.music = 100;
            oldState = Keyboard.GetState();
            load.InitiateLoadOptions();
            load.InitiateLoadPlayer();            
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            hudFont = Content.Load<SpriteFont>("Fonts/Hud");
            gameFont = Content.Load<SpriteFont>("Fonts/gamefont");

            LoadNextLevel();
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (!Global.isPaused)
            {
                HandleInput();
                // update our level, passing down the GameTime along with all of our input states
                level.Update(gameTime, keyboardState, gamePadState);

                // TODO: Add your update logic here
                
            }
            base.Update(gameTime);
        }

        private void HandleInput()
        {

            // get all of our input states
            if (Global.isPaused == false)
            {
                keyboardState = Keyboard.GetState();
                gamePadState = GamePad.GetState(PlayerIndex.One);

                if (gamePadState.Buttons.Start == ButtonState.Pressed ||
                keyboardState.IsKeyDown(Keys.Escape))
                {
                    if (!oldState.IsKeyDown(Keys.Escape))
                    {
                        if (Global.isPaused == false)
                        {
                            Global.isPaused = true;
                            screenManager.AddScreen
                                (new PauseMenuScreen
                                    (   level.Player.hitPoints,
                                        level.Player.maxHitPoints,
                                        level.Player.str,
                                        level.Player.dex,
                                        level.Player.vit,
                                        level.Player.Position,
                                        levelIndex), null);
                        }
                    }
                }
                else if (oldState.IsKeyDown(Keys.Escape))
                {
                    
                }

                if (gamePadState.Buttons.Back == ButtonState.Pressed)
                    Exit();

                if (!level.Player.IsAlive)
                {
                    if (keyboardState.IsKeyDown(Keys.Space))
                    {
                        ReloadCurrentLevel();
                    }
                }

                // If starting a new game
                if (Global.newGame)
                {
                    levelIndex = -1;
                    LoadNextLevel();
                    Global.newGame = false;
                }

                // If loading a saved game
                if (Global.continueGame)
                {
                    levelIndex = Global.levelIndex;
                    LoadNextLevel();                    
                    level.Player.hitPoints = Global.hp;
                    level.Player.maxHitPoints = Global.maxHp;
                    level.Player.str = Global.str;
                    level.Player.dex = Global.dex;
                    level.Player.vit = Global.vit;
                    level.Player.Position = Global.position;                    
                    Global.continueGame = false;
                }

                // Level finished, initiate new level
                if (level.ReachedExit)
                    LoadNextLevel();

                oldState = keyboardState;
            }
        }

        private void LoadNextLevel()
        {
            // move to the next level
            levelIndex = (levelIndex + 1) % numberOfLevels;

            // Unloads the content for the current level before loading the next one.
            if (level != null)
                level.Dispose();

            // Load the level.
            string levelPath = string.Format("Content/Levels/{0}.txt", levelIndex);
            using (Stream fileStream = TitleContainer.OpenStream(levelPath))
                level = new Level(Services, fileStream, levelIndex);
        }

        private void ReloadCurrentLevel()
        {
            --levelIndex;
            LoadNextLevel();           
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            Vector3 screenScalingFactor;

            float horScaling = (float)GraphicsDevice.PresentationParameters.BackBufferWidth / baseScreenSize.X;
            float verScaling = (float)GraphicsDevice.PresentationParameters.BackBufferHeight / baseScreenSize.Y;
            screenScalingFactor = new Vector3(horScaling, verScaling, 1);
            Matrix globalTransformation = Matrix.CreateScale(screenScalingFactor);

            // TODO: Add your drawing code here            
            
            level.Draw(gameTime, spriteBatch);
            DrawHud();
            
            base.Draw(gameTime);
        }

        private void DrawHud()
        {
            spriteBatch.Begin();
            Rectangle titleSafeArea = GraphicsDevice.Viewport.TitleSafeArea;
            Vector2 hudLocation = new Vector2(titleSafeArea.X, titleSafeArea.Y);

            Vector2 center = new Vector2(baseScreenSize.X / 2, baseScreenSize.Y / 2);

            
            DrawShadowedString(hudFont, "Player Position X: " + level.Player.Position.X.ToString(), hudLocation + new Vector2(0.0f, 1f * 1.2f), Color.Yellow);
            DrawShadowedString(hudFont, "Player Position Y: " + level.Player.Position.Y.ToString(), hudLocation + new Vector2(0.0f, 15f * 1.2f), Color.Yellow);
            DrawShadowedString(hudFont, "Camera Position X: " + level.cameraPositionX.ToString(), hudLocation + new Vector2(0.0f, 30f * 1.2f), Color.Yellow);
            DrawShadowedString(hudFont, "Camera Position Y: " + level.cameraPositionY.ToString(), hudLocation + new Vector2(0.0f, 45f * 1.2f), Color.Yellow);
            DrawShadowedString(hudFont, "Alive: " + level.Player.IsAlive.ToString(), hudLocation + new Vector2(0.0f, 60f * 1.2f), Color.Yellow);
            DrawShadowedString(hudFont, "HP: " + level.Player.hitPoints, hudLocation + new Vector2(0.0f, 75f * 1.2f), Color.Yellow);
            
            spriteBatch.End();
        }

        private void DrawShadowedString(SpriteFont font, string value, Vector2 position, Color color)
        {
            spriteBatch.DrawString(font, value, position + new Vector2(1.0f, 1.0f), Color.Black);
            spriteBatch.DrawString(font, value, position, color);
        }
    }
}