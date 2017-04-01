// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;

namespace osu.Game.Modes.UI
{
    public class StandardHealthDisplay : HealthDisplay
    {
        private readonly Container fill;

        public StandardHealthDisplay()
        {
            Children = new Drawable[]
            {
                new Box
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
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            fill.Colour = colours.BlueLighter;
            fill.EdgeEffect = new EdgeEffect
            {
                Colour = colours.BlueDarker.Opacity(0.6f),
                Radius = 8,
                Type = EdgeEffectType.Glow
            };
        }

        protected override void SetHealth(float value) => fill.ScaleTo(new Vector2(value, 1), 200, EasingTypes.OutQuint);
    }
}
