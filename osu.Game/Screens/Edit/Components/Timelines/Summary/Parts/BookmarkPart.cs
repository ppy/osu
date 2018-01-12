// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Screens.Edit.Components.Timelines.Summary.Visualisations;

namespace osu.Game.Screens.Edit.Components.Timelines.Summary.Parts
{
    /// <summary>
    /// The part of the timeline that displays bookmarks.
    /// </summary>
    public class BookmarkPart : TimelinePart
    {
        protected override void LoadBeatmap(WorkingBeatmap beatmap)
        {
            base.LoadBeatmap(beatmap);
            foreach (int bookmark in beatmap.BeatmapInfo.Bookmarks)
                Add(new BookmarkVisualisation(bookmark));
        }

        private class BookmarkVisualisation : PointVisualisation
        {
            public BookmarkVisualisation(double startTime)
                : base(startTime)
            {
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours) => Colour = colours.Blue;
        }
    }
}
