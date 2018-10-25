using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using OpenTK.Graphics;

namespace osu.Core.Containers.SymcolToolbar
{
    public class ToolbarSystemClock : Container
    {
        public ToolbarSystemClock()
        {
            AutoSizeAxes = Axes.X;
            Add(new SystemClock());
        }

        private class SystemClock : Container
        {
            private static DateTime t = DateTime.Now;
            private string time = t.ToString("hh:mm:ss tt");
            SpriteText clockText;

            public SystemClock()
            {
                clockText = new SpriteText
                {
                    Font = @"Exo2.0-Medium",
                    Text = time,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.Centre,
                    Position = new OpenTK.Vector2(-70, 20),
                    TextSize = 28,
                    Colour = Color4.White,
                };
                Add(clockText);
            }

            protected override void Update()
            {
                t = DateTime.Now;
                time = t.ToString("hh:mm:ss tt");
                clockText.Text = time;
            }
        }
    }
}
