// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Threading.Tasks;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Localisation;
using osu.Game.Database;
using osu.Game.Localisation;
using osu.Game.Skinning;

namespace osu.Game.Overlays.Settings.Sections.Maintenance
{
    public class SkinSettings : SettingsSubsection
    {
        protected override LocalisableString Header => "Skins";
        private SettingsButton importSkinsButton;
        private SettingsButton deleteSkinsButton;

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(SkinManager skins, [CanBeNull] LegacyImportManager legacyImportManager, IDialogOverlay dialogOverlay)
        {
            if (legacyImportManager?.SupportsImportFromStable == true)
            {
                Add(importSkinsButton = new SettingsButton
                {
                    Text = MaintenanceSettingsStrings.ImportSkinsFromStable,
                    Action = () =>
                    {
                        importSkinsButton.Enabled.Value = false;
                        legacyImportManager.ImportFromStableAsync(StableContent.Skins).ContinueWith(t => Schedule(() => importSkinsButton.Enabled.Value = true));
                    }
                });
            }

            Add(deleteSkinsButton = new DangerousSettingsButton
            {
                Text = MaintenanceSettingsStrings.DeleteAllSkins,
                Action = () =>
                {
                    dialogOverlay?.Push(new MassDeleteConfirmationDialog(() =>
                    {
                        deleteSkinsButton.Enabled.Value = false;
                        Task.Run(() => skins.Delete()).ContinueWith(t => Schedule(() => deleteSkinsButton.Enabled.Value = true));
                    }));
                }
            });
        }
    }
}
