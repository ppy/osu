// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Screens.Play.HUD
{
    /// <summary>
    /// An overlay layer on top of the playfield which fades to red when the current player health falls below a certain threshold defined by <see cref="LowHealthThreshold"/>.
    /// </summary>
    public class FailingLayer : HealthDisplay
    {
        private const float max_alpha = 0.4f;

        private const int fade_time = 400;

        private readonly Box box;

        private Bindable<bool> enabled;

        /// <summary>
        /// The threshold under which the current player life should be considered low and the layer should start fading in.
        /// </summary>
        public double LowHealthThreshold = 0.20f;

        public FailingLayer()
        {
            RelativeSizeAxes = Axes.Both;
            Child = box = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Alpha = 0
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour color, OsuConfigManager config)
        {
            box.Colour = color.Red;
            enabled = config.GetBindable<bool>(OsuSetting.FadePlayfieldWhenHealthLow);
            enabled.BindValueChanged(e => this.FadeTo(e.NewValue ? 1 : 0, fade_time, Easing.OutQuint), true);
        }

        public override void BindHealthProcessor(HealthProcessor processor)
        {
            base.BindHealthProcessor(processor);

            if (!(processor is DrainingHealthProcessor))
            {
                enabled.UnbindBindings();
                enabled.Value = false;
            }
        }

        protected override void Update()
        {
            box.Alpha = (float)Interpolation.ValueAt(Math.Clamp(Clock.ElapsedFrameTime, 0, fade_time), box.Alpha,
                Math.Clamp(max_alpha * (1 - Current.Value / LowHealthThreshold), 0, max_alpha), 0, fade_time, Easing.Out);

            base.Update();
        }
    }
}
