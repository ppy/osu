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

            const float count = 18;

            for (float i = 0; i < count; i++)
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
                        0.5f + MathF.Sin(i / count * 2 * MathF.PI) / 2 * 0.86f,
                        0.5f + MathF.Cos(i / count * 2 * MathF.PI) / 2 * 0.86f
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
    }
}
