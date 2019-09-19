// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Pieces
{
    public class SpinnerTicks : Container
    {
        public SpinnerTicks()
        {
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;

            const int count = 18;

            for (int i = 0; i < count; i++)
            {
                Add(new Container
                {
                    Colour = Color4.Black,
                    Alpha = 0.4f,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Glow,
                        Radius = 10,
                        Colour = Color4.Gray.Opacity(0.2f),
                    },
                    RelativePositionAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 5,
                    Size = new Vector2(60, 10),
                    Origin = Anchor.Centre,
                    Position = new Vector2(
                        0.5f + (float)Math.Sin((float)i / count * 2 * MathHelper.Pi) / 2 * 0.86f,
                        0.5f + (float)Math.Cos((float)i / count * 2 * MathHelper.Pi) / 2 * 0.86f
                    ),
                    Rotation = -(float)i / count * 360 + 90,
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
    }
}
