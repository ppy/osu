// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using System.Collections.Generic;
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

                var lastHit = ((objects.Last() as IHasEndTime)?.EndTime ?? objects.Last().StartTime) + 1;

                var interval = lastHit / granularity;

                var values = new int[granularity];

                foreach (var h in objects)
                {
                    IHasEndTime end = h as IHasEndTime;

                    int startRange = (int)(h.StartTime / interval);
                    int endRange = (int)((end?.EndTime ?? h.StartTime) / interval);
                    for (int i = startRange; i <= endRange; i++)
                        values[i]++;
                }

                Values = values;
            }
        }
    }
}
