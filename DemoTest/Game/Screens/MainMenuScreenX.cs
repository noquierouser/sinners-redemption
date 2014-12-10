#region File Description
//-----------------------------------------------------------------------------
// MainMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using DemoTest.Game.ScreenManagers;
#endregion

namespace DemoTest.Game.Screens
{
    /// <summary>
    /// The main menu screen is the first thing displayed when the game starts up.
    /// </summary>
    class MainMenuScreenX : MenuScreen
    {
        #region Initialization
        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public MainMenuScreenX()
            : base("Main Menu")
        {
            // Create our menu entries.
            MenuEntry playGameMenuEntry = new MenuEntry("New Game");
            MenuEntry continueGameMenuEntry = new MenuEntry("Continue Game");
            MenuEntry optionsMenuEntry = new MenuEntry("Options");
            MenuEntry exitMenuEntry = new MenuEntry("Exit");

            // Hook up menu event handlers.
            playGameMenuEntry.Selected += PlayGameMenuEntrySelected;
            continueGameMenuEntry.Selected += ContinueGameMenuEntrySelected;
            optionsMenuEntry.Selected += OptionsMenuEntrySelected;
            exitMenuEntry.Selected += OnCancel;

            // Add entries to the menu.
            MenuEntries.Add(playGameMenuEntry);
            MenuEntries.Add(continueGameMenuEntry);
            MenuEntries.Add(optionsMenuEntry);
            MenuEntries.Add(exitMenuEntry);
            
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Event handler for when the Play Game menu entry is selected.
        /// </summary>
        void PlayGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            LoadingScreen.Load(ScreenManager, true, e.PlayerIndex, null);
            Global.isPaused = false;
            Global.newGame = true;
        }

        void ContinueGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            if (Global.saveExists)
            {
                LoadingScreen.Load(ScreenManager, true, e.PlayerIndex, null);
                Global.isPaused = false;
                Global.newGame = false;
                Global.continueGame = true;
            }
            else
            {
                const string message = "No savedata found. Start a new game.";

                MessageBoxScreen noData = new MessageBoxScreen(message, false);

                ScreenManager.AddScreen(noData, null);
            }
        }


        /// <summary>
        /// Event handler for when the Options menu entry is selected.
        /// </summary>
        void OptionsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.AddScreen(new OptionsMenuScreen(), e.PlayerIndex);
        }


        /// <summary>
        /// When the user cancels the main menu, ask if they want to exit the sample.
        /// </summary>
        protected override void OnCancel(PlayerIndex playerIndex)
        {
            const string message = "Are you sure you want to exit this sample?";

            MessageBoxScreen confirmExitMessageBox = new MessageBoxScreen(message);

            confirmExitMessageBox.Accepted += ConfirmExitMessageBoxAccepted;

            ScreenManager.AddScreen(confirmExitMessageBox, playerIndex);
        }


        /// <summary>
        /// Event handler for when the user selects ok on the "are you sure
        /// you want to exit" message box.
        /// </summary>
        void ConfirmExitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.Game.Exit();
        }


        #endregion
    }
}
