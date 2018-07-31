// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
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
        private List<double> strains = new List<double>();

        public List<double> Strains
        {
            set
            {
                for(int x = 0; x < value.Count(); x++)
                {
                    if (x == 0)
                    {
                        strains.Add(value[x]);
                        strains.Add(value[x]);
                        Values.Add(value[x]);
                        Values.Add(value[x]);
                    }
                    else
                    {
                        strains.Add(value[x]);
                        Values.Add(value[x]);
                    }
                }
            }
        }
    }
}
