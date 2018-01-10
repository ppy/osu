// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Graphics.UserInterface
{
    public class BarGraph : FillFlowContainer<Bar>
    {
        /// <summary>
        /// Manually sets the max value, if null <see cref="Enumerable.Max(IEnumerable{float})"/> is instead used
        /// </summary>
        public float? MaxValue { get; set; }

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
                    bar.Size = (direction & BarDirection.Horizontal) > 0 ? new Vector2(1, 1.0f / Children.Count) : new Vector2(1.0f / Children.Count, 1);
                    bar.Direction = direction;
                }
            }
        }

        /// <summary>
        /// A list of floats that defines the length of each <see cref="Bar"/>
        /// </summary>
        public IEnumerable<float> Values
        {
            set
            {
                List<Bar> bars = Children.ToList();
                foreach (var bar in value.Select((length, index) => new { Value = length, Bar = bars.Count > index ? bars[index] : null }))
                {
                    float length = MaxValue ?? value.Max();
                    if (length != 0)
                        length = bar.Value / length;

                    float size = value.Count();
                    if (size != 0)
                        size = 1.0f / size;

                    if (bar.Bar != null)
                    {
                        bar.Bar.Length = length;
                        bar.Bar.Size = (direction & BarDirection.Horizontal) > 0 ? new Vector2(1, size) : new Vector2(size, 1);
                    }
                    else
                    {
                        Add(new Bar
                        {
                            RelativeSizeAxes = Axes.Both,
                            Size = (direction & BarDirection.Horizontal) > 0 ? new Vector2(1, size) : new Vector2(size, 1),
                            Length = length,
                            Direction = Direction,
                        });
                    }
                }
                //I'm using ToList() here because Where() returns an Enumerable which can change it's elements afterwards
                RemoveRange(Children.Where((bar, index) => index >= value.Count()).ToList());
            }
        }
    }
}
