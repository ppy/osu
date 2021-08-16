// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Localisation;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Overlays.Settings.Sections.Gameplay
{
    public class GeneralSettings : SettingsSubsection
    {
        protected override LocalisableString Header => GameplaySettingsStrings.GeneralHeader;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsSlider<double>
                {
                    LabelText = GameplaySettingsStrings.BackgroundDim,
                    Current = config.GetBindable<double>(OsuSetting.DimLevel),
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true
                },
                new SettingsSlider<double>
                {
                    LabelText = GameplaySettingsStrings.BackgroundBlur,
                    Current = config.GetBindable<double>(OsuSetting.BlurLevel),
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true
                },
                new SettingsCheckbox
                {
                    LabelText = GameplaySettingsStrings.LightenDuringBreaks,
                    Current = config.GetBindable<bool>(OsuSetting.LightenDuringBreaks)
                },
                new SettingsEnumDropdown<HUDVisibilityMode>
                {
                    LabelText = GameplaySettingsStrings.HUDVisibilityMode,
                    Current = config.GetBindable<HUDVisibilityMode>(OsuSetting.HUDVisibilityMode)
                },
                new SettingsCheckbox
                {
                    LabelText = GameplaySettingsStrings.ShowDifficultyGraph,
                    Current = config.GetBindable<bool>(OsuSetting.ShowProgressGraph)
                },
                new SettingsCheckbox
                {
                    LabelText = GameplaySettingsStrings.ShowHealthDisplayWhenCantFail,
                    Current = config.GetBindable<bool>(OsuSetting.ShowHealthDisplayWhenCantFail),
                    Keywords = new[] { "hp", "bar" }
                },
                new SettingsCheckbox
                {
                    LabelText = GameplaySettingsStrings.FadePlayfieldWhenHealthLow,
                    Current = config.GetBindable<bool>(OsuSetting.FadePlayfieldWhenHealthLow),
                },
                new SettingsCheckbox
                {
                    LabelText = GameplaySettingsStrings.AlwaysShowKeyOverlay,
                    Current = config.GetBindable<bool>(OsuSetting.KeyOverlay)
                },
                new SettingsCheckbox
                {
                    LabelText = GameplaySettingsStrings.PositionalHitsounds,
                    Current = config.GetBindable<bool>(OsuSetting.PositionalHitSounds)
                },
                new SettingsCheckbox
                {
                    LabelText = GameplaySettingsStrings.AlwaysPlayFirstComboBreak,
                    Current = config.GetBindable<bool>(OsuSetting.AlwaysPlayFirstComboBreak)
                },
                new SettingsEnumDropdown<ScoringMode>
                {
                    LabelText = GameplaySettingsStrings.ScoreDisplayMode,
                    Current = config.GetBindable<ScoringMode>(OsuSetting.ScoreDisplayMode),
                    Keywords = new[] { "scoring" }
                },
            };

            if (RuntimeInfo.OS == RuntimeInfo.Platform.Windows)
            {
                Add(new SettingsCheckbox
                {
                    LabelText = GameplaySettingsStrings.DisableWinKey,
                    Current = config.GetBindable<bool>(OsuSetting.GameplayDisableWinKey)
                });
            }
        }
    }
}
