// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Localisation;
using osu.Game.Collections;
using osu.Game.Database;
using osu.Game.Localisation;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Overlays.Settings.Sections.Maintenance
{
    public class CollectionsSettings : SettingsSubsection
    {
        protected override LocalisableString Header => CommonStrings.Collections;

        private SettingsButton importCollectionsButton = null!;

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        [Resolved]
        private INotificationOverlay? notificationOverlay { get; set; }

        [BackgroundDependencyLoader]
        private void load(LegacyImportManager? legacyImportManager, IDialogOverlay? dialogOverlay)
        {
            if (legacyImportManager?.SupportsImportFromStable == true)
            {
                Add(importCollectionsButton = new SettingsButton
                {
                    Text = MaintenanceSettingsStrings.ImportCollectionsFromStable,
                    Action = () =>
                    {
                        importCollectionsButton.Enabled.Value = false;
                        legacyImportManager.ImportFromStableAsync(StableContent.Collections).ContinueWith(_ => Schedule(() => importCollectionsButton.Enabled.Value = true));
                    }
                });
            }

            Add(new DangerousSettingsButton
            {
                Text = MaintenanceSettingsStrings.DeleteAllCollections,
                Action = () =>
                {
                    dialogOverlay?.Push(new MassDeleteConfirmationDialog(deleteAllCollections));
                }
            });
        }

        private void deleteAllCollections()
        {
            realm.Write(r => r.RemoveAll<BeatmapCollection>());
            notificationOverlay?.Post(new ProgressCompletionNotification { Text = MaintenanceSettingsStrings.DeletedAllCollections });
        }
    }
}
