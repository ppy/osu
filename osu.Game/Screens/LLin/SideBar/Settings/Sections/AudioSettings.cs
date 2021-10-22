// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Screens.LLin.SideBar.Settings.Items;

namespace osu.Game.Screens.LLin.SideBar.Settings.Sections
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
                new SettingsSliderPiece<double>
                {
                    Icon = FontAwesome.Solid.Forward,
                    Description = "播放速度",
                    Bindable = config.GetBindable<double>(MSetting.MvisMusicSpeed),
                    DisplayAsPercentage = true,
                    TransferValueOnCommit = true
                },
                new SettingsTogglePiece
                {
                    Icon = FontAwesome.Solid.PeopleCarry,
                    Description = "调整音调",
                    Bindable = config.GetBindable<bool>(MSetting.MvisAdjustMusicWithFreq),
                    TooltipText = "暂不支持调整故事版的音调"
                },
                new SettingsTogglePiece
                {
                    Icon = FontAwesome.Solid.HeadphonesAlt,
                    Description = "夜核节拍器",
                    Bindable = config.GetBindable<bool>(MSetting.MvisEnableNightcoreBeat),
                    TooltipText = "动次打次动次打次"
                },
            });
        }
    }
}
