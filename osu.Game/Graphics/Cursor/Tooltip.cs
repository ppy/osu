// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Graphics.Cursor
{
    public class Tooltip : Container
    {
        private readonly Box tooltipBackground;
        private readonly OsuSpriteText text;

        public string TooltipText {
            get
            {
                return text.Text;
            }
            set
            {
                text.Text = value;
                if (string.IsNullOrEmpty(value))
                    Hide();
                else
                    Show();
            }
        }

        public Tooltip()
        {
            Children = new[]
            {
                new Container
                {
                    AutoSizeAxes = Axes.Both,
                    CornerRadius = 5,
                    Masking = true,
                    AlwaysPresent = true,
                    EdgeEffect = new EdgeEffect
                    {
                        Type = EdgeEffectType.Shadow,
                        Colour = Color4.Black.Opacity(40),
                        Radius = 5,
                    },
                    Children = new Drawable[]
                    {
                        tooltipBackground = new Box
                        {
                            RelativeSizeAxes = Axes.Both
                        },
                        text = new OsuSpriteText
                        {
                            Padding = new MarginPadding(3),
                            Font = @"Exo2.0-Regular",
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            tooltipBackground.Colour = colour.Gray3;
        }
    }
}
