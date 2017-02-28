// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Overlays.Dialog;

namespace osu.Game
{
    public class BeatmapDeleteDialog : PopupDialog
    {
        public Action<WorkingBeatmap> OnDelete;

        public BeatmapDeleteDialog(WorkingBeatmap beatmap)
        {
            Icon = FontAwesome.fa_trash_o;
            HeaderText = @"Confirm deletion of";
            BodyText = $@"{beatmap?.Beatmap?.Metadata?.Artist} - {beatmap?.Beatmap?.Metadata?.Title}";
            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = @"Yes. Totally. Delete it.",
                    Action = () =>
                    {
                        OnDelete?.Invoke(beatmap);
                    },
                },
                new PopupDialogCancelButton
                {
                    Text = @"Firetruck, I didn't mean to!",
                },
            };
        }
    }
}
