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
            get { return strains; }
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

        private IEnumerable<HitObject> objects;

        public IEnumerable<HitObject> Objects
        {
            get { return objects; }
            set
            {
                objects = value;

                if (!objects.Any())
                {
                    for (int x = 0; x < Strains.Count; x++)
                    {
                        Strains[x] = 0;
                    }
                }

                var values = new List<int>();

                foreach (double strain in strains)
                {
                    values.Add(0);
                }

                var startOfLists = (objects.First().StartTime - (objects.First().StartTime % strainStep))/strainStep;
                if (strainStep==1)
                {
                    startOfLists = objects.First().StartTime;
                }

                var interval = strainStep;
                if (interval==1)
                {
                    interval = 400;
                }

                foreach (var h in objects)
                {
                    var endTime = (h as IHasEndTime)?.EndTime ?? h.StartTime;

                    Debug.Assert(endTime >= h.StartTime);

                    int startRange = (int)((h.StartTime - startOfLists) / interval);
                    int endRange = (int)((endTime - startOfLists) / interval);
                    for (int i = startRange; i <= endRange && i < values.Count; i++)
                        values[i]++;
                }
                // zrób manipulację z values oraz Strains (zwróć uwagę na wielkość liter)
                Debug.Assert(values.Count == Strains.Count);

                for (int x = 0; x < Strains.Count; x++)
                {
                    if (values[x]==0)
                        Strains[x] = 0;
                    //dodaj if-a dla spinnerów
                }
            }
        }

        private double strainStep = new double();

        public double StrainStep
        {
            get { return strainStep; }
            set
            {
                strainStep = value;
            }
        }
    }
}
