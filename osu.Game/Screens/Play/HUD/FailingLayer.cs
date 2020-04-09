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
    /// An overlay layer on top of the playfield which fades to red when the current player health falls below a certain threshold defined by <see cref="LowHealthThreshold"/>.
    /// </summary>
    public class FailingLayer : HealthDisplay
    {
        private const float max_alpha = 0.4f;

        private const int fade_time = 400;

        private Bindable<bool> enabled;

        /// <summary>
        /// The threshold under which the current player life should be considered low and the layer should start fading in.
        /// </summary>
        public double LowHealthThreshold = 0.20f;

        private readonly Container boxes;

        public FailingLayer()
        {
            RelativeSizeAxes = Axes.Both;
            Children = new Drawable[]
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
                            Colour = ColourInfo.GradientVertical(Color4.White, Color4.White.Opacity(0)),
                            Height = 0.2f,
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Height = 0.2f,
                            Colour = ColourInfo.GradientVertical(Color4.White.Opacity(0), Color4.White),
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                        },
                    }
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour color, OsuConfigManager config)
        {
            boxes.Colour = color.Red;

            enabled = config.GetBindable<bool>(OsuSetting.FadePlayfieldWhenHealthLow);
            enabled.BindValueChanged(e => this.FadeTo(e.NewValue ? 1 : 0, fade_time, Easing.OutQuint), true);
        }

        public override void BindHealthProcessor(HealthProcessor processor)
        {
            base.BindHealthProcessor(processor);

            // don't display ever if the ruleset is not using a draining health display.
            if (!(processor is DrainingHealthProcessor))
            {
                enabled.UnbindBindings();
                enabled.Value = false;
            }
        }

        protected override void Update()
        {
            boxes.Alpha = (float)Interpolation.ValueAt(Math.Clamp(Clock.ElapsedFrameTime, 0, fade_time), boxes.Alpha,
                Math.Clamp(max_alpha * (1 - Current.Value / LowHealthThreshold), 0, max_alpha), 0, fade_time, Easing.Out);

            base.Update();
        }
    }
}
