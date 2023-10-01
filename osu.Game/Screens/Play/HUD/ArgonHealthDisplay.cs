// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Threading;
using osu.Framework.Utils;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    public partial class ArgonHealthDisplay : HealthDisplay, ISerialisableDrawable
    {
        private const float curve_start = 280;
        private const float curve_end = 310;
        private const float curve_smoothness = 10;

        private const float bar_length = 350;
        private const float bar_height = 32.5f;

        private BarPath healthBar = null!;
        private BarPath missBar = null!;

        private SliderPath barPath = null!;

        private static readonly Colour4 health_bar_colour = Colour4.White;

        // the opacity isn't part of the design, it's only here to control glow intensity.
        private static readonly Colour4 health_bar_glow_colour = Color4Extensions.FromHex("#7ED7FD").Opacity(0.5f);
        private static readonly Colour4 health_bar_flash_colour = Color4Extensions.FromHex("#7ED7FD").Opacity(0.6f);

        private static readonly Colour4 miss_bar_colour = Color4Extensions.FromHex("#FF9393");
        private static readonly Colour4 miss_bar_glow_colour = Color4Extensions.FromHex("#FD0000");

        // the "flashed" glow colour is just a lightened version of the original one, not part of the design.
        private static readonly Colour4 miss_bar_flash_colour = Color4Extensions.FromHex("#FF5D5D");

        public bool UsesFixedAnchor { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Both;

            Vector2 diagonalDir = (new Vector2(curve_end, bar_height) - new Vector2(curve_start, 0)).Normalized();

            // todo: SliderPath or parts of it should be moved away to a utility class as they're useful for making curved paths in general, as done here.
            barPath = new SliderPath(new[]
            {
                new PathControlPoint(new Vector2(0, 0), PathType.Linear),
                new PathControlPoint(new Vector2(curve_start - curve_smoothness, 0), PathType.Bezier),
                new PathControlPoint(new Vector2(curve_start, 0)),
                new PathControlPoint(new Vector2(curve_start, 0) + diagonalDir * curve_smoothness, PathType.Linear),
                new PathControlPoint(new Vector2(curve_end, bar_height) - diagonalDir * curve_smoothness, PathType.Bezier),
                new PathControlPoint(new Vector2(curve_end, bar_height)),
                new PathControlPoint(new Vector2(curve_end + curve_smoothness, bar_height), PathType.Linear),
                new PathControlPoint(new Vector2(bar_length, bar_height)),
            });

            var vertices = new List<Vector2>();
            barPath.GetPathToProgress(vertices, 0.0, 1.0);

            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(4f, 0f),
                Children = new Drawable[]
                {
                    new Circle
                    {
                        Margin = new MarginPadding { Top = 10f - 3f / 2f, Left = -2f },
                        Size = new Vector2(50f, 3f),
                    },
                    new Container
                    {
                        AutoSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            new BackgroundPath
                            {
                                PathRadius = 10f,
                                Vertices = vertices,
                            },
                            missBar = new BarPath
                            {
                                BarColour = miss_bar_colour,
                                GlowColour = miss_bar_glow_colour,
                                Alpha = 0f,
                                PathRadius = 20f,
                                GlowPortion = 0.75f,
                                Margin = new MarginPadding(-10f),
                                Vertices = vertices
                            },
                            healthBar = new BarPath
                            {
                                BarColour = health_bar_colour,
                                GlowColour = health_bar_glow_colour,
                                PathRadius = 10f,
                                GlowPortion = 0.6f,
                                Vertices = vertices
                            },
                        }
                    }
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindValueChanged(v =>
            {
                if (v.NewValue > MissBarValue)
                {
                    missBar.FadeOut(300, Easing.OutQuint);
                    resetMissBarDelegate?.Cancel();
                    resetMissBarDelegate = null;
                }

                this.TransformTo(nameof(HealthBarValue), v.NewValue, 300, Easing.OutQuint);
            }, true);
        }

        protected override void Update()
        {
            base.Update();

            float targetAlpha = Current.Value > 0 ? 1 : 0;
            healthBar.Alpha = (float)Interpolation.DampContinuously(healthBar.Alpha, targetAlpha, 50.0, Time.Elapsed);
        }

        private ScheduledDelegate? resetMissBarDelegate;

        protected override void Miss(JudgementResult result)
        {
            base.Miss(result);

            if (resetMissBarDelegate != null)
                resetMissBarDelegate.Cancel();
            else
                this.TransformTo(nameof(MissBarValue), HealthBarValue);

            this.Delay(500).Schedule(() =>
            {
                this.TransformTo(nameof(MissBarValue), Current.Value, 300, Easing.OutQuint);
                resetMissBarDelegate = null;
            }, out resetMissBarDelegate);

            missBar.FadeIn(120, Easing.OutQuint);
            missBar.Delay(500).FadeOut(300, Easing.InQuint);

            missBar.TransformTo(nameof(BarPath.BarColour), miss_bar_colour.Lighten(0.1f))
                   .TransformTo(nameof(BarPath.BarColour), miss_bar_colour, 300, Easing.OutQuint);

            missBar.TransformTo(nameof(BarPath.GlowColour), miss_bar_flash_colour)
                   .TransformTo(nameof(BarPath.GlowColour), miss_bar_glow_colour, 300, Easing.OutQuint);
        }

        protected override void Flash(JudgementResult result)
        {
            base.Flash(result);

            healthBar.TransformTo(nameof(BarPath.GlowColour), health_bar_flash_colour)
                     .TransformTo(nameof(BarPath.GlowColour), health_bar_glow_colour, 300, Easing.OutQuint);
        }

        private double missBarValue = 1.0;
        private readonly List<Vector2> missBarVertices = new List<Vector2>();

        public double MissBarValue
        {
            get => missBarValue;
            set
            {
                if (missBarValue == value)
                    return;

                missBarValue = value;
                updatePathVertices();
            }
        }

        private double healthBarValue = 1.0;
        private readonly List<Vector2> healthBarVertices = new List<Vector2>();

        public double HealthBarValue
        {
            get => healthBarValue;
            set
            {
                if (healthBarValue == value)
                    return;

                healthBarValue = value;
                updatePathVertices();
            }
        }

        private void updatePathVertices()
        {
            barPath.GetPathToProgress(healthBarVertices, 0.0, healthBarValue);
            barPath.GetPathToProgress(missBarVertices, healthBarValue, Math.Max(missBarValue, healthBarValue));

            if (healthBarVertices.Count == 0)
                healthBarVertices.Add(Vector2.Zero);

            if (missBarVertices.Count == 0)
                missBarVertices.Add(Vector2.Zero);

            missBar.Vertices = missBarVertices.Select(v => v - missBarVertices[0]).ToList();
            missBar.Position = missBarVertices[0];

            healthBar.Vertices = healthBarVertices.Select(v => v - healthBarVertices[0]).ToList();
            healthBar.Position = healthBarVertices[0];
        }

        private partial class BackgroundPath : SmoothPath
        {
            protected override Color4 ColourAt(float position)
            {
                if (position <= 0.128f)
                    return Color4.White.Opacity(0.5f);

                position -= 0.128f;
                return Interpolation.ValueAt(Math.Clamp(position, 0f, 1f), Color4.White.Opacity(0.5f), Color4.Black.Opacity(0.5f), -0.75f, 1f, Easing.OutQuart);
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

                return Interpolation.ValueAt(position, Colour4.Black.Opacity(0.0f), GlowColour, 0.0, GlowPortion);
            }
        }
    }
}
