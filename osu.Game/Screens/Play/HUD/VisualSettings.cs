// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Timing;
using osu.Game.Configuration;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Play.ReplaySettings;

namespace osu.Game.Screens.Play.HUD
{
    public class VisualSettings : ReplayGroup
    {
        protected override string Title => "Visual settings";

        public IAdjustableClock AudioClock { get; set; }
        public FramedClock FramedClock { get; set; }

        private bool autohide;
        public bool Autohide
        {
            get => autohide;
            set
            {
                autohide = value;
                if (autohide && hideStopWatch == null)
                    hideStopWatch = Stopwatch.StartNew();
                else if (!autohide)
                    hideStopWatch = null;
            }
        }

        private readonly TimeSpan hideTimeSpan = TimeSpan.FromSeconds(3);
        private Stopwatch hideStopWatch;

        private readonly ReplaySliderBar<double> dimSliderBar;
        private readonly ReplaySliderBar<double> blurSliderBar;
        private readonly ReplayCheckbox showStoryboardToggle;
        private readonly ReplayCheckbox mouseWheelDisabledToggle;

        public VisualSettings()
        {
            Children = new Drawable[]
            {
                new OsuSpriteText
                {
                    Text = "Background dim:"
                },
                dimSliderBar = new ReplaySliderBar<double>(),
                new OsuSpriteText
                {
                    Text = "Background blur:"
                },
                blurSliderBar = new ReplaySliderBar<double>(),
                new OsuSpriteText
                {
                    Text = "Toggles:"
                },
                showStoryboardToggle = new ReplayCheckbox { LabelText = "Storyboards" },
                mouseWheelDisabledToggle = new ReplayCheckbox { LabelText = "Disable mouse wheel" }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            dimSliderBar.Bindable = config.GetBindable<double>(OsuSetting.DimLevel);
            blurSliderBar.Bindable = config.GetBindable<double>(OsuSetting.BlurLevel);
            showStoryboardToggle.Bindable = config.GetBindable<bool>(OsuSetting.ShowStoryboard);
            mouseWheelDisabledToggle.Bindable = config.GetBindable<bool>(OsuSetting.MouseDisableWheel);

            ToggleContentVisibility();
        }

        protected override void ToggleContentVisibility()
        {
            base.ToggleContentVisibility();
            if (!Autohide)
                return;
            if (Expanded)
            {
                AudioClock.Stop();
                FramedClock.ProcessSourceClockFrames = false;
                hideStopWatch.Stop();
            }
            else
            {
                AudioClock.Start();
                FramedClock.ProcessSourceClockFrames = true;
                hideStopWatch.Start();
            }
        }

        protected override void Update()
        {
            base.Update();

            if (Autohide && IsPresent && hideStopWatch.Elapsed > hideTimeSpan) this.FadeOut(50);
        }
    }
}
