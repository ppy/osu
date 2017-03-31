// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Screens.Select.Details
{
    public class BeatmapDetailsGraph : FillFlowContainer<BeatmapDetailsBar>
    {

        public IEnumerable<float> Values
        {
            set
            {
                List<float> values = value.ToList();
                List<BeatmapDetailsBar> graphBars = Children.ToList();
                for (int i = 0; i < values.Count; i++)
                    if (graphBars.Count > i)
                    {
                        graphBars[i].Length = values[i] / values.Max();
                        graphBars[i].Width = 1.0f / values.Count;
                    }
                    else
                        Add(new BeatmapDetailsBar
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = 1.0f / values.Count,
                            Length = values[i] / values.Max(),
                            Direction = BarDirection.BottomToTop,
                            BackgroundColour = new Color4(0, 0, 0, 0),
                        });

            }
        }

    }
}
