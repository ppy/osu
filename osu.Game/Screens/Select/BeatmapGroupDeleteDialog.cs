// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.Select
{
    public partial class BeatmapGroupDeleteDialog : DeletionDialog
    {
        private readonly GroupDefinition group;
        private readonly IReadOnlyList<BeatmapSetInfo> beatmapSets;

        public BeatmapGroupDeleteDialog(GroupDefinition group, IReadOnlyList<BeatmapSetInfo> beatmapSets)
        {
            this.group = group;
            this.beatmapSets = beatmapSets;

            BodyText = group.Title;
        }

        [BackgroundDependencyLoader]
        private void load(BeatmapManager beatmapManager)
        {
            DangerousAction = () =>
            {
                foreach (var set in beatmapSets)
                    beatmapManager.Delete(set);
            };
        }
    }
}

