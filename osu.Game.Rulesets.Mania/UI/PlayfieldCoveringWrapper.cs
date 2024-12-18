﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.ComponentModel;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK;
using osuTK.Graphics;
using Container = osu.Framework.Graphics.Containers.Container;

namespace osu.Game.Rulesets.Mania.UI
{
    /// <summary>
    /// A <see cref="Framework.Graphics.Containers.Container"/> that has its contents partially hidden by an adjustable "cover". This is intended to be used in a playfield.
    /// </summary>
    public partial class PlayfieldCoveringWrapper : CompositeDrawable
    {
        /// <summary>
        /// The relative area that should be completely covered. This does not include the fade.
        /// </summary>
        public readonly BindableFloat Coverage = new BindableFloat();

        /// <summary>
        /// The complete cover, including gradient and fill.
        /// </summary>
        private readonly Drawable cover;

        /// <summary>
        /// The gradient portion of the cover.
        /// </summary>
        private readonly Box gradient;

        /// <summary>
        /// The fully-opaque portion of the cover.
        /// </summary>
        private readonly Box filled;

        private readonly IBindable<ScrollingDirection> scrollDirection = new Bindable<ScrollingDirection>();

        private float currentCoverageHeight;

        public PlayfieldCoveringWrapper(Drawable content)
        {
            InternalChild = new BufferedContainer
            {
                RelativeSizeAxes = Axes.Both,
                Children = new[]
                {
                    content,
                    cover = new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Blending = new BlendingParameters
                        {
                            // Don't change the destination colour.
                            RGBEquation = BlendingEquation.Add,
                            Source = BlendingType.Zero,
                            Destination = BlendingType.One,
                            // Subtract the cover's alpha from the destination (points with alpha 1 should make the destination completely transparent).
                            AlphaEquation = BlendingEquation.Add,
                            SourceAlpha = BlendingType.Zero,
                            DestinationAlpha = BlendingType.OneMinusSrcAlpha
                        },
                        Children = new Drawable[]
                        {
                            gradient = new Box
                            {
                                Anchor = Anchor.BottomLeft,
                                Origin = Anchor.BottomLeft,
                                RelativeSizeAxes = Axes.Both,
                                RelativePositionAxes = Axes.Both,
                                Height = 0.25f,
                                Colour = ColourInfo.GradientVertical(
                                    Color4.White.Opacity(0f),
                                    Color4.White.Opacity(1f)
                                )
                            },
                            filled = new Box
                            {
                                Anchor = Anchor.BottomLeft,
                                Origin = Anchor.BottomLeft,
                                RelativeSizeAxes = Axes.Both,
                                Height = 0
                            }
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(IScrollingInfo scrollingInfo)
        {
            scrollDirection.BindTo(scrollingInfo.Direction);
            scrollDirection.BindValueChanged(onScrollDirectionChanged, true);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateCoverSize(true);
        }

        protected override void Update()
        {
            base.Update();
            updateCoverSize(false);
        }

        private void updateCoverSize(bool instant)
        {
            float targetCoverage;
            float targetAlpha;

            if (instant)
            {
                targetCoverage = Coverage.Value;
                targetAlpha = Coverage.Value > 0 ? 1 : 0;
            }
            else
            {
                targetCoverage = (float)Interpolation.DampContinuously(currentCoverageHeight, Coverage.Value, 25, Math.Abs(Time.Elapsed));
                targetAlpha = (float)Interpolation.DampContinuously(gradient.Alpha, Coverage.Value > 0 ? 1 : 0, 25, Math.Abs(Time.Elapsed));
            }

            filled.Height = GetHeight(targetCoverage);
            gradient.Y = -GetHeight(targetCoverage);
            gradient.Alpha = targetAlpha;

            currentCoverageHeight = targetCoverage;
        }

        protected virtual float GetHeight(float coverage) => coverage;

        private void onScrollDirectionChanged(ValueChangedEvent<ScrollingDirection> direction)
            => cover.Rotation = direction.NewValue == ScrollingDirection.Up ? 0 : 180f;

        /// <summary>
        /// The direction in which the cover expands.
        /// </summary>
        public CoverExpandDirection Direction
        {
            set => cover.Scale = value == CoverExpandDirection.AlongScroll ? Vector2.One : new Vector2(1, -1);
        }
    }

    public enum CoverExpandDirection
    {
        /// <summary>
        /// The cover expands along the scrolling direction.
        /// </summary>
        [Description("Along scroll")]
        AlongScroll,

        /// <summary>
        /// The cover expands against the scrolling direction.
        /// </summary>
        [Description("Against scroll")]
        AgainstScroll
    }
}
