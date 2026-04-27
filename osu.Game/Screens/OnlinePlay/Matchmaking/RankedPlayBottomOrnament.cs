// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Lines;
using osu.Framework.Graphics.Shapes;
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

        private Circle centerLine = null!;
        private Circle centerLineThick = null!;

        private readonly Bindable<float> progressBindable = new Bindable<float>();

        protected float Progress
        {
            get => progressBindable.Value;
            set => progressBindable.Value = value;
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
                            Origin = Anchor.BottomRight,
                            PathRadius = 1,
                        },
                        centerLine = new Circle
                        {
                            RelativeSizeAxes = Axes.X,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Y = 1,
                            Height = 2,
                        },
                        centerLineThick = new Circle
                        {
                            RelativeSizeAxes = Axes.X,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Height = 4,
                        },
                        pathRight = new SmoothPath
                        {
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

        protected override void LoadComplete()
        {
            base.LoadComplete();
            progressBindable.BindValueChanged(progress => recomputePaths(progress.NewValue), true);
        }

        private readonly List<Vector2> vertices = new List<Vector2>();

        private void recomputePaths(float newProgress)
        {
            centerLineThick.Width = Math.Clamp(newProgress, 0f, 0.7f);
            centerLine.Width = Math.Clamp(newProgress, 0f, 0.85f);

            if (newProgress > 0.9f)
            {
                pathLeft.Alpha = 1;
                pathRight.Alpha = 1;

                vertices.Clear();
                sliderPath.GetPathToProgress(vertices, 0.5f - newProgress * 0.5f, 0.05f);

                Vector2 lastVertex = vertices[^1];
                Vector2 firstVertex = vertices[0];
                for (int i = 0; i < vertices.Count; i++)
                    vertices[i] -= firstVertex;

                pathLeft.Vertices = vertices;
                pathLeft.Position = pathLeft.PositionInBoundingBox(lastVertex);

                vertices.Clear();
                sliderPath.GetPathToProgress(vertices, 0.95f, 0.5f + newProgress * 0.5f);

                firstVertex = vertices[0];
                for (int i = 0; i < vertices.Count; i++)
                    vertices[i] -= firstVertex;

                pathRight.Vertices = vertices;
                pathRight.Position = pathRight.PositionInBoundingBox(firstVertex) - new Vector2(pathRight.PathRadius * 2);
            }
            else
            {
                pathLeft.Alpha = 0;
                pathRight.Alpha = 0;
            }
        }

        private const int duration = 1200;
        private const Easing easing = Easing.OutExpo;

        protected override void PopIn()
        {
            this.MoveToY(5)
                .Delay(550)
                .MoveToY(0, duration - 550, easing);

            this.FadeIn(duration, easing)
                .TransformTo(nameof(Progress), 1f, duration, easing);
        }

        protected override void PopOut()
        {
            this.MoveToY(5, duration / 2f, Easing.In);

            this.FadeOut(duration, easing)
                .TransformTo(nameof(Progress), 0f, duration, easing);
        }
    }
}
