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

        protected override bool StartHidden => true;
        protected override bool BlockPositionalInput => false;

        private readonly SliderPath sliderPath;

        private Path pathLeft = null!;
        private Path pathRight = null!;

        private Path pathCenter = null!;
        private Path pathCenterWide = null!;

        // TODO: remove this jank after we've migrated to .NET 10
        private float progressStartInternal = 0.5f;
        private float progressEndInternal = 0.5f;

        private float progressStart
        {
            get => progressStartInternal;
            set
            {
                progressStartInternal = value;
                Scheduler.AddOnce(recomputePaths);
            }
        }

        private float progressEnd
        {
            get => progressEndInternal;
            set
            {
                progressEndInternal = value;
                Scheduler.AddOnce(recomputePaths);
            }
        }

        public RankedPlayBottomOrnament()
        {
            const int top = 2; // to account for the middle segment being twice as wide
            const int bottom = 10;
            const int curve_smoothness = 5;

            const int left_start = 0;
            const int left_corner = 10;
            const int left_end = 20;
            var diagonalDirLeft = (new Vector2(left_start, bottom) - new Vector2(left_corner, top)).Normalized();

            const float right_start = width;
            const float right_corner = right_start - 10;
            const float right_end = right_start - 20;
            var diagonalDirRight = (new Vector2(right_start, bottom) - new Vector2(right_corner, top)).Normalized();

            sliderPath = new SliderPath(new[]
            {
                new PathControlPoint(new Vector2(left_start, bottom), PathType.BEZIER),
                new PathControlPoint(new Vector2(left_corner, top) + diagonalDirLeft * curve_smoothness),
                new PathControlPoint(new Vector2(left_corner, top)),
                new PathControlPoint(new Vector2(left_end, top), PathType.LINEAR),
                new PathControlPoint(new Vector2(right_end, top), PathType.BEZIER),
                new PathControlPoint(new Vector2(right_corner, top)),
                new PathControlPoint(new Vector2(right_corner, top) + diagonalDirRight * curve_smoothness),
                new PathControlPoint(new Vector2(right_start, bottom)),
            });
        }

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
            List<Vector2> vertices = new List<Vector2>();
            sliderPath.GetPathToProgress(vertices, progressStart, progressEnd);

            if (progressStart >= 0.15 && progressEnd <= 0.85)
                pathCenterWide.Vertices = vertices;

            if (progressStart >= 0.075 && progressEnd <= 0.925)
                pathCenter.Vertices = vertices;

            if (progressStart <= 0.05)
            {
                List<Vector2> verticesLeft = new List<Vector2>();
                sliderPath.GetPathToProgress(verticesLeft, progressStart, 0.05);
                pathLeft.Vertices = verticesLeft;
            }

            if (progressEnd >= 0.95)
            {
                List<Vector2> verticesRight = new List<Vector2>();
                sliderPath.GetPathToProgress(verticesRight, 0.95, progressEnd);
                pathRight.Vertices = verticesRight;
            }
        }

        private const int duration = 1200;
        private const Easing easing = Easing.OutExpo;

        protected override void PopIn()
        {
            this.FadeIn(duration, easing)
                .TransformTo(nameof(progressStart), 0f, duration, easing)
                .TransformTo(nameof(progressEnd), 1f, duration, easing);
        }

        protected override void PopOut()
        {
            this.FadeOut(duration, easing)
                .TransformTo(nameof(progressStart), 0.5f, duration, easing)
                .TransformTo(nameof(progressEnd), 0.5f, duration, easing);
        }
    }
}
