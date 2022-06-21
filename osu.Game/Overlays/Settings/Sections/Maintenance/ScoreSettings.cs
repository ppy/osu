// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Threading.Tasks;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Localisation;
using osu.Game.Database;
using osu.Game.Localisation;
using osu.Game.Scoring;

namespace osu.Game.Overlays.Settings.Sections.Maintenance
{
    public class ScoreSettings : SettingsSubsection
    {
        protected override LocalisableString Header => "Scores";
        private SettingsButton importScoresButton;
        private SettingsButton deleteScoresButton;

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(ScoreManager scores, [CanBeNull] LegacyImportManager legacyImportManager, IDialogOverlay dialogOverlay)
        {
            if (legacyImportManager?.SupportsImportFromStable == true)
            {
                Add(importScoresButton = new SettingsButton
                {
                    Text = MaintenanceSettingsStrings.ImportScoresFromStable,
                    Action = () =>
                    {
                        importScoresButton.Enabled.Value = false;
                        legacyImportManager.ImportFromStableAsync(StableContent.Scores).ContinueWith(t => Schedule(() => importScoresButton.Enabled.Value = true));
                    }
                });
            }

            Add(deleteScoresButton = new DangerousSettingsButton
            {
                Text = MaintenanceSettingsStrings.DeleteAllScores,
                Action = () =>
                {
                    dialogOverlay?.Push(new MassDeleteConfirmationDialog(() =>
                    {
                        deleteScoresButton.Enabled.Value = false;
                        Task.Run(() => scores.Delete()).ContinueWith(t => Schedule(() => deleteScoresButton.Enabled.Value = true));
                    }));
                }
            });
        }
    }
}
