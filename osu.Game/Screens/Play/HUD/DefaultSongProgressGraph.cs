// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Play.HUD
{
    public partial class DefaultSongProgressGraph : SquareGraph
    {
        private IEnumerable<HitObject> objects;

        public IEnumerable<HitObject> Objects
        {
            set
            {
                objects = value;

                const int granularity = 200;
                Values = new int[granularity];

                if (!objects.Any())
                    return;

                (double firstHit, double lastHit) = BeatmapExtensions.CalculatePlayableBounds(objects);

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
    }
}
