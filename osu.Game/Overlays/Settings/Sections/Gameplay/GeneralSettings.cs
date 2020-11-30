// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Overlays.Settings.Sections.Gameplay
{
    public class GeneralSettings : SettingsSubsection
    {
        protected override string Header => "General";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsSlider<double>
                {
                    LabelText = "Background dim",
                    Current = config.GetBindable<double>(OsuSetting.DimLevel),
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true
                },
                new SettingsSlider<double>
                {
                    LabelText = "Background blur",
                    Current = config.GetBindable<double>(OsuSetting.BlurLevel),
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true
                },
                new SettingsCheckbox
                {
                    LabelText = "Lighten playfield during breaks",
                    Current = config.GetBindable<bool>(OsuSetting.LightenDuringBreaks)
                },
                new SettingsEnumDropdown<HUDVisibilityMode>
                {
                    LabelText = "HUD overlay visibility mode",
                    Current = config.GetBindable<HUDVisibilityMode>(OsuSetting.HUDVisibilityMode)
                },
                new SettingsCheckbox
                {
                    LabelText = "Show difficulty graph on progress bar",
                    Current = config.GetBindable<bool>(OsuSetting.ShowProgressGraph)
                },
                new SettingsCheckbox
                {
                    LabelText = "Show health display even when you can't fail",
                    Current = config.GetBindable<bool>(OsuSetting.ShowHealthDisplayWhenCantFail),
                    Keywords = new[] { "hp", "bar" }
                },
                new SettingsCheckbox
                {
                    LabelText = "Fade playfield to red when health is low",
                    Current = config.GetBindable<bool>(OsuSetting.FadePlayfieldWhenHealthLow),
                },
                new SettingsCheckbox
                {
                    LabelText = "Always show key overlay",
                    Current = config.GetBindable<bool>(OsuSetting.KeyOverlay)
                },
                new SettingsCheckbox
                {
                    LabelText = "Positional hitsounds",
                    Current = config.GetBindable<bool>(OsuSetting.PositionalHitSounds)
                },
                new SettingsCheckbox
                {
                    LabelText = "Always play first combo break sound",
                    Current = config.GetBindable<bool>(OsuSetting.AlwaysPlayFirstComboBreak)
                },
                new SettingsEnumDropdown<ScoreMeterType>
                {
                    LabelText = "Score meter type",
                    Current = config.GetBindable<ScoreMeterType>(OsuSetting.ScoreMeter)
                },
                new SettingsEnumDropdown<ScoringMode>
                {
                    LabelText = "Score display mode",
                    Current = config.GetBindable<ScoringMode>(OsuSetting.ScoreDisplayMode),
                    Keywords = new[] { "scoring" }
                },
            };

            if (RuntimeInfo.OS == RuntimeInfo.Platform.Windows)
            {
                Add(new SettingsCheckbox
                {
                    LabelText = "Disable Windows key during gameplay",
                    Current = config.GetBindable<bool>(OsuSetting.GameplayDisableWinKey)
                });
            }
        }
    }
}
