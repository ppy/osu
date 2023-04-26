// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Taiko.UI
{
    public partial class PlayfieldBackgroundRight : CompositeDrawable
    {
        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Name = "Transparent playfield background";
            RelativeSizeAxes = Axes.Both;
            Masking = true;
            BorderColour = colours.Gray1;

            EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Colour = Color4.Black.Opacity(0.2f),
                Radius = 5,
            };

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colours.Gray0,
                    Alpha = 0.6f
                },
                new Container
                {
                    Name = "Border",
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    MaskingSmoothness = 0,
                    BorderThickness = 2,
                    AlwaysPresent = true,
                    Children = new[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                            AlwaysPresent = true
                        }
                    }
                }
            };
        }
    }
}
