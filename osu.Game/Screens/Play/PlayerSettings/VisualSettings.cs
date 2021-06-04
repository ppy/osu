// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Play.PlayerSettings
{
    public class VisualSettings : PlayerSettingsGroup
    {
        private readonly PlayerSliderBar<double> dimSliderBar;
        private readonly PlayerSliderBar<double> blurSliderBar;
        private readonly PlayerCheckbox showStoryboardToggle;
        private readonly PlayerCheckbox beatmapSkinsToggle;
        private readonly PlayerCheckbox beatmapColorsToggle;
        private readonly PlayerCheckbox beatmapHitsoundsToggle;

        public VisualSettings()
            : base("Visual Settings")
        {
            Children = new Drawable[]
            {
                new OsuSpriteText
                {
                    Text = "Background dim:"
                },
                dimSliderBar = new PlayerSliderBar<double>
                {
                    DisplayAsPercentage = true
                },
                new OsuSpriteText
                {
                    Text = "Background blur:"
                },
                blurSliderBar = new PlayerSliderBar<double>
                {
                    DisplayAsPercentage = true
                },
                new OsuSpriteText
                {
                    Text = "Toggles:"
                },
                showStoryboardToggle = new PlayerCheckbox { LabelText = "Storyboard / Video" },
                beatmapSkinsToggle = new PlayerCheckbox { LabelText = "Beatmap skins" },
                beatmapColorsToggle = new PlayerCheckbox { LabelText = "Beatmap colours" },
                beatmapHitsoundsToggle = new PlayerCheckbox { LabelText = "Beatmap hitsounds" }
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
            beatmapHitsoundsToggle.Current = config.GetBindable<bool>(OsuSetting.BeatmapHitsounds);
        }
    }
}
