// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Mania.Objects.Drawables.Pieces
{
    public class LaneGlowPiece : CompositeDrawable, IHasAccentColour
    {
        private const float total_height = 100;
        private const float glow_height = 50;
        private const float glow_alpha = 0.4f;
        private const float edge_alpha = 0.3f;

        public LaneGlowPiece()
        {
            BypassAutoSizeAxes = Axes.Both;
            RelativeSizeAxes = Axes.X;
            Height = total_height;

            InternalChildren = new[]
            {
                new Container
                {
                    Name = "Left edge",
                    RelativeSizeAxes = Axes.Y,
                    Width = 1,
                    Children = createGradient(edge_alpha)
                },
                new Container
                {
                    Name = "Right edge",
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    RelativeSizeAxes = Axes.Y,
                    Width = 1,
                    Children = createGradient(edge_alpha)
                },
                new Container
                {
                    Name = "Glow",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    Height = glow_height,
                    Children = createGradient(glow_alpha)
                }
            };
        }

        private Drawable[] createGradient(float alpha) => new Drawable[]
        {
            new Box
            {
                Name = "Top",
                RelativeSizeAxes = Axes.Both,
                Height = 0.5f,
                Blending = BlendingParameters.Additive,
                Colour = ColourInfo.GradientVertical(Color4.Transparent, Color4.White.Opacity(alpha))
            },
            new Box
            {
                Name = "Bottom",
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
                RelativeSizeAxes = Axes.Both,
                Height = 0.5f,
                Blending = BlendingParameters.Additive,
                Colour = ColourInfo.GradientVertical(Color4.White.Opacity(alpha), Color4.Transparent)
            }
        };

        public Color4 AccentColour
        {
            get => Colour;
            set => Colour = value;
        }
    }
}
