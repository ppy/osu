﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Play
{
    public class SongProgressGraph : SquareGraph
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

                var firstHit = objects.First().StartTime;
                var lastHit = objects.Max(o => (o as IHasEndTime)?.EndTime ?? o.StartTime);

                if (lastHit == 0)
                    lastHit = objects.Last().StartTime;

                var interval = (lastHit - firstHit + 1) / granularity;

                foreach (var h in objects)
                {
                    var endTime = (h as IHasEndTime)?.EndTime ?? h.StartTime;

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
