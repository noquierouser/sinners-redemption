#region File Description
//-----------------------------------------------------------------------------
// OptionsMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;
using DemoTest.Game.ScreenManagers;
#endregion

namespace DemoTest.Game.Screens
{
    /// <summary>
    /// The options screen is brought up over the top of the main menu
    /// screen, and gives the user a chance to configure the game
    /// in various hopefully useful ways.
    /// </summary>
    class OptionsMenuScreen : MenuScreen
    {
        #region Fields

        MenuEntry musicMenuEntry;
        MenuEntry soundMenuEntry;

        SaveLoadData save = new SaveLoadData();

        #endregion

        #region Initialization
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public OptionsMenuScreen()
            : base("Options")
        {
            // Create our menu entries.
            soundMenuEntry = new MenuEntry(string.Empty);
            musicMenuEntry = new MenuEntry(string.Empty);
            
            SetMenuEntryText();

            MenuEntry back = new MenuEntry("Back");

            // Hook up menu event handlers.
            soundMenuEntry.Left += SoundMenuEntryLeft;
            soundMenuEntry.Right += SoundMenuEntryRight;
            musicMenuEntry.Left += MusicMenuEntryLeft;
            musicMenuEntry.Right += MusicMenuEntryRight;
            back.Selected += OnCancel;
            
            // Add entries to the menu.
            MenuEntries.Add(soundMenuEntry);
            MenuEntries.Add(musicMenuEntry);
            MenuEntries.Add(back);
        }    

        /// <summary>
        /// Fills in the latest values for the options screen menu text.
        /// </summary>
        void SetMenuEntryText()
        {
            soundMenuEntry.Text = "Sound Volume: " + Global.sound;
            musicMenuEntry.Text = "Music Volume: " + Global.music;                        
        }


        #endregion

        #region Handle Input

        protected override void OnCancel(PlayerIndex playerIndex)
        {
            save.InitiateSaveOptions();                  
            ExitScreen();
        }

        void SoundMenuEntryLeft(object sender, PlayerIndexEventArgs e)
        {
            
            if (Global.sound > 0)
                Global.sound--;
            
            SetMenuEntryText();
        }

        void SoundMenuEntryRight(object sender, PlayerIndexEventArgs e)
        {
            if (Global.sound < 10)
                Global.sound++;

            SetMenuEntryText();
        }

        void MusicMenuEntryLeft(object sender, PlayerIndexEventArgs e)
        {
            if (Global.music > 0)
                Global.music--;

            SetMenuEntryText();
        }

        void MusicMenuEntryRight(object sender, PlayerIndexEventArgs e)
        {
            if (Global.music < 10)
                Global.music++;

            SetMenuEntryText();
        }

        #endregion
    }
}
