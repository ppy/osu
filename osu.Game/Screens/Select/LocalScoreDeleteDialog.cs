// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Overlays.Dialog;
using osu.Game.Scoring;

namespace osu.Game.Screens.Select
{
    public partial class LocalScoreDeleteDialog : DeletionDialog
    {
        private readonly ScoreInfo score;

        public LocalScoreDeleteDialog(ScoreInfo score)
        {
            this.score = score;
        }

        [BackgroundDependencyLoader]
        private void load(ScoreManager scoreManager)
        {
            BodyText = $"{score.User} ({score.DisplayAccuracy}, {score.Rank})";
            DangerousAction = () => scoreManager.Delete(score);
        }
    }
}
