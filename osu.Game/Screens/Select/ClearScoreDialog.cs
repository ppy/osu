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
    public class ClearScoresDialog : PopupDialog
    {
        private ScoreManager manager;

        [BackgroundDependencyLoader]
        private void load(ScoreManager beatmapManager)
        {
            manager = beatmapManager;
        }

        public ClearScoresDialog(BeatmapSetInfo beatmap, IEnumerable<ScoreInfo> scores, Action refresh)
        {
            BodyText = $@"{beatmap.Metadata?.Artist} - {beatmap.Metadata?.Title}";

            Icon = FontAwesome.fa_eraser;
            HeaderText = $@"Clearing {scores.Count()} local score(s). Are you sure?";
            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = @"Yes. Please.",
                    Action = () =>
                    {
                        manager.Delete(scores.ToList());
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
