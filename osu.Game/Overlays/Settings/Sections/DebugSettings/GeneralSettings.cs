// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Framework.Screens;
using osu.Game.Localisation;
using osu.Game.Screens.Import;

namespace osu.Game.Overlays.Settings.Sections.DebugSettings
{
    public class GeneralSettings : SettingsSubsection
    {
        protected override LocalisableString Header => DebugSettingsStrings.GeneralHeader;

        [BackgroundDependencyLoader(true)]
        private void load(FrameworkDebugConfigManager config, FrameworkConfigManager frameworkConfig, OsuGame game)
        {
            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = DebugSettingsStrings.ShowLogOverlay,
                    Current = frameworkConfig.GetBindable<bool>(FrameworkSetting.ShowLogOverlay)
                },
                new SettingsCheckbox
                {
                    LabelText = DebugSettingsStrings.BypassFrontToBackPass,
                    Current = config.GetBindable<bool>(DebugSetting.BypassFrontToBackPass)
                }
            };
            Add(new SettingsButton
            {
                Text = DebugSettingsStrings.ImportFiles,
                Action = () => game?.PerformFromScreen(menu => menu.Push(new FileImportScreen()))
            });
        }
    }
}
