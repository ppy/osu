// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects;
using osu.Game.Graphics.UserInterface;
using osu.Game.Utils;

namespace osu.Game.Screens.Play.HUD
{
    public partial class ArgonSongProgressGraph : SegmentedGraph<float>
    {
        private const int tier_count = 5;

        private const int display_granularity = 200;

        public void SetFromObjects(IEnumerable<HitObject> objects)
        {
            float[] values = new float[display_granularity];

            if (!objects.Any())
                return;

            (double firstHit, double lastHit) = BeatmapExtensions.CalculatePlayableBounds(objects);

            if (lastHit == 0)
                lastHit = objects.Last().StartTime;

            double interval = (lastHit - firstHit + 1) / display_granularity;

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

        public void SetFromStrains(double[] strains)
        {
            Values = FormatUtils.ResampleStrains(strains, display_granularity).Select(value => (float)value).ToArray();
        }

        public ArgonSongProgressGraph()
            : base(tier_count)
        {
            var colours = new List<Colour4>();

            for (int i = 0; i < tier_count; i++)
                colours.Add(OsuColour.Gray(0.2f).Opacity(0.1f));

            TierColours = colours;
        }
    }
}
