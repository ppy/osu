// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Game.Configuration;
using osu.Game.Database;

namespace osu.Game.Overlays.Settings.Sections.Mf
{
    public class MfSettings : SettingsSubsection
    {
        private SettingsCheckbox systemCursor;
        private SettingsTextBoxWithIndicator accelTextBox;
        protected override LocalisableString Header => "Mf-osu";

        [BackgroundDependencyLoader]
        private void load(MConfigManager config, OsuConfigManager osuConfig, GameHost host)
        {
            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "启用Mf自定义UI",
                    TooltipText = "启用以获得mfosu提供的默认界面体验, "
                                  + "禁用以获得接近原版lazer提供的界面体验",
                    Current = config.GetBindable<bool>(MSetting.OptUI)
                },
                new SettingsCheckbox
                {
                    LabelText = "启用三角形粒子动画",
                    Current = config.GetBindable<bool>(MSetting.TrianglesEnabled)
                },
                new SettingsCheckbox
                {
                    LabelText = "隐藏Disclaimer",
                    TooltipText = "要跳过Disclaimer, 自定义开屏页背景也需要关闭。",
                    Current = config.GetBindable<bool>(MSetting.DoNotShowDisclaimer)
                },
                new SettingsSlider<float>
                {
                    LabelText = "立体音效增益",
                    Current = config.GetBindable<float>(MSetting.SamplePlaybackGain),
                    DisplayAsPercentage = true,
                    KeyboardStep = 0.01f,
                },
                new SettingsSlider<float>
                {
                    LabelText = "歌曲选择界面背景模糊",
                    Current = config.GetBindable<float>(MSetting.SongSelectBgBlur),
                    DisplayAsPercentage = true,
                    KeyboardStep = 0.01f,
                },
                new SettingsCheckbox
                {
                    LabelText = "启动后直接进入选歌界面",
                    TooltipText = "仅在开场样式为\"略过开场\"时生效",
                    Current = config.GetBindable<bool>(MSetting.IntroLoadDirectToSongSelect)
                },
                systemCursor = new SettingsCheckbox
                {
                    LabelText = "使用系统光标",
                    Current = config.GetBindable<bool>(MSetting.UseSystemCursor)
                },
                new SettingsCheckbox
                {
                    LabelText = "使用自定义开屏页背景",
                    TooltipText = "请将要显示在开屏页的图片放在custom下, 并更名为avatarlogo",
                    Current = config.GetBindable<bool>(MSetting.UseCustomGreetingPicture)
                },
                new SettingsCheckbox
                {
                    LabelText = "默认使用下载加速",
                    TooltipText = "启用后谱面信息界面以外的下图功能将默认使用指定的加速源。 这也将影响所有谱面预览和封面功能, 但不会影响已完成或正在进行中的请求",
                    Current = config.GetBindable<bool>(MSetting.UseAccelForDefault)
                },
                accelTextBox = new SettingsTextBoxWithIndicator
                {
                    LabelText = "加速源",
                    Current = config.GetBindable<string>(MSetting.AccelSource)
                }
            };

            accelTextBox.Current.BindValueChanged(onAccelUrlChanged);
        }

        private void onAccelUrlChanged(ValueChangedEvent<string> v)
        {
            accelTextBox.ChangeState(SettingsTextBoxWithIndicator.ParseState.Working, null);

            var dict = new Dictionary<string, object>
            {
                ["BID"] = 114514,
                ["NOVIDEO"] = "(novideo 或 full)",
                ["TARGET"] = "(novideo 或 full)/114514 (图号)"
            };

            List<string> errors;
            string parseResult;
            bool success = v.NewValue.TryParse(dict, out parseResult, out errors);

            accelTextBox.ChangeState(success
                ? SettingsTextBoxWithIndicator.ParseState.Success
                : SettingsTextBoxWithIndicator.ParseState.Failed, parseResult, errors);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            systemCursor.WarningText = "与高精度模式、数位板功能冲突。\n启用后会导致上述功能失效或光标鬼畜。";
        }
    }
}
