using Mvis.Plugin.CloudMusicSupport.Config;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Screens.Mvis.Plugins;
using osu.Game.Screens.Mvis.Plugins.Config;
using osu.Game.Screens.Mvis.SideBar.Settings.Items;

namespace Mvis.Plugin.CloudMusicSupport.UI
{
    public class LyricSidebarSection : PluginSidebarSettingsSection
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (LyricConfigManager)ConfigManager;

            AddRange(new Drawable[]
            {
                new SettingsTogglePiece
                {
                    Description = "启用本插件",
                    Bindable = config.GetBindable<bool>(LyricSettings.EnablePlugin)
                },
                new SettingsTogglePiece
                {
                    Icon = FontAwesome.Solid.SwimmingPool,
                    Description = "使用DrawablePool",
                    TooltipText = "试验性功能！",
                    Bindable = config.GetBindable<bool>(LyricSettings.UseDrawablePool)
                },
                new SettingsTogglePiece
                {
                    Icon = FontAwesome.Solid.Save,
                    Description = "自动保存歌词到本地",
                    Bindable = config.GetBindable<bool>(LyricSettings.SaveLrcWhenFetchFinish),
                    TooltipText = "歌词将保存在\"custom/lyrics/beatmap-{ID}.json\"中"
                },
                new SettingsTogglePiece
                {
                    Icon = FontAwesome.Solid.FillDrip,
                    Description = "禁用额外阴影",
                    Bindable = config.GetBindable<bool>(LyricSettings.NoExtraShadow),
                    TooltipText = "不要给歌词文本添加额外的阴影效果"
                },
                new SettingsSliderPiece<double>
                {
                    Description = "全局歌词偏移(毫秒)",
                    Bindable = config.GetBindable<double>(LyricSettings.LyricOffset),
                    TooltipText = "如果当前歌曲歌词太早或太晚，拖动这里的滑条试试"
                },
                new SettingsSliderPiece<float>
                {
                    Description = "歌词淡入时间",
                    TooltipText = "调整歌词淡入动画要花多长时间",
                    Bindable = config.GetBindable<float>(LyricSettings.LyricFadeInDuration)
                },
                new SettingsSliderPiece<float>
                {
                    Description = "歌词淡出时间",
                    TooltipText = "调整歌词淡出动画要花多长时间",
                    Bindable = config.GetBindable<float>(LyricSettings.LyricFadeOutDuration)
                }
            });
        }

        public LyricSidebarSection(MvisPlugin plugin)
            : base(plugin)
        {
        }
    }
}
