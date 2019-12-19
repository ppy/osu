// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Overlays.Dialog;
using osu.Game.Scoring;
using System;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Screens.Select
{
    public class BeatmapClearScoresDialog : PopupDialog
    {
        private ScoreManager scoreManager;

        public BeatmapClearScoresDialog(BeatmapInfo beatmap, Action onCompletion)
        {
            BodyText = $@"{beatmap.Metadata?.Artist} - {beatmap.Metadata?.Title}";
            Icon = FontAwesome.Solid.Eraser;
            HeaderText = @"Clearing all local scores. Are you sure?";
            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = @"Yes. Please.",
                    Action = () =>
                    {
                        Task.Run(() => scoreManager.Delete(scoreManager.QueryScores(s => !s.DeletePending && s.Beatmap.ID == beatmap.ID).ToList()))
                            .ContinueWith(_ => onCompletion);
                    }
                },
                new PopupDialogCancelButton
                {
                    Text = @"No, I'm still attached.",
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(ScoreManager scoreManager)
        {
            this.scoreManager = scoreManager;
        }
    }
}
