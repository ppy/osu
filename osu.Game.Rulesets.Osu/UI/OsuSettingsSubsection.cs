// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Osu.Configuration;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Osu.UI
{
    public partial class OsuSettingsSubsection : RulesetSettingsSubsection
    {
        protected override LocalisableString Header => "osu!";

        public OsuSettingsSubsection(Ruleset ruleset)
            : base(ruleset)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (OsuRulesetConfigManager)Config;

            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = RulesetSettingsStrings.SnakingInSliders,
                    Current = config.GetBindable<bool>(OsuRulesetSetting.SnakingInSliders)
                },
                new SettingsCheckbox
                {
                    ClassicDefault = false,
                    LabelText = RulesetSettingsStrings.SnakingOutSliders,
                    Current = config.GetBindable<bool>(OsuRulesetSetting.SnakingOutSliders)
                },
                new SettingsCheckbox
                {
                    LabelText = RulesetSettingsStrings.CursorTrail,
                    Current = config.GetBindable<bool>(OsuRulesetSetting.ShowCursorTrail)
                },
                new SettingsCheckbox
                {
                    LabelText = "强制光标使用长拖尾",
                    Current = config.GetBindable<bool>(OsuRulesetSetting.CursorTrailForceLong)
                },
                new SettingsCheckbox
                {
                    LabelText = "隐藏300判定显示",
                    Current = config.GetBindable<bool>(OsuRulesetSetting.NoDraw300)
                },
                new SettingsEnumDropdown<PlayfieldBorderStyle>
                {
                    LabelText = RulesetSettingsStrings.PlayfieldBorderStyle,
                    Current = config.GetBindable<PlayfieldBorderStyle>(OsuRulesetSetting.PlayfieldBorderStyle),
                },
                new OsuSpriteText
                {
                    Text = "CursorDance设置",
                    Margin = new MarginPadding { Vertical = 15, Left = SettingsPanel.CONTENT_MARGINS },
                    Font = OsuFont.GetFont(weight: FontWeight.Bold)
                },
                new SettingsSlider<float, FramerateSlider>
                {
                    LabelText = "回放帧率",
                    Current = config.GetBindable<float>(OsuRulesetSetting.ReplayFramerate),
                    KeyboardStep = 10f
                },
                new SettingsEnumDropdown<OsuDanceMover>
                {
                    LabelText = "Dance移动方式",
                    Current = config.GetBindable<OsuDanceMover>(OsuRulesetSetting.DanceMover)
                },
                new SettingsSlider<float, AngleSlider>
                {
                    LabelText = "角度偏移",
                    Current = config.GetBindable<float>(OsuRulesetSetting.AngleOffset),
                    KeyboardStep = 1f / 18f
                },
                new SettingsSlider<float, MultiplierSlider>
                {
                    LabelText = "跳跃倍率(Jump Multiplier)",
                    Current = config.GetBindable<float>(OsuRulesetSetting.JumpMult),
                    KeyboardStep = 1f / 12f
                },
                new SettingsSlider<float, MultiplierSlider>
                {
                    LabelText = "下一跳倍率(Next Jump Multiplier)",
                    Current = config.GetBindable<float>(OsuRulesetSetting.NextJumpMult),
                    KeyboardStep = 1f / 12f
                },
                new SettingsCheckbox
                {
                    LabelText = "跳过堆叠角度(Skip Stack Angles)",
                    Current = config.GetBindable<bool>(OsuRulesetSetting.SkipStackAngles)
                },
                new SettingsCheckbox
                {
                    LabelText = "将Dance Mover应用至转盘",
                    Current = config.GetBindable<bool>(OsuRulesetSetting.SpinnerDance)
                },
                new SettingsCheckbox
                {
                    LabelText = "将Dance Mover应用至滑条",
                    Current = config.GetBindable<bool>(OsuRulesetSetting.SliderDance)
                },
                new SettingsSlider<float, MultiplierSlider>
                {
                    LabelText = "Dance转盘起始半径",
                    Current = config.GetBindable<float>(OsuRulesetSetting.SpinnerRadiusStart),
                    KeyboardStep = 1f / 12f
                },
                new SettingsSlider<float, MultiplierSlider>
                {
                    LabelText = "Dance转盘结束半径",
                    Current = config.GetBindable<float>(OsuRulesetSetting.SpinnerRadiusEnd),
                    KeyboardStep = 1f / 12f
                },
                new SettingsCheckbox
                {
                    LabelText = "遇到边界时反弹",
                    Current = config.GetBindable<bool>(OsuRulesetSetting.BorderBounce)
                },
                new SettingsCheckbox
                {
                    LabelText = "强制为转盘启用pippi移动方式",
                    Current = config.GetBindable<bool>(OsuRulesetSetting.PippiSpinner)
                },
                new SettingsCheckbox
                {
                    LabelText = "强制为note串启用pippi移动方式",
                    Current = config.GetBindable<bool>(OsuRulesetSetting.PippiStream)
                },
                new SettingsSlider<float, MultiplierSlider>
                {
                    LabelText = "时长倍率",
                    Current = config.GetBindable<float>(OsuRulesetSetting.DurationMult),
                    KeyboardStep = 0.05f
                },
                new SettingsSlider<float>
                {
                    LabelText = "时长倍率触发值",
                    Current = config.GetBindable<float>(OsuRulesetSetting.DurationTrigger),
                    KeyboardStep = 100f
                },
                new SettingsCheckbox
                {
                    LabelText = "限制反转",
                    Current = config.GetBindable<bool>(OsuRulesetSetting.RestrictInvert)
                },
                new SettingsCheckbox
                {
                    LabelText = "打串限制",
                    Current = config.GetBindable<bool>(OsuRulesetSetting.StreamRestrict)
                },
                new OsuSpriteText
                {
                    Text = "Momentum 移动设置",
                    Margin = new MarginPadding { Vertical = 15, Left = SettingsPanel.CONTENT_MARGINS },
                    Font = OsuFont.GetFont(weight: FontWeight.Bold)
                },
                new SettingsSlider<float, MultiplierSlider>
                {
                    LabelText = "打串倍率",
                    Current = config.GetBindable<float>(OsuRulesetSetting.StreamMult),
                    KeyboardStep = 1f / 12f
                },
                new SettingsSlider<float>
                {
                    LabelText = "限制角度",
                    Current = config.GetBindable<float>(OsuRulesetSetting.RestrictAngle),
                    KeyboardStep = 1
                },
                new SettingsSlider<float>
                {
                    LabelText = "Restrict angle add",
                    Current = config.GetBindable<float>(OsuRulesetSetting.RestrictAngleAdd),
                    KeyboardStep = 100f
                },
                new SettingsSlider<float>
                {
                    LabelText = "Restrict angle sub",
                    Current = config.GetBindable<float>(OsuRulesetSetting.RestrictAngleSub),
                    KeyboardStep = 100f
                },
                new SettingsSlider<float>
                {
                    LabelText = "打串区域",
                    Current = config.GetBindable<float>(OsuRulesetSetting.StreamArea),
                    KeyboardStep = 100f
                },
                new SettingsSlider<float>
                {
                    LabelText = "最小串距离",
                    Current = config.GetBindable<float>(OsuRulesetSetting.StreamMinimum)
                },
                new SettingsSlider<float>
                {
                    LabelText = "最大串距离",
                    Current = config.GetBindable<float>(OsuRulesetSetting.StreamMaximum)
                },
                new SettingsSlider<float>
                {
                    LabelText = "Bounce on equal pos",
                    Current = config.GetBindable<float>(OsuRulesetSetting.EqualPosBounce)
                },
                new SettingsCheckbox
                {
                    LabelText = "滑条预测",
                    Current = config.GetBindable<bool>(OsuRulesetSetting.SliderPredict)
                },
                new SettingsCheckbox
                {
                    LabelText = "角度插值",
                    Current = config.GetBindable<bool>(OsuRulesetSetting.InterpolateAngles)
                },
                new SettingsCheckbox
                {
                    LabelText = "翻转角度插值",
                    Current = config.GetBindable<bool>(OsuRulesetSetting.InvertAngleInterpolation)
                },
                new OsuSpriteText
                {
                    Text = "Bezier 移动设置",
                    Margin = new MarginPadding { Vertical = 15, Left = SettingsPanel.CONTENT_MARGINS },
                    Font = OsuFont.GetFont(weight: FontWeight.Bold)
                },
                new SettingsSlider<float>
                {
                    LabelText = "倍率",
                    Current = config.GetBindable<float>(OsuRulesetSetting.BezierAggressiveness),
                    KeyboardStep = 1
                },
                new SettingsSlider<float>
                {
                    LabelText = "滑条倍率",
                    Current = config.GetBindable<float>(OsuRulesetSetting.BezierSliderAggressiveness),
                    KeyboardStep = 1
                },
                new SettingsCheckbox
                {
                    LabelText = "跳过短滑条",
                    Current = config.GetBindable<bool>(OsuRulesetSetting.SkipShortSlider)
                },
                new OsuSpriteText
                {
                    Text = "Linear 移动设置",
                    Margin = new MarginPadding { Vertical = 15, Left = SettingsPanel.CONTENT_MARGINS },
                    Font = OsuFont.GetFont(weight: FontWeight.Bold)
                },
                new SettingsCheckbox
                {
                    LabelText = "Wait for preempt",
                    Current = config.GetBindable<bool>(OsuRulesetSetting.WaitForPreempt)
                }
            };
        }

        private partial class MultiplierSlider : RoundedSliderBar<float>
        {
            public override LocalisableString TooltipText => Current.Value.ToString("N3") + "x";
        }

        private partial class AngleSlider : RoundedSliderBar<float>
        {
            public override LocalisableString TooltipText => (Current.Value * 180).ToString("N2") + "deg";
        }

        private partial class FramerateSlider : RoundedSliderBar<float>
        {
            public override LocalisableString TooltipText => Current.Value.ToString("N0") + "fps";
        }
    }
}
