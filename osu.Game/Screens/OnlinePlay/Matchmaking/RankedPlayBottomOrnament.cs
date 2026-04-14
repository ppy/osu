// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Lines;
using osu.Framework.Layout;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking
{
    /// <summary>
    /// A small component intended to be always present at the bottom of all ranked play screens
    /// to indicate a ranked play session is in progress.
    /// </summary>
    public partial class RankedPlayBottomOrnament : OverlayContainer
    {
        private const int width = 400;
        private const int height = 24;

        protected override bool BlockPositionalInput => false;

        private Path pathLeft = null!;
        private Path pathRight = null!;

        private Path pathCenter = null!;
        private Path pathCenterWide = null!;

        private readonly LayoutValue layout = new LayoutValue(Invalidation.DrawSize);

        protected override bool StartHidden => true;

        [BackgroundDependencyLoader]
        private void load()
        {
            Width = width;
            Height = height;
            Alpha = 0;

            Masking = true;

            EdgeEffect = new EdgeEffectParameters
            {
                Colour = Color4Extensions.FromHex("#15061e").Opacity(0.8f),
                Type = EdgeEffectType.Glow,
                Radius = height * 2,
                Roundness = height * 2,
                Offset = new Vector2(0, height / 2f),
            };

            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 10,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Children = new Drawable[]
                    {
                        pathLeft = new SmoothPath
                        {
                            AutoSizeAxes = Axes.None,
                            RelativeSizeAxes = Axes.Both,
                            PathRadius = 1,
                        },
                        pathCenter = new SmoothPath
                        {
                            AutoSizeAxes = Axes.None,
                            RelativeSizeAxes = Axes.Both,
                            PathRadius = 1,
                        },
                        pathCenterWide = new SmoothPath
                        {
                            AutoSizeAxes = Axes.None,
                            RelativeSizeAxes = Axes.Both,
                            PathRadius = 2,
                        },
                        pathRight = new SmoothPath
                        {
                            AutoSizeAxes = Axes.None,
                            RelativeSizeAxes = Axes.Both,
                            PathRadius = 1,
                        },
                    },
                },
                new OsuSpriteText
                {
                    Y = 4,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Font = OsuFont.Torus.With(size: 10, weight: FontWeight.Bold),
                    Spacing = new Vector2(3, 0),
                    Text = ButtonSystemStrings.RankedPlay.ToUpper(),
                },
            };
        }

        private void recomputePaths()
        {
            const int top = 2; // to account for the middle segment being twice as wide
            const int bottom = 10;
            const int curve_smoothness = 5;

            pathCenter.AddVertex(new Vector2(30, top));
            pathCenter.AddVertex(new Vector2(DrawWidth - 30, top));

            pathCenterWide.AddVertex(new Vector2(60, top));
            pathCenterWide.AddVertex(new Vector2(DrawWidth - 60, top));

            const int left_start = 0;
            const int left_corner = 10;
            const int left_end = 20;

            List<Vector2> vertices = new List<Vector2>();
            var diagonalDirLeft = (new Vector2(left_start, bottom) - new Vector2(left_corner, top)).Normalized();

            var sliderPathLeft = new SliderPath(new[]
            {
                new PathControlPoint(new Vector2(left_start, bottom), PathType.LINEAR),
                new PathControlPoint(new Vector2(left_corner, top) + diagonalDirLeft * curve_smoothness, PathType.BEZIER),
                new PathControlPoint(new Vector2(left_corner, top)),
                new PathControlPoint(new Vector2(left_end, top), PathType.LINEAR),
            });

            sliderPathLeft.GetPathToProgress(vertices, 0.0, 1.0);
            pathLeft.Vertices = vertices;

            float rightStart = DrawWidth;
            float rightCorner = rightStart - 10;
            float rightEnd = rightStart - 20;

            var diagonalDirRight = (new Vector2(rightStart, bottom) - new Vector2(rightCorner, top)).Normalized();
            var sliderPathRight = new SliderPath(new[]
            {
                new PathControlPoint(new Vector2(rightStart, bottom), PathType.LINEAR),
                new PathControlPoint(new Vector2(rightCorner, top) + diagonalDirRight * curve_smoothness, PathType.BEZIER),
                new PathControlPoint(new Vector2(rightCorner, top)),
                new PathControlPoint(new Vector2(rightEnd, top), PathType.LINEAR),
            });

            sliderPathRight.GetPathToProgress(vertices, 0.0, 1.0);
            pathRight.Vertices = vertices;
        }

        protected override void Update()
        {
            base.Update();

            if (!layout.IsValid)
            {
                recomputePaths();
                layout.Validate();
            }
        }

        protected override void PopIn()
        {
            this.FadeIn(500, Easing.OutQuint);
            // TODO: animate this better.
        }

        protected override void PopOut()
        {
            this.FadeOut(500, Easing.OutQuint);
        }
    }
}
