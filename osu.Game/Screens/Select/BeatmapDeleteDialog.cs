// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.Select
{
    public class BeatmapDeleteDialog : PopupDialog
    {
        private BeatmapManager manager;

        [BackgroundDependencyLoader]
        private void load(BeatmapManager beatmapManager)
        {
            manager = beatmapManager;
        }

        public BeatmapDeleteDialog(BeatmapSetInfo beatmap)
        {
            BodyText = $@"{beatmap.Metadata.Artist} - {beatmap.Metadata.Title}";

            Icon = FontAwesome.Regular.TrashAlt;
            HeaderText = @"Confirm deletion of";
            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = @"Yes. Totally. Delete it.",
                    Action = () => manager?.Delete(beatmap),
                },
                new PopupDialogCancelButton
                {
                    Text = @"Firetruck, I didn't mean to!",
                },
            };
        }
    }
}
