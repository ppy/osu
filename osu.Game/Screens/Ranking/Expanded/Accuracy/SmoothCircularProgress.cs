// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osuTK;

namespace osu.Game.Screens.Ranking.Expanded.Accuracy
{
    /// <summary>
    /// Contains a <see cref="CircularProgress"/> with smoothened edges.
    /// </summary>
    public class SmoothCircularProgress : CompositeDrawable
    {
        public Bindable<double> Current
        {
            get => progress.Current;
            set => progress.Current = value;
        }

        public float InnerRadius
        {
            get => progress.InnerRadius;
            set
            {
                progress.InnerRadius = value;
                innerSmoothingContainer.Size = new Vector2(1 - value);
                smoothingWedge.Height = value / 2;
            }
        }

        private readonly CircularProgress progress;
        private readonly Container innerSmoothingContainer;
        private readonly Drawable smoothingWedge;

        public SmoothCircularProgress()
        {
            Container smoothingWedgeContainer;

            InternalChild = new BufferedContainer
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    progress = new CircularProgress { RelativeSizeAxes = Axes.Both },
                    smoothingWedgeContainer = new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Child = smoothingWedge = new Box
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.Y,
                            Width = 1f,
                            EdgeSmoothness = new Vector2(2, 0),
                        }
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding(-1),
                        Child = new CircularContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            BorderThickness = 2,
                            Masking = true,
                            BorderColour = OsuColour.Gray(0.5f).Opacity(0.75f),
                            Blending = new BlendingParameters
                            {
                                AlphaEquation = BlendingEquation.ReverseSubtract,
                            },
                            Child = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Alpha = 0,
                                AlwaysPresent = true
                            }
                        }
                    },
                    innerSmoothingContainer = new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Size = Vector2.Zero,
                        Padding = new MarginPadding(-1),
                        Child = new CircularContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            BorderThickness = 2,
                            BorderColour = OsuColour.Gray(0.5f).Opacity(0.75f),
                            Masking = true,
                            Blending = new BlendingParameters
                            {
                                AlphaEquation = BlendingEquation.ReverseSubtract,
                            },
                            Child = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Alpha = 0,
                                AlwaysPresent = true
                            }
                        }
                    },
                }
            };

            Current.BindValueChanged(c =>
            {
                smoothingWedgeContainer.Alpha = c.NewValue > 0 ? 1 : 0;
                smoothingWedgeContainer.Rotation = (float)(360 * c.NewValue);
            }, true);
        }

        public TransformSequence<CircularProgress> FillTo(double newValue, double duration = 0, Easing easing = Easing.None)
            => progress.FillTo(newValue, duration, easing);
    }
}
