// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Skinning.Argon
{
    internal class ArgonBananaPiece : ArgonFruitPiece
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(new UprightAspectMaintainingContainer
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children = new Drawable[]
                {
                    new Circle
                    {
                        Colour = Color4.White.Opacity(0.4f),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Blending = BlendingParameters.Additive,
                        Size = new Vector2(8),
                        Scale = new Vector2(30, 1),
                    },
                    new Box
                    {
                        Colour = ColourInfo.GradientHorizontal(Color4.White.Opacity(0), Color4.White),
                        RelativeSizeAxes = Axes.X,
                        Blending = BlendingParameters.Additive,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.CentreRight,
                        Width = 1.6f,
                        Height = 2,
                    },
                    new Box
                    {
                        Colour = ColourInfo.GradientHorizontal(Color4.White, Color4.White.Opacity(0)),
                        RelativeSizeAxes = Axes.X,
                        Blending = BlendingParameters.Additive,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.CentreLeft,
                        Width = 1.6f,
                        Height = 2,
                    },
                    new Circle
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(1.2f),
                        EdgeEffect = new EdgeEffectParameters
                        {
                            Type = EdgeEffectType.Glow,
                            Hollow = false,
                            Colour = Color4.White.Opacity(0.1f),
                            Radius = 50,
                        },
                        Child =
                        {
                            Alpha = 0,
                            AlwaysPresent = true,
                        },
                        BorderColour = Color4.White.Opacity(0.1f),
                        BorderThickness = 3,
                    },
                }
            });
        }
    }
}
