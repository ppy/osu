// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Play.HUD
{
    public partial class ArgonSongProgressGraph : SegmentedGraph<int>
    {
        private IEnumerable<HitObject>? objects;

        public IEnumerable<HitObject> Objects
        {
            set
            {
                objects = value;

                const int granularity = 200;
                int[] values = new int[granularity];

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
                        values[i]++;
                }

                Values = values;
            }
        }

        public ArgonSongProgressGraph()
            : base(5)
        {
            var colours = new List<Colour4>();

            for (int i = 0; i < 5; i++)
                colours.Add(Colour4.White.Darken(1 + 1 / 5f).Opacity(1 / 5f));

            TierColours = colours;
        }
    }
}
