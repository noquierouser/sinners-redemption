using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace DemoTest
{
    public static class Global
    {
        public static bool isPaused {get; set;}
        public static bool newGame { get; set; }
        public static bool continueGame { get; set; }
        public static int sound { get; set; }
        public static int music { get; set; }

        // Status of the game for save data
        public static int hp;
        public static int maxHp;
        public static int str;
        public static int dex;
        public static int vit;
        public static int levelIndex;
        public static Vector2 position;        
    }
}