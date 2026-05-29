// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Play.HUD
{
    public partial class DefaultSongProgressGraph : SquareGraph
    {
        private const int granularity = 200;

        private double firstHit;
        private double lastHit;

        [Resolved(CanBeNull = true)]
        private ReplayBookmarkController bookmarkController { get; set; }

        private IEnumerable<HitObject> objects;

        public IEnumerable<HitObject> Objects
        {
            set
            {
                objects = value;

                Values = new int[granularity];

                firstHit = 0;
                lastHit = 0;

                if (!objects.Any())
                    return;

                (firstHit, lastHit) = BeatmapExtensions.CalculatePlayableBounds(objects);

                if (lastHit == 0)
                    lastHit = objects.Last().StartTime;

                double interval = (lastHit - firstHit + 1) / granularity;

                foreach (var h in objects)
                {
                    double endTime = h.GetEndTime();

                    Debug.Assert(endTime >= h.StartTime);

                    int startRange = (int)((h.StartTime - firstHit) / interval);
                    int endRange = (int)((endTime - firstHit) / interval);
                    for (int i = startRange; i <= endRange; i++)
                        Values[i]++;
                }
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            bookmarkController?.Bookmarks.BindCollectionChanged((_, _) => RedrawBookmarks(), false);
        }

        protected override void RedrawBookmarks()
        {
            if (bookmarkController == null || ColumnCount == 0) return;

            double length = lastHit - firstHit + 1;
            if (length <= 0) return;

            double interval = length / ColumnCount;

            for (int i = 0; i < ColumnCount; i++)
                Columns[i].IsBookmarked = false;

            foreach (int b in bookmarkController.Bookmarks)
            {
                int col = (int)((b - firstHit) / interval);
                if (col >= 0 && col < ColumnCount)
                    Columns[col].IsBookmarked = true;
            }

            Columns.ForceRedraw();
        }
    }
}
