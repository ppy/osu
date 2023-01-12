// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Screens.Edit.Components.Timelines.Summary.Visualisations;

namespace osu.Game.Screens.Edit.Components.Timelines.Summary.Parts
{
    /// <summary>
    /// The part of the timeline that displays bookmarks.
    /// </summary>
    public partial class BookmarkPart : TimelinePart
    {
        protected override void LoadBeatmap(EditorBeatmap beatmap)
        {
            base.LoadBeatmap(beatmap);
            foreach (int bookmark in beatmap.BeatmapInfo.Bookmarks)
                Add(new BookmarkVisualisation(bookmark));
        }

        private partial class BookmarkVisualisation : PointVisualisation
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
