// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Overlays.Dialog;
using osu.Game.Scoring;
using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Screens.Select
{
    public class BeatmapClearScoresDialog : PopupDialog
    {
        private ScoreManager manager;

        [BackgroundDependencyLoader]
        private void load(ScoreManager scoreManager)
        {
            manager = scoreManager;
        }

        public BeatmapClearScoresDialog(BeatmapInfo beatmap, Action refresh)
        {
            BodyText = $@"{beatmap.Metadata?.Artist} - {beatmap.Metadata?.Title}";
            Icon = FontAwesome.fa_eraser;
            HeaderText = $@"Clearing all local scores. Are you sure?";
            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = @"Yes. Please.",
                    Action = () =>
                    {
                        manager.Delete(manager.QueryScores(s => !s.DeletePending && s.Beatmap.ID == beatmap.ID).ToList());
                        refresh();
                    }
                },
                new PopupDialogCancelButton
                {
                    Text = @"No, I'm still attached.",
                },
            };
        }
    }
}
