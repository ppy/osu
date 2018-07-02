// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Play.PlayerSettings
{
    public class VisualSettings : PlayerSettingsGroup
    {
        protected override string Title => "Visual settings";

        private readonly PlayerSliderBar<double> dimSliderBar;
        private readonly PlayerSliderBar<double> blurSliderBar;
        private readonly PlayerCheckbox showStoryboardToggle;
        private readonly PlayerCheckbox beatmapSkinsToggle;
        private readonly PlayerCheckbox beatmapHitsoundsToggle;

        public VisualSettings()
        {
            Children = new Drawable[]
            {
                new OsuSpriteText
                {
                    Text = "Background dim:"
                },
                dimSliderBar = new PlayerSliderBar<double>(),
                new OsuSpriteText
                {
                    Text = "Background blur:"
                },
                blurSliderBar = new PlayerSliderBar<double>(),
                new OsuSpriteText
                {
                    Text = "Toggles:"
                },
                showStoryboardToggle = new PlayerCheckbox { LabelText = "Storyboards" },
                beatmapSkinsToggle = new PlayerCheckbox { LabelText = "Beatmap skins" },
                beatmapHitsoundsToggle = new PlayerCheckbox { LabelText = "Beatmap hitsounds" }
            };
        }

        [BackgroundDependencyLoader]
        private void load(BindableVisualSettings visualSettings)
        {
            dimSliderBar.Bindable = visualSettings.DimLevel;
            blurSliderBar.Bindable = visualSettings.BlurLevel;
            showStoryboardToggle.Bindable = visualSettings.ShowStoryboard;
            beatmapSkinsToggle.Bindable = visualSettings.BeatmapSkins;
            beatmapHitsoundsToggle.Bindable = visualSettings.BeatmapHitsounds;
        }
    }
}
