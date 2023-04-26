// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Localisation;

namespace osu.Game.Screens.Play.PlayerSettings
{
    public partial class VisualSettings : PlayerSettingsGroup
    {
        private readonly PlayerSliderBar<double> dimSliderBar;
        private readonly PlayerSliderBar<double> blurSliderBar;
        private readonly PlayerSliderBar<float> comboColourNormalisationSliderBar;
        private readonly PlayerCheckbox showStoryboardToggle;
        private readonly PlayerCheckbox beatmapSkinsToggle;
        private readonly PlayerCheckbox beatmapColorsToggle;

        public VisualSettings()
            : base("Visual Settings")
        {
            Children = new Drawable[]
            {
                dimSliderBar = new PlayerSliderBar<double>
                {
                    LabelText = GameplaySettingsStrings.BackgroundDim,
                    DisplayAsPercentage = true
                },
                blurSliderBar = new PlayerSliderBar<double>
                {
                    LabelText = GameplaySettingsStrings.BackgroundBlur,
                    DisplayAsPercentage = true
                },
                showStoryboardToggle = new PlayerCheckbox { LabelText = GraphicsSettingsStrings.StoryboardVideo },
                beatmapSkinsToggle = new PlayerCheckbox { LabelText = SkinSettingsStrings.BeatmapSkins },
                beatmapColorsToggle = new PlayerCheckbox { LabelText = SkinSettingsStrings.BeatmapColours },
                comboColourNormalisationSliderBar = new PlayerSliderBar<float>
                {
                    LabelText = GraphicsSettingsStrings.ComboColourNormalisation,
                    DisplayAsPercentage = true,
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            dimSliderBar.Current = config.GetBindable<double>(OsuSetting.DimLevel);
            blurSliderBar.Current = config.GetBindable<double>(OsuSetting.BlurLevel);
            showStoryboardToggle.Current = config.GetBindable<bool>(OsuSetting.ShowStoryboard);
            beatmapSkinsToggle.Current = config.GetBindable<bool>(OsuSetting.BeatmapSkins);
            beatmapColorsToggle.Current = config.GetBindable<bool>(OsuSetting.BeatmapColours);
            comboColourNormalisationSliderBar.Current = config.GetBindable<float>(OsuSetting.ComboColourNormalisationAmount);
        }
    }
}
