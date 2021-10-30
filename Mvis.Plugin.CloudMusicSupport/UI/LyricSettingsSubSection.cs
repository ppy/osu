using M.Resources.Localisation.LLin;
using M.Resources.Localisation.LLin.Plugins;
using Mvis.Plugin.CloudMusicSupport.Config;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays.Settings;
using osu.Game.Screens.LLin.Plugins;
using osu.Game.Screens.LLin.Plugins.Config;

namespace Mvis.Plugin.CloudMusicSupport.UI
{
    public class LyricSettingsSubSection : PluginSettingsSubSection
    {
        public LyricSettingsSubSection(LLinPlugin plugin)
            : base(plugin)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (LyricConfigManager)ConfigManager;

            SettingsCheckbox useDrawablePoolCheckBox;
            SettingsCheckbox autoScrollChechBox;
            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = LLinGenericStrings.EnablePlugin,
                    Current = config.GetBindable<bool>(LyricSettings.EnablePlugin)
                },
                useDrawablePoolCheckBox = new SettingsCheckbox
                {
                    LabelText = CloudMusicStrings.UseDrawablePool,
                    Current = config.GetBindable<bool>(LyricSettings.UseDrawablePool)
                },
                new SettingsCheckbox
                {
                    LabelText = CloudMusicStrings.SaveLyricOnDownloadedMain,
                    Current = config.GetBindable<bool>(LyricSettings.SaveLrcWhenFetchFinish),
                    TooltipText = CloudMusicStrings.SaveLyricOnDownloadedSub
                },
                new SettingsCheckbox
                {
                    LabelText = CloudMusicStrings.DisableShader,
                    Current = config.GetBindable<bool>(LyricSettings.NoExtraShadow)
                },
                new SettingsSlider<double>
                {
                    LabelText = CloudMusicStrings.GlobalOffsetMain,
                    Current = config.GetBindable<double>(LyricSettings.LyricOffset),
                    TooltipText = CloudMusicStrings.GlobalOffsetSub
                },
                new SettingsSlider<float>
                {
                    LabelText = CloudMusicStrings.LyricFadeInDuration,
                    Current = config.GetBindable<float>(LyricSettings.LyricFadeInDuration)
                },
                new SettingsSlider<float>
                {
                    LabelText = CloudMusicStrings.LyricFadeOutDuration,
                    Current = config.GetBindable<float>(LyricSettings.LyricFadeOutDuration)
                },
                autoScrollChechBox = new SettingsCheckbox
                {
                    LabelText = CloudMusicStrings.LyricAutoScrollMain,
                    Current = config.GetBindable<bool>(LyricSettings.AutoScrollToCurrent)
                },
                new SettingsDropdown<Anchor>
                {
                    LabelText = CloudMusicStrings.LocationDirection,
                    Current = config.GetBindable<Anchor>(LyricSettings.LyricDirection),
                    Items = new[]
                    {
                        Anchor.TopLeft,
                        Anchor.TopCentre,
                        Anchor.TopRight,
                        Anchor.CentreLeft,
                        Anchor.Centre,
                        Anchor.CentreRight,
                        Anchor.BottomLeft,
                        Anchor.BottomCentre,
                        Anchor.BottomRight,
                    }
                },
                new SettingsSlider<float>
                {
                    LabelText = CloudMusicStrings.PositionX,
                    Current = config.GetBindable<float>(LyricSettings.LyricPositionX),
                    DisplayAsPercentage = true
                },
                new SettingsSlider<float>
                {
                    LabelText = CloudMusicStrings.PositionY,
                    Current = config.GetBindable<float>(LyricSettings.LyricPositionY),
                    DisplayAsPercentage = true
                }
            };

            useDrawablePoolCheckBox.WarningText = CloudMusicStrings.ExperimentalWarning;
            autoScrollChechBox.WarningText = CloudMusicStrings.LyricAutoScrollSub;
        }
    }
}
