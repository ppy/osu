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

        private WorkingBeatmap beatmap;
        public WorkingBeatmap Beatmap
        {
            get
            {
                return beatmap;
            }
            set
            {
                beatmap = value;
                BodyText = $@"{beatmap?.Beatmap?.Metadata?.Artist} - {beatmap?.Beatmap?.Metadata?.Title}";
            }
        }

        public BeatmapDeleteDialog()
        {
            Icon = FontAwesome.fa_trash_o;
            HeaderText = @"Confirm deletion of";
            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = @"Yes. Totally. Delete it.",
                    Action = () =>
                    {
                        if (Beatmap != null)
                        {
                            OnDelete?.Invoke(Beatmap);
                        }
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
