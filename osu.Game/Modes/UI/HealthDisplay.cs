// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Game.Graphics;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Modes.UI
{
    public class HealthDisplay : Container
    {
        private Box background;
        private Container fill;

        public BindableDouble Current = new BindableDouble()
        {
            MinValue = 0,
            MaxValue = 1
        };

        public HealthDisplay()
        {
            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                },
                fill = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Scale = new Vector2(0, 1),
                    Masking = true,
                    Children = new[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        }
                    }
                },
            };

            Current.ValueChanged += current_ValueChanged;
        }

        [BackgroundDependencyLoader]
        private void laod(OsuColour colours)
        {
            fill.Colour = colours.BlueLighter;
            fill.EdgeEffect = new EdgeEffect
            {
                Colour = colours.BlueDarker.Opacity(0.6f),
                Radius = 8,
                Type=  EdgeEffectType.Glow
            };
        }

        private void current_ValueChanged(object sender, EventArgs e)
        {
            fill.ScaleTo(new Vector2((float)Current, 1), 200, EasingTypes.OutQuint);
        }
    }
}
