// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Graphics.UserInterface
{
    public class BarGraph : FillFlowContainer<Bar>
    {
        private BarDirection direction = BarDirection.BottomToTop;
        public new BarDirection Direction
        {
            get
            {
                return direction;
            }
            set
            {
                direction = value;
                base.Direction = (direction & BarDirection.Horizontal) > 0 ? FillDirection.Vertical : FillDirection.Horizontal;
                foreach (var bar in Children)
                {
                    bar.Size = (direction & BarDirection.Horizontal) > 0 ? new Vector2(1, 1.0f / Children.Count()) : new Vector2(1.0f / Children.Count(), 1);
                    bar.Direction = direction;
                }
            }
        }

        public IEnumerable<float> Values
        {
            set
            {
                List<float> values = value.ToList();
                List<Bar> graphBars = Children.ToList();
                for (int i = 0; i < values.Count; i++)
                    if (graphBars.Count > i)
                    {
                        graphBars[i].Length = values[i] / values.Max();
                        graphBars[i].Size = (direction & BarDirection.Horizontal) > 0 ? new Vector2(1, 1.0f / values.Count) : new Vector2(1.0f / values.Count, 1);
                    }
                    else
                        Add(new Bar
                        {
                            RelativeSizeAxes = Axes.Both,
                            Size = (direction & BarDirection.Horizontal) > 0 ? new Vector2(1, 1.0f / values.Count) : new Vector2(1.0f / values.Count, 1),
                            Length = values[i] / values.Max(),
                            Direction = Direction,
                        });
            }
        }
    }
}