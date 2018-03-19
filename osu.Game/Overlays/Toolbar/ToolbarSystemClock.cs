using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using System;

namespace osu.Game.Overlays.Toolbar
{
    class ToolbarSystemClock : Container
    {
        private SystemClock time;

        public ToolbarSystemClock()
        {
            AutoSizeAxes = Axes.X;
            Add(time = new SystemClock());
        }
    }
    public class SystemClock : Container
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