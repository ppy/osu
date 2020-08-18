// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Settings.Sections.General
{
    public class MvisAudioSettings : SettingsSubsection
    {
        protected override string Header => "音频";

        [BackgroundDependencyLoader]
        private void load(MfConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsSlider<double>
                {
                    LabelText = "播放速度",
                    Bindable = config.GetBindable<double>(MfSetting.MvisMusicSpeed),
                    KeyboardStep = 0.1f,
                    DisplayAsPercentage = true,
                    TransferValueOnCommit = true
                },
                new SettingsCheckbox
                {
                    LabelText = "调整播放速度时也同时调整音调",
                    Bindable = config.GetBindable<bool>(MfSetting.MvisAdjustMusicWithFreq),
                    TooltipText = "请注意: 播放速度调整暂不支持故事版音频, 调整故事版音频请手动前往歌曲选择添加加速/减速mod"
                },
                new SettingsCheckbox
                {
                    LabelText = "夜核节拍器",
                    Bindable = config.GetBindable<bool>(MfSetting.MvisEnableNightcoreBeat),
                    TooltipText = "动次打次动次打次"
                }
            };
        }
    }
}
