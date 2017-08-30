// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.Select
{
    public class BeatmapDeleteDialog : PopupDialog
    {
        private BeatmapManager manager;

        private readonly Action deleteAction;

        [BackgroundDependencyLoader]
        private void load(BeatmapManager beatmapManager)
        {
            manager = beatmapManager;
        }

        public BeatmapDeleteDialog(BeatmapSetInfo beatmap) : this()
        {
            BodyText = $@"{beatmap.Metadata?.Artist} - {beatmap.Metadata?.Title} (ALL DIFFICULTIES)";
            deleteAction = () => manager.Delete(beatmap);
        }

        public BeatmapDeleteDialog(BeatmapInfo beatmap) : this()
        {
            BodyText = $@"{beatmap.Metadata?.Artist} - {beatmap.Metadata?.Title} [{beatmap.Version}]";
            deleteAction = () => manager.Delete(beatmap);
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
                    Action = () => deleteAction(),
                },
                new PopupDialogCancelButton
                {
                    Text = @"Firetruck, I didn't mean to!",
                },
            };
        }
    }
}
