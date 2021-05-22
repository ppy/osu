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
        protected override string Header => "整体";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsSlider<double>
                {
                    LabelText = "背景暗化",
                    Current = config.GetBindable<double>(OsuSetting.DimLevel),
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true
                },
                new SettingsSlider<double>
                {
                    LabelText = "背景模糊",
                    Current = config.GetBindable<double>(OsuSetting.BlurLevel),
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true
                },
                new SettingsCheckbox
                {
                    LabelText = "在休息时段使背景变亮",
                    Current = config.GetBindable<bool>(OsuSetting.LightenDuringBreaks)
                },
                new SettingsEnumDropdown<HUDVisibilityMode>
                {
                    LabelText = "分数计界面显示模式",
                    Current = config.GetBindable<HUDVisibilityMode>(OsuSetting.HUDVisibilityMode)
                },
                new SettingsCheckbox
                {
                    LabelText = "显示难度分布图",
                    Current = config.GetBindable<bool>(OsuSetting.ShowProgressGraph)
                },
                new SettingsCheckbox
                {
                    LabelText = "即使你无法失败，也显示分数计界面",
                    Current = config.GetBindable<bool>(OsuSetting.ShowHealthDisplayWhenCantFail),
                    Keywords = new[] { "hp", "bar" }
                },
                new SettingsCheckbox
                {
                    LabelText = "生命值低时游戏区域泛红",
                    Current = config.GetBindable<bool>(OsuSetting.FadePlayfieldWhenHealthLow),
                },
                new SettingsCheckbox
                {
                    LabelText = "总是显示按键表示框",
                    Current = config.GetBindable<bool>(OsuSetting.KeyOverlay)
                },
                new SettingsCheckbox
                {
                    LabelText = "立体音效",
                    Current = config.GetBindable<bool>(OsuSetting.PositionalHitSounds)
                },
                new SettingsCheckbox
                {
                    LabelText = "在第一次断连时播放音效",
                    Current = config.GetBindable<bool>(OsuSetting.AlwaysPlayFirstComboBreak)
                },
                new SettingsEnumDropdown<ScoringMode>
                {
                    LabelText = "分数显示模式",
                    Current = config.GetBindable<ScoringMode>(OsuSetting.ScoreDisplayMode),
                    Keywords = new[] { "scoring" }
                },
            };

            if (RuntimeInfo.OS == RuntimeInfo.Platform.Windows)
            {
                Add(new SettingsCheckbox
                {
                    LabelText = "在游戏时禁用Windows键",
                    Current = config.GetBindable<bool>(OsuSetting.GameplayDisableWinKey)
                });
            }
        }
    }
}
