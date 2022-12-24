// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Overlays.Dialog;
using osu.Game.Scoring;
using System.Diagnostics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;

namespace osu.Game.Screens.Select
{
    public partial class LocalScoreDeleteDialog : DeleteConfirmationDialog
    {
        private readonly ScoreInfo score;

        public LocalScoreDeleteDialog(ScoreInfo score)
        {
            this.score = score;
        }

        [BackgroundDependencyLoader]
        private void load(BeatmapManager beatmapManager, ScoreManager scoreManager)
        {
            BeatmapInfo? beatmapInfo = beatmapManager.QueryBeatmap(b => b.ID == score.BeatmapInfoID);
            Debug.Assert(beatmapInfo != null);

            BodyText = $"{score.User} ({score.DisplayAccuracy}, {score.Rank})";

            Icon = FontAwesome.Regular.TrashAlt;
            DeleteAction = () => scoreManager.Delete(score);
        }
    }
}
