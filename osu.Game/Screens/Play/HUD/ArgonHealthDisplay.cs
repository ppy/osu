// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;
using osu.Framework.Layout;
using osu.Framework.Threading;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    public partial class ArgonHealthDisplay : HealthDisplay, ISerialisableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        [SettingSource("Bar height")]
        public BindableFloat BarHeight { get; } = new BindableFloat(20)
        {
            MinValue = 0,
            MaxValue = 64,
            Precision = 1
        };

        [SettingSource("Use relative size")]
        public BindableBool UseRelativeSize { get; } = new BindableBool(true);

        private BarPath mainBar = null!;

        /// <summary>
        /// Used to show a glow at the end of the main bar, or red "damage" area when missing.
        /// </summary>
        private BarPath glowBar = null!;

        private BackgroundPath background = null!;

        private SliderPath barPath = null!;

        private static readonly Colour4 main_bar_colour = Colour4.White;
        private static readonly Colour4 main_bar_glow_colour = Color4Extensions.FromHex("#7ED7FD").Opacity(0.5f);

        private ScheduledDelegate? resetMissBarDelegate;

        private readonly List<Vector2> missBarVertices = new List<Vector2>();
        private readonly List<Vector2> healthBarVertices = new List<Vector2>();

        private double glowBarValue;

        public double GlowBarValue
        {
            get => glowBarValue;
            set
            {
                if (glowBarValue == value)
                    return;

                glowBarValue = value;
                Scheduler.AddOnce(updatePathVertices);
            }
        }

        private double healthBarValue;

        public double HealthBarValue
        {
            get => healthBarValue;
            set
            {
                if (healthBarValue == value)
                    return;

                healthBarValue = value;
                Scheduler.AddOnce(updatePathVertices);
            }
        }

        public const float MAIN_PATH_RADIUS = 10f;

        private const float curve_start_offset = 70;
        private const float curve_end_offset = 40;
        private const float padding = MAIN_PATH_RADIUS * 2;
        private const float curve_smoothness = 10;

        private readonly LayoutValue drawSizeLayout = new LayoutValue(Invalidation.DrawSize);

        public ArgonHealthDisplay()
        {
            AddLayout(drawSizeLayout);

            // sane default width specification.
            // this only matters if the health display isn't part of the default skin
            // (in which case width will be set to 300 via `ArgonSkin.GetDrawableComponent()`),
            // and if the user hasn't applied their own modifications
            // (which are applied via `SerialisedDrawableInfo.ApplySerialisedInfo()`).
            Width = 0.98f;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Y;

            InternalChild = new Container
            {
                AutoSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    background = new BackgroundPath
                    {
                        PathRadius = MAIN_PATH_RADIUS,
                    },
                    glowBar = new BarPath
                    {
                        BarColour = Color4.White,
                        GlowColour = main_bar_glow_colour,
                        Blending = BlendingParameters.Additive,
                        Colour = ColourInfo.GradientHorizontal(Color4.White.Opacity(0.8f), Color4.White),
                        PathRadius = 40f,
                        // Kinda hacky, but results in correct positioning with increased path radius.
                        Margin = new MarginPadding(-30f),
                        GlowPortion = 0.9f,
                    },
                    mainBar = new BarPath
                    {
                        AutoSizeAxes = Axes.None,
                        RelativeSizeAxes = Axes.Both,
                        Blending = BlendingParameters.Additive,
                        BarColour = main_bar_colour,
                        GlowColour = main_bar_glow_colour,
                        PathRadius = MAIN_PATH_RADIUS,
                        GlowPortion = 0.6f,
                    },
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindValueChanged(_ => Scheduler.AddOnce(updateCurrent), true);

            // we're about to set `RelativeSizeAxes` depending on the value of `UseRelativeSize`.
            // setting `RelativeSizeAxes` internally transforms absolute sizing to relative and back to keep the size the same,
            // but that is not what we want in this case, since the width at this point is valid in the *target* sizing mode.
            // to counteract this, store the numerical value here, and restore it after setting the correct initial relative sizing axes.
            float previousWidth = Width;
            UseRelativeSize.BindValueChanged(v => RelativeSizeAxes = v.NewValue ? Axes.X : Axes.None, true);
            Width = previousWidth;

            BarHeight.BindValueChanged(_ => updatePath(), true);
        }

        private void updateCurrent()
        {
            if (Current.Value >= GlowBarValue) finishMissDisplay();

            double time = Current.Value > GlowBarValue ? 500 : 250;

            // TODO: this should probably use interpolation in update.
            this.TransformTo(nameof(HealthBarValue), Current.Value, time, Easing.OutQuint);
            if (resetMissBarDelegate == null) this.TransformTo(nameof(GlowBarValue), Current.Value, time, Easing.OutQuint);
        }

        protected override void Update()
        {
            base.Update();

            if (!drawSizeLayout.IsValid)
            {
                updatePath();
                drawSizeLayout.Validate();
            }

            mainBar.Alpha = (float)Interpolation.DampContinuously(mainBar.Alpha, Current.Value > 0 ? 1 : 0, 40, Time.Elapsed);
            glowBar.Alpha = (float)Interpolation.DampContinuously(glowBar.Alpha, GlowBarValue > 0 ? 1 : 0, 40, Time.Elapsed);
        }

        protected override void Flash()
        {
            base.Flash();

            mainBar.TransformTo(nameof(BarPath.GlowColour), main_bar_glow_colour.Opacity(0.8f))
                   .TransformTo(nameof(BarPath.GlowColour), main_bar_glow_colour, 300, Easing.OutQuint);

            if (resetMissBarDelegate == null)
            {
                glowBar.TransformTo(nameof(BarPath.BarColour), Colour4.White, 30, Easing.OutQuint)
                       .Then()
                       .TransformTo(nameof(BarPath.BarColour), main_bar_colour, 1000, Easing.OutQuint);

                glowBar.TransformTo(nameof(BarPath.GlowColour), Colour4.White, 30, Easing.OutQuint)
                       .Then()
                       .TransformTo(nameof(BarPath.GlowColour), main_bar_glow_colour, 300, Easing.OutQuint);
            }
        }

        protected override void Miss()
        {
            base.Miss();

            if (resetMissBarDelegate != null)
            {
                resetMissBarDelegate.Cancel();
                resetMissBarDelegate = null;
            }
            else
            {
                // Reset any ongoing animation immediately, else things get weird.
                this.TransformTo(nameof(GlowBarValue), HealthBarValue);
            }

            this.Delay(500).Schedule(() =>
            {
                this.TransformTo(nameof(GlowBarValue), Current.Value, 300, Easing.OutQuint);
                finishMissDisplay();
            }, out resetMissBarDelegate);

            glowBar.TransformTo(nameof(BarPath.BarColour), new Colour4(255, 147, 147, 255), 100, Easing.OutQuint).Then()
                   .TransformTo(nameof(BarPath.BarColour), new Colour4(255, 93, 93, 255), 800, Easing.OutQuint);

            glowBar.TransformTo(nameof(BarPath.GlowColour), new Colour4(253, 0, 0, 255).Lighten(0.2f))
                   .TransformTo(nameof(BarPath.GlowColour), new Colour4(253, 0, 0, 255), 800, Easing.OutQuint);
        }

        private void finishMissDisplay()
        {
            if (resetMissBarDelegate == null)
                return;

            if (Current.Value > 0)
            {
                glowBar.TransformTo(nameof(BarPath.BarColour), main_bar_colour, 300, Easing.In);
                glowBar.TransformTo(nameof(BarPath.GlowColour), main_bar_glow_colour, 300, Easing.In);
            }

            resetMissBarDelegate?.Cancel();
            resetMissBarDelegate = null;
        }

        private void updatePath()
        {
            float usableWidth = DrawWidth - padding;

            if (usableWidth < 0) enforceMinimumWidth();

            // the display starts curving at `curve_start_offset` units from the right and ends curving at `curve_end_offset`.
            // to ensure that the curve is symmetric when it starts being narrow enough, add a `curve_end_offset` to the left side too.
            const float rescale_cutoff = curve_start_offset + curve_end_offset;

            float barLength = Math.Max(DrawWidth - padding, rescale_cutoff);
            float curveStart = barLength - curve_start_offset;
            float curveEnd = barLength - curve_end_offset;

            Vector2 diagonalDir = (new Vector2(curveEnd, BarHeight.Value) - new Vector2(curveStart, 0)).Normalized();

            barPath = new SliderPath(new[]
            {
                new PathControlPoint(new Vector2(0, 0), PathType.LINEAR),
                new PathControlPoint(new Vector2(curveStart - curve_smoothness, 0), PathType.BEZIER),
                new PathControlPoint(new Vector2(curveStart, 0)),
                new PathControlPoint(new Vector2(curveStart, 0) + diagonalDir * curve_smoothness, PathType.LINEAR),
                new PathControlPoint(new Vector2(curveEnd, BarHeight.Value) - diagonalDir * curve_smoothness, PathType.BEZIER),
                new PathControlPoint(new Vector2(curveEnd, BarHeight.Value)),
                new PathControlPoint(new Vector2(curveEnd + curve_smoothness, BarHeight.Value), PathType.LINEAR),
                new PathControlPoint(new Vector2(barLength, BarHeight.Value)),
            });

            if (DrawWidth - padding < rescale_cutoff)
                rescalePathProportionally();

            List<Vector2> vertices = new List<Vector2>();
            barPath.GetPathToProgress(vertices, 0.0, 1.0);

            background.Vertices = vertices;
            mainBar.Vertices = vertices;
            glowBar.Vertices = vertices;

            updatePathVertices();

            void enforceMinimumWidth()
            {
                // Switch to absolute in order to be able to define a minimum width.
                // Then switch back is required. Framework will handle the conversion for us.
                Axes relativeAxes = RelativeSizeAxes;
                RelativeSizeAxes = Axes.None;

                Width = padding;

                RelativeSizeAxes = relativeAxes;
            }

            void rescalePathProportionally()
            {
                foreach (var point in barPath.ControlPoints)
                    point.Position = new Vector2(point.Position.X / barLength * (DrawWidth - padding), point.Position.Y);
            }
        }

        private void updatePathVertices()
        {
            barPath.GetPathToProgress(healthBarVertices, 0.0, healthBarValue);
            barPath.GetPathToProgress(missBarVertices, healthBarValue, Math.Max(glowBarValue, healthBarValue));

            if (healthBarVertices.Count == 0)
                healthBarVertices.Add(Vector2.Zero);

            if (missBarVertices.Count == 0)
                missBarVertices.Add(Vector2.Zero);

            glowBar.Vertices = missBarVertices.Select(v => v - missBarVertices[0]).ToList();
            glowBar.Position = missBarVertices[0];

            mainBar.Vertices = healthBarVertices.Select(v => v - healthBarVertices[0]).ToList();
            mainBar.Position = healthBarVertices[0];
        }

        private partial class BackgroundPath : SmoothPath
        {
            protected override Color4 ColourAt(float position)
            {
                if (position <= 0.16f)
                    return Color4.White.Opacity(0.8f);

                return Interpolation.ValueAt(position,
                    Color4.White.Opacity(0.8f),
                    Color4.Black.Opacity(0.2f),
                    -0.5f, 1f, Easing.OutQuint);
            }
        }

        private partial class BarPath : SmoothPath
        {
            private Colour4 barColour;

            public Colour4 BarColour
            {
                get => barColour;
                set
                {
                    if (barColour == value)
                        return;

                    barColour = value;
                    InvalidateTexture();
                }
            }

            private Colour4 glowColour;

            public Colour4 GlowColour
            {
                get => glowColour;
                set
                {
                    if (glowColour == value)
                        return;

                    glowColour = value;
                    InvalidateTexture();
                }
            }

            public float GlowPortion { get; init; }

            protected override Color4 ColourAt(float position)
            {
                if (position >= GlowPortion)
                    return BarColour;

                return Interpolation.ValueAt(position, Colour4.Black.Opacity(0.0f), GlowColour, 0.0, GlowPortion, Easing.InQuint);
            }
        }
    }
}
