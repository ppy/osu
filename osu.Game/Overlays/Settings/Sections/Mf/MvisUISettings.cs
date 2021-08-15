// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Mvis.Plugins;
using osu.Game.Screens.Mvis.Plugins.Types;
using osu.Game.Screens.Mvis.SideBar.Tabs;

namespace osu.Game.Overlays.Settings.Sections.Mf
{
    public class MvisUISettings : SettingsSubsection
    {
        protected override LocalisableString Header => "界面";
        private readonly BindableFloat iR = new BindableFloat();
        private readonly BindableFloat iG = new BindableFloat();
        private readonly BindableFloat iB = new BindableFloat();
        private ColourPreviewer preview;
        private FunctionBarPluginDropDown dropdown;

        [BackgroundDependencyLoader]
        private void load(MConfigManager config, MvisPluginManager pluginManager)
        {
            config.BindWith(MSetting.MvisInterfaceRed, iR);
            config.BindWith(MSetting.MvisInterfaceGreen, iG);
            config.BindWith(MSetting.MvisInterfaceBlue, iB);

            Children = new Drawable[]
            {
                new SettingsSlider<float>
                {
                    LabelText = "背景模糊",
                    Current = config.GetBindable<float>(MSetting.MvisBgBlur),
                    DisplayAsPercentage = true,
                    KeyboardStep = 0.01f,
                },
                new SettingsSlider<float>
                {
                    LabelText = "空闲时的背景亮度",
                    Current = config.GetBindable<float>(MSetting.MvisIdleBgDim),
                    DisplayAsPercentage = true,
                    KeyboardStep = 0.01f,
                },
                new SettingsSlider<float>
                {
                    LabelText = "界面主题色(红)",
                    Current = iR,
                    KeyboardStep = 1,
                    TransferValueOnCommit = false
                },
                new SettingsSlider<float>
                {
                    LabelText = "界面主题色(绿)",
                    Current = iG,
                    KeyboardStep = 1,
                    TransferValueOnCommit = false
                },
                new SettingsSlider<float>
                {
                    LabelText = "界面主题色(蓝)",
                    Current = iB,
                    KeyboardStep = 1,
                    TransferValueOnCommit = false
                },
                preview = new ColourPreviewer(),
                new SettingsEnumDropdown<TabControlPosition>
                {
                    LabelText = "TabControl位置",
                    Current = config.GetBindable<TabControlPosition>(MSetting.MvisTabControlPosition)
                },
                new SettingsCheckbox
                {
                    LabelText = "置顶Proxy",
                    Current = config.GetBindable<bool>(MSetting.MvisStoryboardProxy),
                    TooltipText = "让所有Proxy显示在前景上方"
                },
                new SettingsCheckbox
                {
                    LabelText = "启用背景动画",
                    Current = config.GetBindable<bool>(MSetting.MvisEnableBgTriangles),
                    TooltipText = "如果条件允许,播放器将会在背景显示动画"
                },
                dropdown = new FunctionBarPluginDropDown
                {
                    LabelText = "底栏插件"
                }
            };

            var plugins = pluginManager.GetAllFunctionBarProviders();
            plugins.Insert(0, pluginManager.DummyFunctionBar);

            var currentFunctionBar = config.Get<string>(MSetting.MvisCurrentFunctionBar);

            foreach (var pl in plugins)
            {
                if (currentFunctionBar == pluginManager.ToPath(pl))
                {
                    dropdown.Current.Value = pl;
                }
            }

            dropdown.Items = plugins;
            dropdown.Current.BindValueChanged(v =>
            {
                if (v.NewValue == null)
                {
                    config.SetValue(MSetting.MvisCurrentFunctionBar, string.Empty);
                    return;
                }

                var pl = (MvisPlugin)v.NewValue;

                config.SetValue(MSetting.MvisCurrentFunctionBar, pluginManager.ToPath(pl));
            });
        }

        protected override void LoadComplete()
        {
            iR.BindValueChanged(_ => updateColor());
            iG.BindValueChanged(_ => updateColor());
            iB.BindValueChanged(_ => updateColor(), true);
        }

        private void updateColor() => preview.UpdateColor(iR.Value, iG.Value, iB.Value);

        private class FunctionBarPluginDropDown : SettingsDropdown<IFunctionBarProvider>
        {
            protected override OsuDropdown<IFunctionBarProvider> CreateDropdown()
                => new PluginDropDownControl();

            private class PluginDropDownControl : DropdownControl
            {
                protected override LocalisableString GenerateItemText(IFunctionBarProvider item)
                {
                    return ((MvisPlugin)item).Name;
                }
            }
        }
    }
}
