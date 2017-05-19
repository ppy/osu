// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Graphics.Containers
{
    /// <summary>
    /// A container that can scroll to each section inside it.
    /// </summary>
    public class SectionsContainer : Container
    {
        private Drawable expandableHeader, fixedHeader;
        private ScrollContainer scrollContainer;

        public Drawable ExpandableHeader
        {
            get { return expandableHeader; }
            set
            {
                if (value == expandableHeader) return;

                scrollContainer.Remove(expandableHeader);
                expandableHeader = value;
                expandableHeader.Depth = float.MinValue;
                scrollContainer.Add(expandableHeader);
            }
        }

        public Drawable FixedHeader
        {
            get { return fixedHeader; }
            set
            {
                if (value == fixedHeader) return;

                scrollContainer.Remove(fixedHeader);
                fixedHeader = value;
                fixedHeader.Depth = float.MinValue / 2;
                scrollContainer.Add(fixedHeader);
            }
        }

        private List<Drawable> sections = new List<Drawable>();
        public IEnumerable<Drawable> Sections
        {
            get { return sections; }
            set
            {
                if (value == sections) return;

                foreach (var section in sections)
                    scrollContainer.Remove(section);

                sections = value.ToList();
                scrollContainer.Add(value);
            }
        }

        public SectionsContainer()
        {
            Add(scrollContainer = new ScrollContainer
            {
                RelativeSizeAxes = Axes.Both
            });
        }
    }
}
