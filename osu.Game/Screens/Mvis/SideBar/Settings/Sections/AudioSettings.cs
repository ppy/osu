// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;

namespace osu.Game.Screens.Mvis.SideBar.Settings.Sections
{
    public class AudioSettings : Section
    {
        public AudioSettings()
        {
            Title = "音频设置";
        }

        [BackgroundDependencyLoader]
        private void load(MConfigManager config)
        {
            AddRange(new Drawable[]
            {
                new SliderSettingsPiece<double>
                {
                    Icon = FontAwesome.Solid.Forward,
                    Description = "播放速度",
                    Bindable = config.GetBindable<double>(MSetting.MvisMusicSpeed),
                    DisplayAsPercentage = true
                },
                new SettingsToggleablePiece
                {
                    Icon = FontAwesome.Solid.PeopleCarry,
                    Description = "调整音调",
                    Bindable = config.GetBindable<bool>(MSetting.MvisAdjustMusicWithFreq),
                    TooltipText = "暂不支持调整故事版的音调"
                },
                new SettingsToggleablePiece
                {
                    Icon = FontAwesome.Solid.Headphones,
                    Description = "夜核节拍器",
                    Bindable = config.GetBindable<bool>(MSetting.MvisEnableNightcoreBeat),
                    TooltipText = "动次打次动次打次"
                },
            });
        }
    }
}
