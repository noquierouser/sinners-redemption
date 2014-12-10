using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DemoTest
{
    public class Message
    {
        public string Text { get; set; }
        public TimeSpan Appeared { get; set; }
        public Vector2 Position;
        public Color MessageColor { get; set; }
    }
}