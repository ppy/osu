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
        private Drawable expandableHeader, fixedHeader, footer;
        public readonly ScrollContainer ScrollContainer;
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
                if (value == null) return;

                Add(expandableHeader);
                lastKnownScroll = float.NaN;
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
                if (value == null) return;

                Add(fixedHeader);
                lastKnownScroll = float.NaN;
            }
        }

        public Drawable Footer
        {
            get { return footer; }
            set
            {
                if (value == footer) return;

                if (footer != null)
                    ScrollContainer.Remove(footer);
                footer = value;
                if (value == null) return;

                footer.Anchor |= Anchor.y2;
                footer.Origin |= Anchor.y2;
                ScrollContainer.Add(footer);
                lastKnownScroll = float.NaN;
            }
        }

        public Bindable<Drawable> SelectedSection { get; } = new Bindable<Drawable>();

        protected virtual Container<Drawable> CreateScrollContentContainer()
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
                foreach (var section in sections)
                    sectionsContainer.Remove(section);

                sections = value.ToList();
                if (sections.Count == 0) return;

                sectionsContainer.Add(sections);
                SelectedSection.Value = sections[0];
                lastKnownScroll = float.NaN;
            }
        }

        private float headerHeight, footerHeight;
        private readonly MarginPadding originalSectionsMargin;
        private void updateSectionsMargin()
        {
            if (sections.Count == 0) return;

            var newMargin = originalSectionsMargin;
            newMargin.Top += headerHeight;
            newMargin.Bottom += footerHeight;

            sectionsContainer.Margin = newMargin;
        }

        public SectionsContainer()
        {
            Add(ScrollContainer = new ScrollContainer()
            {
                RelativeSizeAxes = Axes.Both,
                Masking = false,
                Children = new Drawable[] { sectionsContainer = CreateScrollContentContainer() }
            });
            originalSectionsMargin = sectionsContainer.Margin;
        }

        private float lastKnownScroll;
        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            float headerH = (ExpandableHeader?.LayoutSize.Y ?? 0) + (FixedHeader?.LayoutSize.Y ?? 0);
            float footerH = Footer?.LayoutSize.Y ?? 0;
            if (headerH != headerHeight || footerH != footerHeight)
            {
                headerHeight = headerH;
                footerHeight = footerH;
                updateSectionsMargin();
            }

            float currentScroll = Math.Max(0, ScrollContainer.Current);
            if (currentScroll != lastKnownScroll)
            {
                lastKnownScroll = currentScroll;

                if (expandableHeader != null && fixedHeader != null)
                {
                    float offset = Math.Min(expandableHeader.LayoutSize.Y, currentScroll);

                    expandableHeader.Y = -offset;
                    fixedHeader.Y = -offset + expandableHeader.LayoutSize.Y;
                }

                Drawable bestMatch = null;
                float minDiff = float.MaxValue;

                foreach (var section in sections)
                {
                    float diff = Math.Abs(ScrollContainer.GetChildPosInContent(section) - currentScroll);
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
