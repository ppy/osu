// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Screens.Import;

namespace osu.Game.Overlays.Settings.Sections.Debug
{
    public class GeneralSettings : SettingsSubsection
    {
        protected override string Header => "General";

        [BackgroundDependencyLoader(true)]
        private void load(FrameworkDebugConfigManager config, FrameworkConfigManager frameworkConfig, OsuGame game)
        {
            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "Show log overlay",
                    Current = frameworkConfig.GetBindable<bool>(FrameworkSetting.ShowLogOverlay)
                },
                new SettingsCheckbox
                {
                    LabelText = "Bypass front-to-back render pass",
                    Current = config.GetBindable<bool>(DebugSetting.BypassFrontToBackPass)
                }
            };
            Add(new SettingsButton
            {
                Text = "Import files",
                Action = () => game?.PerformFromScreen(menu => menu.Push(new FileImportScreen()))
            });
        }
    }
}
