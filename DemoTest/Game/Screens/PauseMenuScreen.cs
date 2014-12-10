#region File Description
//-----------------------------------------------------------------------------
// PauseMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using System.Collections.Generic;
using DemoTest.Game;
#endregion

namespace DemoTest.Game.Screens
{
    /// <summary>
    /// The pause menu comes up over the top of the game,
    /// giving the player options to resume or quit.
    /// </summary>
    class PauseMenuScreen : MenuScreen
    {
        #region Initialization

        SaveLoadData save = new SaveLoadData();
        
        int hitPoints;
        int maxHitPoints;
        int str;
        int dex;
        int vit;        
        Vector2 position;
        int levelIndex;
        Vector2[] enemies;
        bool[] aliveEnemies;
        Song music;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public PauseMenuScreen
            (
                int hp,
                int hpMax,
                int str,
                int dex,
                int vit,
                Vector2 position,
                int levelIndex,
                Vector2[] enemies,
                bool[] aliveEnemies
            )
            : base("Paused")
        {
            this.hitPoints = hp;
            this.maxHitPoints = hpMax;
            this.str = str;
            this.dex = dex;
            this.vit = vit;
            this.position = position;
            this.levelIndex = levelIndex;
            this.enemies = enemies;
            this.aliveEnemies = aliveEnemies;

            // Create our menu entries.
            MenuEntry resumeGameMenuEntry = new MenuEntry("Resume Game");
            MenuEntry quitGameMenuEntry = new MenuEntry("Save & Exit");
            
            // Hook up menu event handlers.
            resumeGameMenuEntry.Selected += OnCancel;
            quitGameMenuEntry.Selected += QuitGameMenuEntrySelected;

            // Add entries to the menu.
            MenuEntries.Add(resumeGameMenuEntry);
            MenuEntries.Add(quitGameMenuEntry);
        }


        #endregion
        
        #region Handle Input

        protected override void OnCancel(PlayerIndex playerIndex)
        {
            ExitScreen();
            Global.isPaused = false;
        }

        /// <summary>
        /// Event handler for when the Quit Game menu entry is selected.
        /// </summary>
        void QuitGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            const string message = "Are you sure you want to save & exit?";

            MessageBoxScreen confirmQuitMessageBox = new MessageBoxScreen(message);

            confirmQuitMessageBox.Accepted += ConfirmQuitMessageBoxAccepted;

            ScreenManager.AddScreen(confirmQuitMessageBox, ControllingPlayer);
        }


        /// <summary>
        /// Event handler for when the user selects ok on the "are you sure
        /// you want to quit" message box. This uses the loading screen to
        /// transition from the game back to the main menu screen.
        /// </summary>
        void ConfirmQuitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
        {
            Global.hp = hitPoints;
            Global.maxHp = maxHitPoints;
            Global.str = str;
            Global.dex = dex;
            Global.vit = vit;
            Global.position = position;
            Global.levelIndex = levelIndex-1;
            Global.enemies = enemies;
            Global.aliveEnemy = aliveEnemies;
            save.InitiateSavePlayer();            
            LoadingScreen.Load(ScreenManager, false, null, new BackgroundScreen(),
                                                           new MainMenuScreenX());
        }


        #endregion
    }
}
