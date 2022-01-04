// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Rulesets.Scoring;
using osuTK.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    /// <summary>
    /// An overlay layer on top of the playfield which fades to red when the current player health falls below a certain threshold defined by <see cref="low_health_threshold"/>.
    /// </summary>
    public class FailingLayer : HealthDisplay
    {
        /// <summary>
        /// Whether the current player health should be shown on screen.
        /// </summary>
        public readonly Bindable<bool> ShowHealth = new Bindable<bool>();

        private const float max_alpha = 0.4f;
        private const int fade_time = 400;
        private const float gradient_size = 0.2f;

        /// <summary>
        /// The threshold under which the current player life should be considered low and the layer should start fading in.
        /// </summary>
        private const double low_health_threshold = 0.20f;

        private readonly Container boxes;

        private Bindable<bool> fadePlayfieldWhenHealthLow;

        public FailingLayer()
        {
            RelativeSizeAxes = Axes.Both;
            InternalChildren = new Drawable[]
            {
                boxes = new Container
                {
                    Alpha = 0,
                    Blending = BlendingParameters.Additive,
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = ColourInfo.GradientHorizontal(Color4.White, Color4.White.Opacity(0)),
                            Width = gradient_size,
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = gradient_size,
                            Colour = ColourInfo.GradientHorizontal(Color4.White.Opacity(0), Color4.White),
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                        },
                    }
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour color, OsuConfigManager config)
        {
            boxes.Colour = color.Red;

            fadePlayfieldWhenHealthLow = config.GetBindable<bool>(OsuSetting.FadePlayfieldWhenHealthLow);
            fadePlayfieldWhenHealthLow.BindValueChanged(_ => updateState());
            ShowHealth.BindValueChanged(_ => updateState());
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateState();
        }

        private void updateState()
        {
            // Don't display ever if the ruleset is not using a draining health display.
            bool showLayer = HealthProcessor is DrainingHealthProcessor && fadePlayfieldWhenHealthLow.Value && ShowHealth.Value;
            this.FadeTo(showLayer ? 1 : 0, fade_time, Easing.OutQuint);
        }

        protected override void Update()
        {
            double target = Math.Clamp(max_alpha * (1 - Current.Value / low_health_threshold), 0, max_alpha);

            boxes.Alpha = (float)Interpolation.Lerp(boxes.Alpha, target, Clock.ElapsedFrameTime * 0.01f);

            base.Update();
        }
    }
}
