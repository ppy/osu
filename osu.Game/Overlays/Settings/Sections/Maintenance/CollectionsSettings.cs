// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Localisation;
using osu.Game.Collections;
using osu.Game.Database;
using osu.Game.Localisation;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Overlays.Settings.Sections.Maintenance
{
    public partial class CollectionsSettings : SettingsSubsection
    {
        protected override LocalisableString Header => CommonStrings.Collections;

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        [Resolved]
        private INotificationOverlay? notificationOverlay { get; set; }

        [BackgroundDependencyLoader]
        private void load(IDialogOverlay? dialogOverlay)
        {
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
            bool anyDeleted = realm.Write(r =>
            {
                if (r.All<BeatmapCollection>().Any())
                {
                    r.RemoveAll<BeatmapCollection>();
                    return true;
                }
                else
                {
                    return false;
                }
            });

            notificationOverlay?.Post(new ProgressCompletionNotification { Text = anyDeleted ? MaintenanceSettingsStrings.DeletedAllCollections : MaintenanceSettingsStrings.NoCollectionsFoundToDelete });
        }
    }
}
