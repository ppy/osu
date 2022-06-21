// Copyright (c) ppy Pty Ltd <contact@ppy.sh>.Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Localisation;
using osu.Game.Collections;
using osu.Game.Database;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections.Maintenance
{
    public class CollectionsSettings : SettingsSubsection
    {
        protected override LocalisableString Header => "Collections";
        private SettingsButton importCollectionsButton;

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load([CanBeNull] CollectionManager collectionManager, [CanBeNull] LegacyImportManager legacyImportManager, IDialogOverlay dialogOverlay)
        {
            if (collectionManager == null) return;

            if (legacyImportManager?.SupportsImportFromStable == true)

            {
                Add(importCollectionsButton = new SettingsButton
                {
                    Text = MaintenanceSettingsStrings.ImportCollectionsFromStable,
                    Action = () =>
                    {
                        importCollectionsButton.Enabled.Value = false;
                        legacyImportManager.ImportFromStableAsync(StableContent.Collections).ContinueWith(t => Schedule(() => importCollectionsButton.Enabled.Value = true));
                    }
                });
            }

            Add(new DangerousSettingsButton
            {
                Text = MaintenanceSettingsStrings.DeleteAllCollections,
                Action = () =>
                {
                    dialogOverlay?.Push(new MassDeleteConfirmationDialog(collectionManager.DeleteAll));
                }
            });
        }
    }
}
