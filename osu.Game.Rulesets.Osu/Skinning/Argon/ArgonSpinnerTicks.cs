// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Argon
{
    public partial class ArgonSpinnerTicks : CompositeDrawable
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;

            const float count = 25;

            for (float i = 0; i < count; i++)
            {
                AddInternal(new CircularContainer
                {
                    RelativePositionAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 5,
                    BorderColour = Color4.White,
                    BorderThickness = 2f,
                    Size = new Vector2(30, 5),
                    Origin = Anchor.Centre,
                    Position = new Vector2(
                        0.5f + MathF.Sin(i / count * 2 * MathF.PI) / 2 * 0.75f,
                        0.5f + MathF.Cos(i / count * 2 * MathF.PI) / 2 * 0.75f
                    ),
                    Rotation = -i / count * 360 - 120,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Colour = Colour4.White.Opacity(0.2f),
                        Radius = 30,
                    },
                    Children = new[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                            AlwaysPresent = true,
                        }
                    }
                });
            }
        }
    }
}
