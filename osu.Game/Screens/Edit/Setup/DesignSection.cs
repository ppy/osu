// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Globalization;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterfaceV2;
using osuTK;

namespace osu.Game.Screens.Edit.Setup
{
    internal partial class DesignSection : SetupSection
    {
        protected LabelledSwitchButton EnableCountdown;

        protected FillFlowContainer CountdownSettings;
        protected LabelledEnumDropdown<CountdownType> CountdownSpeed;
        protected LabelledNumberBox CountdownOffset;

        private LabelledSwitchButton widescreenSupport;
        private LabelledSwitchButton epilepsyWarning;
        private LabelledSwitchButton letterboxDuringBreaks;
        private LabelledSwitchButton samplesMatchPlaybackRate;

        public override LocalisableString Title => "设计";

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new[]
            {
                EnableCountdown = new LabelledSwitchButton
                {
                    Label = "启用倒计时",
                    Current = { Value = Beatmap.BeatmapInfo.Countdown != CountdownType.None },
                    Description = "启用后，将会在谱面开头插入\"Are you ready? 3, 2, 1, GO!\" 的倒计时, 让玩家可以有时间准备"
                },
                CountdownSettings = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(10),
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        CountdownSpeed = new LabelledEnumDropdown<CountdownType>
                        {
                            Label = "倒计时速度",
                            Current = { Value = Beatmap.BeatmapInfo.Countdown != CountdownType.None ? Beatmap.BeatmapInfo.Countdown : CountdownType.Normal },
                            Items = Enum.GetValues<CountdownType>().Where(type => type != CountdownType.None)
                        },
                        CountdownOffset = new LabelledNumberBox
                        {
                            Label = "倒计时偏移",
                            Current = { Value = Beatmap.BeatmapInfo.CountdownOffset.ToString() },
                            Description = "如果倒计时出现的时间听上去不对，可以在这里设置让他早几拍",
                        }
                    }
                },
                Empty(),
                widescreenSupport = new LabelledSwitchButton
                {
                    Label = "宽屏支持",
                    Description = "允许故事版全屏显示，而不是标准的4:3",
                    Current = { Value = Beatmap.BeatmapInfo.WidescreenStoryboard }
                },
                epilepsyWarning = new LabelledSwitchButton
                {
                    Label = "光敏性癫痫警告",
                    Description = "如果故事版或视频有快速的闪光，建议开启",
                    Current = { Value = Beatmap.BeatmapInfo.EpilepsyWarning }
                },
                letterboxDuringBreaks = new LabelledSwitchButton
                {
                    Label = "遮罩",
                    Description = "在休息时段添加上下两条黑色遮罩以提供电影效果",
                    Current = { Value = Beatmap.BeatmapInfo.LetterboxInBreaks }
                },
                samplesMatchPlaybackRate = new LabelledSwitchButton
                {
                    Label = "采样跟随播放速度",
                    Description = "启用后，所有音频采样将根据当前调速Mod加或减速播放",
                    Current = { Value = Beatmap.BeatmapInfo.SamplesMatchPlaybackRate }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            EnableCountdown.Current.BindValueChanged(_ => updateCountdownSettingsVisibility(), true);

            EnableCountdown.Current.BindValueChanged(_ => updateBeatmap());
            CountdownSpeed.Current.BindValueChanged(_ => updateBeatmap());
            CountdownOffset.OnCommit += (_, _) => onOffsetCommitted();

            widescreenSupport.Current.BindValueChanged(_ => updateBeatmap());
            epilepsyWarning.Current.BindValueChanged(_ => updateBeatmap());
            letterboxDuringBreaks.Current.BindValueChanged(_ => updateBeatmap());
            samplesMatchPlaybackRate.Current.BindValueChanged(_ => updateBeatmap());
        }

        private void updateCountdownSettingsVisibility() => CountdownSettings.FadeTo(EnableCountdown.Current.Value ? 1 : 0);

        private void onOffsetCommitted()
        {
            updateBeatmap();
            // update displayed text to ensure parsed value matches display (i.e. if empty string was provided).
            CountdownOffset.Current.Value = Beatmap.BeatmapInfo.CountdownOffset.ToString(CultureInfo.InvariantCulture);
        }

        private void updateBeatmap()
        {
            Beatmap.BeatmapInfo.Countdown = EnableCountdown.Current.Value ? CountdownSpeed.Current.Value : CountdownType.None;
            Beatmap.BeatmapInfo.CountdownOffset = int.TryParse(CountdownOffset.Current.Value, NumberStyles.None, CultureInfo.InvariantCulture, out int offset) ? offset : 0;

            Beatmap.BeatmapInfo.WidescreenStoryboard = widescreenSupport.Current.Value;
            Beatmap.BeatmapInfo.EpilepsyWarning = epilepsyWarning.Current.Value;
            Beatmap.BeatmapInfo.LetterboxInBreaks = letterboxDuringBreaks.Current.Value;
            Beatmap.BeatmapInfo.SamplesMatchPlaybackRate = samplesMatchPlaybackRate.Current.Value;

            Beatmap.SaveState();
        }
    }
}
