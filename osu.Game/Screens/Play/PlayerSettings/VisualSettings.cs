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

        public VisualSettings()
            : base("视觉效果设置")
        {
            Children = new Drawable[]
            {
                new OsuSpriteText
                {
                    Text = "背景暗化:"
                },
                dimSliderBar = new PlayerSliderBar<double>
                {
                    DisplayAsPercentage = true
                },
                new OsuSpriteText
                {
                    Text = "背景模糊:"
                },
                blurSliderBar = new PlayerSliderBar<double>
                {
                    DisplayAsPercentage = true
                },
                new OsuSpriteText
                {
                    Text = "切换:"
                },
                showStoryboardToggle = new PlayerCheckbox { LabelText = "故事版 / 背景视频" },
                beatmapSkinsToggle = new PlayerCheckbox { LabelText = "谱面皮肤" },
                beatmapColorsToggle = new PlayerCheckbox { LabelText = "谱面颜色" }
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
        }
    }
}
