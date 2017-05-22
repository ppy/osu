// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.Select
{
    public class BeatmapDeleteDialog : PopupDialog
    {
        private BeatmapDatabase database;

        [BackgroundDependencyLoader]
        private void load(BeatmapDatabase beatmapDatabase)
        {
            database = beatmapDatabase;
        }

        public BeatmapDeleteDialog(WorkingBeatmap beatmap)
        {
            if (beatmap == null) throw new ArgumentNullException(nameof(beatmap));

            Icon = FontAwesome.fa_trash_o;
            HeaderText = @"Confirm deletion of";
            BodyText = $@"{beatmap.Metadata?.Artist} - {beatmap.Metadata?.Title}";
            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = @"Yes. Totally. Delete it.",
                    Action = () =>
                    {
                        beatmap.Dispose();
                        database.Delete(beatmap.BeatmapSetInfo);
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
