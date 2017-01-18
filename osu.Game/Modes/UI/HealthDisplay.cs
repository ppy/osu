using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using System;

namespace osu.Game.Modes.UI
{
    public class HealthDisplay : Container
    {
        private Box background;
        private Box fill;

        public HealthDisplay()
        {
            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Gray,
                },
                fill = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.White,
                    Scale = new Vector2(0, 1),
                }, 
            };
        }

        public double Current;

        public void Set(double value)
        {
            Current = value;
            fill.ScaleTo(new Vector2((float)Current, 1), 200, EasingTypes.OutQuint);
        }
    }
}
