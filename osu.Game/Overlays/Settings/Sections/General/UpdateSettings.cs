// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings.Sections.Maintenance;

namespace osu.Game.Overlays.Settings.Sections.General
{
    public class UpdateSettings : SettingsSubsection
    {
        protected override string Header => "Updates";

        [BackgroundDependencyLoader]
        private void load(Storage storage, OsuConfigManager config, OsuGame game)
        {
            Add(new SettingsEnumDropdown<ReleaseStream>
            {
                LabelText = "Release stream",
                Bindable = config.GetBindable<ReleaseStream>(OsuSetting.ReleaseStream),
            });

            if (RuntimeInfo.IsDesktop)
            {
                Add(new SettingsButton
                {
                    Text = "Open osu! folder",
                    Action = storage.OpenInNativeExplorer,
                });

                Add(new SettingsButton
                {
                    Text = "Change folder location...",
                    Action = () => game.PerformFromScreen(menu => menu.Push(new MigrationSelectScreen()))
                });
            }
        }
    }
}
