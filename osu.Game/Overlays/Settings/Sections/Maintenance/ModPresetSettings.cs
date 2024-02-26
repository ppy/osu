// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Game.Database;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets.Mods;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections.Maintenance
{
    public partial class ModPresetSettings : SettingsSubsection
    {
        protected override LocalisableString Header => CommonStrings.ModPresets;

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        [Resolved]
        private INotificationOverlay? notificationOverlay { get; set; }

        private SettingsButton undeleteButton = null!;
        private SettingsButton deleteAllButton = null!;

        [BackgroundDependencyLoader]
        private void load(IDialogOverlay? dialogOverlay)
        {
            AddRange(new Drawable[]
            {
                deleteAllButton = new DangerousSettingsButton
                {
                    Text = MaintenanceSettingsStrings.DeleteAllModPresets,
                    Action = () =>
                    {
                        dialogOverlay?.Push(new MassDeleteConfirmationDialog(() =>
                        {
                            deleteAllButton.Enabled.Value = false;
                            Task.Run(deleteAllModPresets).ContinueWith(t => Schedule(onAllModPresetsDeleted, t));
                        }));
                    }
                },
                undeleteButton = new SettingsButton
                {
                    Text = MaintenanceSettingsStrings.RestoreAllRecentlyDeletedModPresets,
                    Action = () => Task.Run(undeleteModPresets).ContinueWith(t => Schedule(onModPresetsUndeleted, t))
                }
            });
        }

        private bool deleteAllModPresets() =>
            realm.Write(r =>
            {
                bool anyDeleted = false;

                foreach (var preset in r.All<ModPreset>())
                {
                    anyDeleted |= !preset.DeletePending;
                    preset.DeletePending = true;
                }

                return anyDeleted;
            });

        private void onAllModPresetsDeleted(Task<bool> deletionTask)
        {
            deleteAllButton.Enabled.Value = true;

            if (deletionTask.IsCompletedSuccessfully)
                notificationOverlay?.Post(new ProgressCompletionNotification { Text = deletionTask.GetResultSafely() ? MaintenanceSettingsStrings.DeletedAllModPresets : MaintenanceSettingsStrings.NoModPresetsFoundToDelete });
            else if (deletionTask.IsFaulted)
                Logger.Error(deletionTask.Exception, "Failed to delete all mod presets");
        }

        private bool undeleteModPresets() =>
            realm.Write(r =>
            {
                bool anyRestored = false;

                foreach (var preset in r.All<ModPreset>().Where(preset => preset.DeletePending))
                {
                    anyRestored |= preset.DeletePending;
                    preset.DeletePending = false;
                }

                return anyRestored;
            });

        private void onModPresetsUndeleted(Task<bool> undeletionTask)
        {
            undeleteButton.Enabled.Value = true;

            if (undeletionTask.IsCompletedSuccessfully)
                notificationOverlay?.Post(new ProgressCompletionNotification { Text = undeletionTask.GetResultSafely() ? MaintenanceSettingsStrings.RestoredAllDeletedModPresets : MaintenanceSettingsStrings.NoModPresetsFoundToRestore });
            else if (undeletionTask.IsFaulted)
                Logger.Error(undeletionTask.Exception, "Failed to restore mod presets");
        }
    }
}
