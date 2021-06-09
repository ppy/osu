using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Screens.Mvis.Plugins;
using osu.Game.Screens.Mvis.Plugins.Types;
using osu.Game.Screens.Mvis.SideBar.Settings.Items;

namespace osu.Game.Screens.Mvis.SideBar.Settings.Sections
{
    public class BaseSettings : Section
    {
        private readonly BindableFloat iR = new BindableFloat();
        private readonly BindableFloat iG = new BindableFloat();
        private readonly BindableFloat iB = new BindableFloat();

        public BaseSettings()
        {
            Title = "基本设置";
        }

        [BackgroundDependencyLoader]
        private void load(MConfigManager config, MvisPluginManager pluginManager, MvisScreen mvisScreen)
        {
            config.BindWith(MSetting.MvisInterfaceRed, iR);
            config.BindWith(MSetting.MvisInterfaceGreen, iG);
            config.BindWith(MSetting.MvisInterfaceBlue, iB);

            var plugins = new List<IProvideAudioControlPlugin>();
            var currentAudioControlPlugin = config.Get<string>(MSetting.MvisCurrentAudioProvider);
            IProvideAudioControlPlugin currentprovider = mvisScreen.MusicControllerWrapper;

            foreach (var pl in pluginManager.GetAllPlugins(false))
            {
                if (pl is IProvideAudioControlPlugin pacp)
                {
                    plugins.Add(pacp);

                    var type = pl.GetType();

                    if (currentAudioControlPlugin == $"{type.Namespace}+{type.Name}")
                    {
                        currentprovider = pacp;
                    }
                }
            }

            plugins.Add(mvisScreen.MusicControllerWrapper);

            Bindable<IProvideAudioControlPlugin> configBindable;
            AddRange(new Drawable[]
            {
                new SettingsSliderPiece<float>
                {
                    Description = "界面主题色(红)",
                    Bindable = iR
                },
                new SettingsSliderPiece<float>
                {
                    Description = "界面主题色(绿)",
                    Bindable = iG
                },
                new SettingsSliderPiece<float>
                {
                    Description = "界面主题色(蓝)",
                    Bindable = iB
                },
                new ProviderSettingsPiece<IProvideAudioControlPlugin>
                {
                    Icon = FontAwesome.Solid.Bullseye,
                    Description = "音乐控制插件",
                    Bindable = configBindable = new Bindable<IProvideAudioControlPlugin>
                    {
                        Value = currentprovider,
                        Default = mvisScreen.MusicControllerWrapper
                    },
                    Values = plugins
                },
                new SettingsSliderPiece<float>
                {
                    Icon = FontAwesome.Solid.SolarPanel,
                    Description = "背景模糊",
                    Bindable = config.GetBindable<float>(MSetting.MvisBgBlur),
                    DisplayAsPercentage = true
                },
                new SettingsSliderPiece<float>
                {
                    Icon = FontAwesome.Regular.Sun,
                    Description = "空闲时的背景亮度",
                    Bindable = config.GetBindable<float>(MSetting.MvisIdleBgDim),
                    DisplayAsPercentage = true
                },
                new SettingsTogglePiece
                {
                    Icon = FontAwesome.Regular.ArrowAltCircleUp,
                    Description = "置顶Proxy",
                    Bindable = config.GetBindable<bool>(MSetting.MvisStoryboardProxy),
                    TooltipText = "让所有Proxy显示在前景上方"
                },
                new SettingsTogglePiece
                {
                    Icon = FontAwesome.Solid.Clock,
                    Description = "启用背景动画",
                    Bindable = config.GetBindable<bool>(MSetting.MvisEnableBgTriangles),
                    TooltipText = "如果条件允许,播放器将会在背景显示动画"
                },
            });

            configBindable.BindValueChanged(v =>
            {
                if (v.NewValue == null)
                {
                    config.SetValue(MSetting.MvisCurrentAudioProvider, string.Empty);
                    return;
                }

                var pl = (MvisPlugin)v.NewValue;
                var type = pl.GetType();

                config.SetValue(MSetting.MvisCurrentAudioProvider, $"{type.Namespace}+{type.Name}");
            });
        }

        private class ProviderSettingsPiece<T> : SettingsListPiece<T>
            where T : IProvideAudioControlPlugin
        {
            protected override string GetValueText(T newValue)
            {
                return (newValue as MvisPlugin)?.Name ?? "???";
            }
        }
    }
}
