using M.Resources.Localisation.LLin;
using M.Resources.Localisation.LLin.Plugins;
using Mvis.Plugin.CloudMusicSupport.Config;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Screens.LLin.Plugins;
using osu.Game.Screens.LLin.Plugins.Config;
using osu.Game.Screens.LLin.SideBar.Settings.Items;

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
                    Description = LLinGenericStrings.EnablePlugin,
                    Bindable = config.GetBindable<bool>(LyricSettings.EnablePlugin)
                },
                new SettingsTogglePiece
                {
                    Icon = FontAwesome.Solid.SwimmingPool,
                    Description = CloudMusicStrings.UseDrawablePool,
                    TooltipText = CloudMusicStrings.ExperimentalWarning,
                    Bindable = config.GetBindable<bool>(LyricSettings.UseDrawablePool)
                },
                new SettingsTogglePiece
                {
                    Icon = FontAwesome.Solid.Save,
                    Description = CloudMusicStrings.SaveLyricOnDownloadedMain,
                    Bindable = config.GetBindable<bool>(LyricSettings.SaveLrcWhenFetchFinish),
                    TooltipText = CloudMusicStrings.SaveLyricOnDownloadedSub
                },
                new SettingsTogglePiece
                {
                    Icon = FontAwesome.Solid.FillDrip,
                    Description = CloudMusicStrings.DisableShader,
                    Bindable = config.GetBindable<bool>(LyricSettings.NoExtraShadow)
                },
                new SettingsSliderPiece<double>
                {
                    Description = CloudMusicStrings.GlobalOffsetMain,
                    Bindable = config.GetBindable<double>(LyricSettings.LyricOffset),
                    TooltipText = CloudMusicStrings.GlobalOffsetSub,
                },
                new SettingsSliderPiece<float>
                {
                    Description = CloudMusicStrings.LyricFadeInDuration,
                    Bindable = config.GetBindable<float>(LyricSettings.LyricFadeInDuration)
                },
                new SettingsSliderPiece<float>
                {
                    Description = CloudMusicStrings.LyricFadeOutDuration,
                    Bindable = config.GetBindable<float>(LyricSettings.LyricFadeOutDuration)
                },
                new SettingsTogglePiece
                {
                    Description = CloudMusicStrings.LyricAutoScrollMain,
                    TooltipText = CloudMusicStrings.LyricAutoScrollSub,
                    Bindable = config.GetBindable<bool>(LyricSettings.AutoScrollToCurrent)
                },
                new SettingsAnchorPiece
                {
                    Description = CloudMusicStrings.LocationDirection,
                    Icon = FontAwesome.Solid.Anchor,
                    Bindable = config.GetBindable<Anchor>(LyricSettings.LyricDirection)
                },
                new SettingsSliderPiece<float>
                {
                    Description = CloudMusicStrings.PositionX,
                    Bindable = config.GetBindable<float>(LyricSettings.LyricPositionX),
                    DisplayAsPercentage = true
                },
                new SettingsSliderPiece<float>
                {
                    Description = CloudMusicStrings.PositionY,
                    Bindable = config.GetBindable<float>(LyricSettings.LyricPositionY),
                    DisplayAsPercentage = true
                }
            });
        }

        public LyricSidebarSection(LLinPlugin plugin)
            : base(plugin)
        {
        }
    }
}
