// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Configuration;
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
        private readonly ScrollContainer scrollContainer;
        private readonly Container<Drawable> sectionsContainer;

        public Drawable ExpandableHeader
        {
            get { return expandableHeader; }
            set
            {
                if (value == expandableHeader) return;

                if (expandableHeader != null)
                    Remove(expandableHeader);
                expandableHeader = value;
                expandableHeader.Depth = float.MinValue;
                Add(expandableHeader);
                updateSectionMargin();
            }
        }

        public Drawable FixedHeader
        {
            get { return fixedHeader; }
            set
            {
                if (value == fixedHeader) return;

                if (fixedHeader != null)
                    Remove(fixedHeader);
                fixedHeader = value;
                fixedHeader.Depth = float.MinValue / 2;
                Add(fixedHeader);
                updateSectionMargin();
            }
        }

        public Bindable<Drawable> SelectedSection { get; } = new Bindable<Drawable>();
        public void ScrollToSection(Drawable section) => scrollContainer.ScrollIntoView(section);

        protected virtual Container<Drawable> CreateSectionsContainer()
            => new FillFlowContainer
            {
                Direction = FillDirection.Vertical,
                AutoSizeAxes = Axes.Both
            };

        private List<Drawable> sections = new List<Drawable>();
        public IEnumerable<Drawable> Sections
        {
            get { return sections; }
            set
            {
                if (value == sections) return;

                foreach (var section in sections)
                    sectionsContainer.Remove(section);

                sections = value.ToList();
                if (sections.Count == 0) return;

                originalSectionMargin = sections[0].Margin;
                updateSectionMargin();
                sectionsContainer.Add(sections);
                SelectedSection.Value = sections[0];
            }
        }

        private MarginPadding originalSectionMargin;
        private void updateSectionMargin()
        {
            if (sections.Count == 0) return;

            var newMargin = originalSectionMargin;
            newMargin.Top += ExpandableHeader?.Height ?? 0 + FixedHeader?.Height ?? 0;

            sections[0].Margin = newMargin;
        }

        public SectionsContainer()
        {
            Add(scrollContainer = new ScrollContainer()
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[] { sectionsContainer = CreateSectionsContainer() }
            });
        }

        float lastKnownScroll;
        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();
            if (expandableHeader == null) return;

            float position = scrollContainer.Current;
            float offset = Math.Max(expandableHeader.Height, position);

            expandableHeader.Y = -offset;
            fixedHeader.Y = -offset + expandableHeader.Height;

            float currentScroll = scrollContainer.Current;
            if (currentScroll != lastKnownScroll)
            {
                lastKnownScroll = currentScroll;

                Drawable bestMatch = null;
                float minDiff = float.MaxValue;

                foreach (var section in sections)
                {
                    float diff = Math.Abs(scrollContainer.GetChildPosInContent(section) - currentScroll);
                    if (diff < minDiff)
                    {
                        minDiff = diff;
                        bestMatch = section;
                    }
                }

                if (bestMatch != null)
                    SelectedSection.Value = bestMatch;
            }
        }
    }
}
