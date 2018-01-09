// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Configuration;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Play.ReplaySettings;

namespace osu.Game.Screens.Play.HUD
{
    public class VisualSettings : ReplayGroup
    {
        protected override string Title => "Visual settings";
        public IAdjustableClock AdjustableClock { get; set; }

        private readonly ReplaySliderBar<double> dimSliderBar;
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
                        Text = "Toggles:"
                    },
                    showStoryboardToggle = new ReplayCheckbox {LabelText = "Storyboards" },
                    mouseWheelDisabledToggle = new ReplayCheckbox { LabelText = "Disable mouse wheel" }
            };
            ToggleContentVisibility();
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            dimSliderBar.Bindable = config.GetBindable<double>(OsuSetting.DimLevel);
            showStoryboardToggle.Bindable = config.GetBindable<bool>(OsuSetting.ShowStoryboard);
            mouseWheelDisabledToggle.Bindable = config.GetBindable<bool>(OsuSetting.MouseDisableWheel);
        }

        protected override void ToggleContentVisibility()
        {
            base.ToggleContentVisibility();
            if (Expanded)
                AdjustableClock?.Stop();
            else
                AdjustableClock?.Start();
        }
    }
}
