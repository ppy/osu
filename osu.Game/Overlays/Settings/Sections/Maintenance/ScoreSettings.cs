// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Localisation;
using osu.Game.Localisation;
using osu.Game.Scoring;

namespace osu.Game.Overlays.Settings.Sections.Maintenance
{
    public partial class ScoreSettings : SettingsSubsection
    {
        protected override LocalisableString Header => CommonStrings.Scores;

        private SettingsButton deleteScoresButton = null!;

        [BackgroundDependencyLoader]
        private void load(ScoreManager scores, IDialogOverlay? dialogOverlay)
        {
            Add(deleteScoresButton = new DangerousSettingsButton
            {
                Text = MaintenanceSettingsStrings.DeleteAllScores,
                Action = () =>
                {
                    dialogOverlay?.Push(new MassDeleteConfirmationDialog(() =>
                    {
                        deleteScoresButton.Enabled.Value = false;
                        Task.Run(() => scores.Delete()).ContinueWith(_ => Schedule(() => deleteScoresButton.Enabled.Value = true));
                    }, DeleteConfirmationContentStrings.Scores));
                }
            });
        }
    }
}
