﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.Select
{
    public partial class BeatmapDeleteDialog : DeleteConfirmationDialog
    {
        private readonly BeatmapSetInfo beatmapSet;

        public BeatmapDeleteDialog(BeatmapSetInfo beatmapSet)
        {
            this.beatmapSet = beatmapSet;
            BodyText = $@"{beatmapSet.Metadata.Artist} - {beatmapSet.Metadata.Title}";
        }

        [BackgroundDependencyLoader]
        private void load(BeatmapManager beatmapManager)
        {
            DeleteAction = () => beatmapManager.Delete(beatmapSet);
        }
    }
}
