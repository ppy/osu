// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Layout;
using osu.Framework.Threading;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Rulesets.Judgements;
using osu.Game.Screens.Play.HUD.ArgonHealthDisplayParts;
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

        private ArgonHealthDisplayBar mainBar = null!;

        /// <summary>
        /// Used to show a glow at the end of the main bar, or red "damage" area when missing.
        /// </summary>
        private ArgonHealthDisplayBar glowBar = null!;

        private Container content = null!;

        private static readonly Colour4 main_bar_colour = Colour4.White;
        private static readonly Colour4 main_bar_glow_colour = Color4Extensions.FromHex("#7ED7FD").Opacity(0.5f);

        private ScheduledDelegate? resetMissBarDelegate;

        private bool displayingMiss => resetMissBarDelegate != null;

        private double glowBarValue;

        private double healthBarValue;

        public const float MAIN_PATH_RADIUS = 10f;
        private const float padding = MAIN_PATH_RADIUS * 2;

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

            InternalChild = content = new Container
            {
                Children = new Drawable[]
                {
                    new ArgonHealthDisplayBackground
                    {
                        RelativeSizeAxes = Axes.Both,
                        PathRadius = MAIN_PATH_RADIUS,
                        PathPadding = MAIN_PATH_RADIUS
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding(-30f),
                        Child = glowBar = new ArgonHealthDisplayBar
                        {
                            RelativeSizeAxes = Axes.Both,
                            BarColour = Color4.White,
                            GlowColour = main_bar_glow_colour,
                            Blending = BlendingParameters.Additive,
                            Colour = ColourInfo.GradientHorizontal(Color4.White.Opacity(0.8f), Color4.White),
                            PathRadius = 40f,
                            PathPadding = 40f,
                            GlowPortion = 0.9f,
                        }
                    },
                    mainBar = new ArgonHealthDisplayBar
                    {
                        RelativeSizeAxes = Axes.Both,
                        Blending = BlendingParameters.Additive,
                        BarColour = main_bar_colour,
                        GlowColour = main_bar_glow_colour,
                        PathRadius = MAIN_PATH_RADIUS,
                        PathPadding = MAIN_PATH_RADIUS,
                        GlowPortion = 0.6f,
                    }
                }
            };
        }

        private bool pendingMissAnimation;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            HealthProcessor.NewJudgement += onNewJudgement;

            // we're about to set `RelativeSizeAxes` depending on the value of `UseRelativeSize`.
            // setting `RelativeSizeAxes` internally transforms absolute sizing to relative and back to keep the size the same,
            // but that is not what we want in this case, since the width at this point is valid in the *target* sizing mode.
            // to counteract this, store the numerical value here, and restore it after setting the correct initial relative sizing axes.
            float previousWidth = Width;
            UseRelativeSize.BindValueChanged(v => RelativeSizeAxes = v.NewValue ? Axes.X : Axes.None, true);
            Width = previousWidth;

            BarHeight.BindValueChanged(_ => updateContentSize(), true);
        }

        private void onNewJudgement(JudgementResult result) => pendingMissAnimation |= !result.IsHit;

        protected override void Update()
        {
            base.Update();

            if (!drawSizeLayout.IsValid)
            {
                updateContentSize();
                drawSizeLayout.Validate();
            }

            healthBarValue = Interpolation.DampContinuously(healthBarValue, Current.Value, 50, Time.Elapsed);
            if (!displayingMiss)
                glowBarValue = Interpolation.DampContinuously(glowBarValue, Current.Value, 50, Time.Elapsed);

            mainBar.Alpha = (float)Interpolation.DampContinuously(mainBar.Alpha, Current.Value > 0 ? 1 : 0, 40, Time.Elapsed);
            glowBar.Alpha = (float)Interpolation.DampContinuously(glowBar.Alpha, glowBarValue > 0 ? 1 : 0, 40, Time.Elapsed);

            updatePathProgress();
        }

        protected override void HealthChanged(bool increase)
        {
            if (Current.Value >= glowBarValue)
                finishMissDisplay();

            if (pendingMissAnimation)
            {
                triggerMissDisplay();
                pendingMissAnimation = false;
            }

            base.HealthChanged(increase);
        }

        protected override void FinishInitialAnimation(double value)
        {
            base.FinishInitialAnimation(value);
            this.TransformTo(nameof(healthBarValue), value, 500, Easing.OutQuint);
            this.TransformTo(nameof(glowBarValue), value, 250, Easing.OutQuint);
        }

        protected override void Flash()
        {
            base.Flash();

            if (!displayingMiss)
            {
                glowBar.TransformTo(nameof(ArgonHealthDisplayBar.GlowColour), Colour4.White, 30, Easing.OutQuint)
                       .Then()
                       .TransformTo(nameof(ArgonHealthDisplayBar.GlowColour), main_bar_glow_colour, 300, Easing.OutQuint);
            }
        }

        private void triggerMissDisplay()
        {
            resetMissBarDelegate?.Cancel();
            resetMissBarDelegate = null;

            this.Delay(500).Schedule(() =>
            {
                this.TransformTo(nameof(glowBarValue), Current.Value, 300, Easing.OutQuint);
                finishMissDisplay();
            }, out resetMissBarDelegate);

            glowBar.TransformTo(nameof(ArgonHealthDisplayBar.BarColour), new Colour4(255, 147, 147, 255), 100, Easing.OutQuint).Then()
                   .TransformTo(nameof(ArgonHealthDisplayBar.BarColour), new Colour4(255, 93, 93, 255), 800, Easing.OutQuint);

            glowBar.TransformTo(nameof(ArgonHealthDisplayBar.GlowColour), new Colour4(253, 0, 0, 255).Lighten(0.2f))
                   .TransformTo(nameof(ArgonHealthDisplayBar.GlowColour), new Colour4(253, 0, 0, 255), 800, Easing.OutQuint);
        }

        private void finishMissDisplay()
        {
            if (!displayingMiss)
                return;

            if (Current.Value > 0)
            {
                glowBar.TransformTo(nameof(ArgonHealthDisplayBar.BarColour), main_bar_colour, 300, Easing.In);
                glowBar.TransformTo(nameof(ArgonHealthDisplayBar.GlowColour), main_bar_glow_colour, 300, Easing.In);
            }

            resetMissBarDelegate?.Cancel();
            resetMissBarDelegate = null;
        }

        private void updateContentSize()
        {
            float usableWidth = DrawWidth - padding;

            if (usableWidth < 0) enforceMinimumWidth();

            content.Size = new Vector2(DrawWidth, BarHeight.Value + padding);
            updatePathProgress();

            void enforceMinimumWidth()
            {
                // Switch to absolute in order to be able to define a minimum width.
                // Then switch back is required. Framework will handle the conversion for us.
                Axes relativeAxes = RelativeSizeAxes;
                RelativeSizeAxes = Axes.None;

                Width = padding;

                RelativeSizeAxes = relativeAxes;
            }
        }

        private void updatePathProgress()
        {
            mainBar.ProgressRange = new Vector2(0f, (float)healthBarValue);
            glowBar.ProgressRange = new Vector2((float)healthBarValue, (float)Math.Max(glowBarValue, healthBarValue));
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (HealthProcessor.IsNotNull())
                HealthProcessor.NewJudgement -= onNewJudgement;
        }
    }
}
