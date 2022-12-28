// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Configuration.AccelUtils;

#nullable disable

namespace osu.Game.Overlays.Settings.Sections.Mf
{
    public partial class MfSettings : SettingsSubsection
    {
        private SettingsCheckbox systemCursor = null!;
        private SettingsTextBoxWithIndicator accelTextBox = null!;
        private SettingsTextBoxWithIndicator previewAccelTextBox = null!;
        private SettingsTextBoxWithIndicator coverAccelTextBox = null!;

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
                    TooltipText = "请将要显示在开屏页的图片放在custom下, 并更名为startup或avatarlogo",
                    Current = config.GetBindable<bool>(MSetting.UseCustomGreetingPicture)
                },
                new SettingsCheckbox
                {
                    LabelText = "默认使用加速源",
                    TooltipText = "启用后所有谱面预览和封面将优先从指定的加速源获取, 该选项不会影响正在进行中的请求",
                    Current = config.GetBindable<bool>(MSetting.UseAccelForDefault)
                },
                accelTextBox = new SettingsTextBoxWithIndicator
                {
                    LabelText = "下载加速源",
                    Current = config.GetBindable<string>(MSetting.AccelSource)
                },
                previewAccelTextBox = new SettingsTextBoxWithIndicator
                {
                    LabelText = "音频预览加速源",
                    Current = config.GetBindable<string>(MSetting.TrackPreviewAccelSource)
                },
                coverAccelTextBox = new SettingsTextBoxWithIndicator
                {
                    LabelText = "封面加速源",
                    Current = config.GetBindable<string>(MSetting.CoverAccelSource)
                }
            };

            accelTextBox.Current.BindValueChanged(v => onPreviewOrCoverAccelChanged(v, accelTextBox), true);
            previewAccelTextBox.Current.BindValueChanged(v => onPreviewOrCoverAccelChanged(v, previewAccelTextBox, dictOverrides), true);
            coverAccelTextBox.Current.BindValueChanged(v => onPreviewOrCoverAccelChanged(v, coverAccelTextBox, dictOverrides), true);
        }

        private static readonly IBeatmapInfo dummy_beatmap_info = new BeatmapInfo
        {
            OnlineID = 114514
        };

        private readonly IDictionary<string, object> dictOverrides = new Dictionary<string, object>
        {
            ["NOVIDEO"] = "此选项不适用于预览或封面"
        };

        private void onPreviewOrCoverAccelChanged(ValueChangedEvent<string> v,
                                                  SettingsTextBoxWithIndicator target,
                                                  IDictionary<string, object> overrides = null)
        {
            target.ChangeState(SettingsTextBoxWithIndicator.ParseState.Working, null);

            IList<string> errors;
            string parseResult;
            bool success = v.NewValue.TryParseAccelUrl(dummy_beatmap_info, out parseResult, out errors, overrides);

            target.ChangeState(success
                ? SettingsTextBoxWithIndicator.ParseState.Success
                : SettingsTextBoxWithIndicator.ParseState.Failed, parseResult, errors);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            systemCursor.SetNoticeText("与高精度模式、数位板功能冲突。\n启用后会导致上述功能失效或光标鬼畜。", true);
        }
    }
}
