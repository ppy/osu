// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Backgrounds;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// A loading spinner.
    /// </summary>
    public partial class LoadingSpinner : VisibilityContainer
    {
        public const float TRANSITION_DURATION = 500;

        private readonly SpriteIcon spinner;

        protected override bool StartHidden => true;

        protected Container MainContents;

        private readonly TrianglesV2 triangles;

        private readonly Container? trianglesMasking;

        private readonly bool withBox;

        private const float spin_duration = 900;

        /// <summary>
        /// Constuct a new loading spinner.
        /// </summary>
        /// <param name="withBox">Whether the spinner should have a surrounding black box for visibility.</param>
        /// <param name="inverted">Whether colours should be inverted (black spinner instead of white).</param>
        public LoadingSpinner(bool withBox = false, bool inverted = false)
        {
            this.withBox = withBox;

            Size = new Vector2(60);

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            if (withBox)
            {
                Child = MainContents = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 20,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = inverted ? Color4.White : Color4.Black,
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0.7f,
                        },
                        triangles = new TrianglesV2
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = inverted ? Color4.White : Color4.Black,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Alpha = 0.2f,
                            ScaleAdjust = 0.4f,
                            Velocity = 0.8f,
                            SpawnRatio = 2
                        },
                        spinner = new SpriteIcon
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Colour = inverted ? Color4.Black : Color4.White,
                            Scale = new Vector2(0.6f),
                            RelativeSizeAxes = Axes.Both,
                            Icon = FontAwesome.Solid.CircleNotch
                        }
                    }
                };
            }
            else
            {
                Children = new[]
                {
                    MainContents = new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            spinner = new SpriteIcon
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Colour = inverted ? Color4.Black : Color4.White,
                                RelativeSizeAxes = Axes.Both,
                                Icon = FontAwesome.Solid.CircleNotch
                            }
                        }
                    },
                    trianglesMasking = new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Size = new Vector2(0.8f),
                        Masking = true,
                        CornerRadius = 20,
                        Children = new Drawable[]
                        {
                            triangles = new TrianglesV2
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Alpha = 0.4f,
                                Colour = ColourInfo.GradientVertical(
                                    inverted ? Color4.Black.Opacity(0) : Color4.White.Opacity(0),
                                    inverted ? Color4.Black : Color4.White),
                                RelativeSizeAxes = Axes.Both,
                                ScaleAdjust = 0.4f,
                                SpawnRatio = 4,
                            },
                        }
                    },
                };
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            rotate();
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            if (withBox)
            {
                MainContents.CornerRadius = MainContents.DrawWidth / 4;
                triangles.Rotation = -MainContents.Rotation;
            }
            else
            {
                Debug.Assert(trianglesMasking != null);
                trianglesMasking.CornerRadius = MainContents.DrawWidth / 2;
            }
        }

        protected override void PopIn()
        {
            if (Alpha < 0.5f)
                // reset animation if the user can't see us.
                rotate();

            MainContents.ScaleTo(1, TRANSITION_DURATION, Easing.OutQuint);
            this.FadeIn(TRANSITION_DURATION, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            MainContents.ScaleTo(0.6f, TRANSITION_DURATION, Easing.OutQuint);
            this.FadeOut(TRANSITION_DURATION / 2, Easing.OutQuint);
        }

        private void rotate()
        {
            spinner.Spin(spin_duration * 3.5f, RotationDirection.Clockwise);

            MainContents.RotateTo(0).Then()
                        .RotateTo(90, spin_duration, Easing.InOutQuart).Then()
                        .RotateTo(180, spin_duration, Easing.InOutQuart).Then()
                        .RotateTo(270, spin_duration, Easing.InOutQuart).Then()
                        .RotateTo(360, spin_duration, Easing.InOutQuart).Then()
                        .Loop();
        }
    }
}
