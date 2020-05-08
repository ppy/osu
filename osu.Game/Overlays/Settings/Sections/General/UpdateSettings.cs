// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Platform;
using osu.Game.Configuration;
using osu.Game.Updater;

namespace osu.Game.Overlays.Settings.Sections.General
{
    public class UpdateSettings : SettingsSubsection
    {
        [Resolved(CanBeNull = true)]
        private UpdateManager updateManager { get; set; }

        protected override string Header => "Updates";

        [BackgroundDependencyLoader]
        private void load(Storage storage, OsuConfigManager config, OsuGameBase game)
        {
            Add(new SettingsEnumDropdown<ReleaseStream>
            {
                LabelText = "Release stream",
                Bindable = config.GetBindable<ReleaseStream>(OsuSetting.ReleaseStream),
            });

            // We should only display the button for UpdateManagers that do check for updates
            if (updateManager?.CanCheckForUpdate == true)
            {
                Add(new SettingsButton
                {
                    Text = "Check for updates",
                    Action = updateManager.CheckForUpdate,
                    Enabled = { Value = game.IsDeployedBuild }
                });
            }

            if (RuntimeInfo.IsDesktop)
            {
                Add(new SettingsButton
                {
                    Text = "Open osu! folder",
                    Action = storage.OpenInNativeExplorer,
                });
            }
        }
    }
}
