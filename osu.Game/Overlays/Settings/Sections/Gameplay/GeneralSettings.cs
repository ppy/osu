// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
                    Bindable = config.GetBindable<double>(OsuSetting.DimLevel),
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true
                },
                new SettingsSlider<double>
                {
                    LabelText = "背景模糊",
                    Bindable = config.GetBindable<double>(OsuSetting.BlurLevel),
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true
                },
                new SettingsCheckbox
                {
                    LabelText = "在休息时段使背景变亮",
                    Bindable = config.GetBindable<bool>(OsuSetting.LightenDuringBreaks)
                },
                new SettingsCheckbox
                {
                    LabelText = "显示分数计界面",
                    Bindable = config.GetBindable<bool>(OsuSetting.ShowInterface)
                },
                new SettingsCheckbox
                {
                    LabelText = "显示难度分布图",
                    Bindable = config.GetBindable<bool>(OsuSetting.ShowProgressGraph)
                },
                new SettingsCheckbox
                {
                    LabelText = "即使你无法失败,也显示分数计界面",
                    Bindable = config.GetBindable<bool>(OsuSetting.ShowHealthDisplayWhenCantFail),
                    Keywords = new[] { "hp", "bar" }
                },
                new SettingsCheckbox
                {
                    LabelText = "总是显示按键框",
                    Bindable = config.GetBindable<bool>(OsuSetting.KeyOverlay)
                },
                new SettingsCheckbox
                {
                    LabelText = "立体音效",
                    Bindable = config.GetBindable<bool>(OsuSetting.PositionalHitSounds)
                },
                new SettingsEnumDropdown<ScoreMeterType>
                {
                    LabelText = "分数计类型",
                    Bindable = config.GetBindable<ScoreMeterType>(OsuSetting.ScoreMeter)
                },
                new SettingsEnumDropdown<ScoringMode>
                {
                    LabelText = "分数显示模式",
                    Bindable = config.GetBindable<ScoringMode>(OsuSetting.ScoreDisplayMode)
                }
            };
        }
    }
}
