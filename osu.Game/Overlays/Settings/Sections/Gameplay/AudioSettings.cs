// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Localisation;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Overlays.Settings.Sections.Gameplay
{
    public class AudioSettings : SettingsSubsection
    {
        protected override LocalisableString Header => GameplaySettingsStrings.AudioHeader;

        private Bindable<float> positionalHitsoundsLevel;

        private FillFlowContainer<SettingsSlider<float>> positionalHitsoundsSettings;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, OsuConfigManager osuConfig)
        {
            positionalHitsoundsLevel = osuConfig.GetBindable<float>(OsuSetting.PositionalHitsoundsLevel);
            Children = new Drawable[]
            {
                positionalHitsoundsSettings = new FillFlowContainer<SettingsSlider<float>>
                {
                    Direction = FillDirection.Vertical,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Masking = true,
                    Children = new[]
                    {
                        new SettingsSlider<float>
                        {
                            LabelText = AudioSettingsStrings.PositionalLevel,
                            Current = positionalHitsoundsLevel,
                            KeyboardStep = 0.01f,
                            DisplayAsPercentage = true
                        }
                    }
                },
                new SettingsCheckbox
                {
                    LabelText = GameplaySettingsStrings.AlwaysPlayFirstComboBreak,
                    Current = config.GetBindable<bool>(OsuSetting.AlwaysPlayFirstComboBreak)
                }
            };
        }
    }
}
