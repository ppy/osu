// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Utils;

namespace osu.Game.Screens.Play.HUD
{
    public partial class DefaultSongProgressGraph : SquareGraph
    {
        private const int granularity = 200;

        public void SetFromObjects(IEnumerable<HitObject> objects)
        {
            Values = new float[granularity];

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

        public void SetFromStrains(double[] strains)
        {
            Values = FormatUtils.ResampleStrains(strains, granularity).Select(value => (float)value).ToArray();
        }
    }
}
