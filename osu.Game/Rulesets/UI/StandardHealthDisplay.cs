// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.UI
{
    public class StandardHealthDisplay : HealthDisplay, IHasAccentColour
    {
        private readonly Container fill;

        public Color4 AccentColour
        {
            get { return fill.Colour; }
            set { fill.Colour = value; }
        }

        private Color4 glowColour;
        public Color4 GlowColour
        {
            get { return glowColour; }
            set
            {
                if (glowColour == value)
                    return;
                glowColour = value;

                fill.EdgeEffect = new EdgeEffect
                {
                    Colour = glowColour,
                    Radius = 8,
                    Type = EdgeEffectType.Glow
                };
            }
        }

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

        protected override void SetHealth(float value) => fill.ScaleTo(new Vector2(value, 1), 200, EasingTypes.OutQuint);
    }
}
