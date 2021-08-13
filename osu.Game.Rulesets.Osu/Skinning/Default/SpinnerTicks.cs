// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Default
{
    public class SpinnerTicks : Container, IHasAccentColour
    {
        public SpinnerTicks()
        {
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;

            const float count = 8;

            for (float i = 0; i < count; i++)
            {
                Add(new Container
                {
                    Alpha = 0.4f,
                    Blending = BlendingParameters.Additive,
                    RelativePositionAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 5,
                    Size = new Vector2(60, 10),
                    Origin = Anchor.Centre,
                    Position = new Vector2(
                        0.5f + MathF.Sin(i / count * 2 * MathF.PI) / 2 * 0.83f,
                        0.5f + MathF.Cos(i / count * 2 * MathF.PI) / 2 * 0.83f
                    ),
                    Rotation = -i / count * 360 + 90,
                    Children = new[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        }
                    }
                });
            }
        }

        public Color4 AccentColour
        {
            get => Colour;
            set
            {
                Colour = value;

                foreach (var c in Children.OfType<Container>())
                {
                    c.EdgeEffect =
                        new EdgeEffectParameters
                        {
                            Type = EdgeEffectType.Glow,
                            Radius = 20,
                            Colour = value.Opacity(0.8f),
                        };
                }
            }
        }
    }
}
