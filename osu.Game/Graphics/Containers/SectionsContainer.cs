﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Graphics.Containers
{
    /// <summary>
    /// A container that can scroll to each section inside it.
    /// </summary>
    public class SectionsContainer<T> : Container<T>
        where T : Drawable
    {
        private Drawable expandableHeader, fixedHeader, footer, headerBackground;
        private readonly ScrollContainer scrollContainer;
        private readonly Container headerBackgroundContainer;
        private readonly FlowContainer<T> scrollContentContainer;

        protected override Container<T> Content => scrollContentContainer;

        public Drawable ExpandableHeader
        {
            get { return expandableHeader; }
            set
            {
                if (value == expandableHeader) return;

                expandableHeader?.Expire();
                expandableHeader = value;
                if (value == null) return;

                AddInternal(expandableHeader);
                lastKnownScroll = float.NaN;
            }
        }

        public Drawable FixedHeader
        {
            get { return fixedHeader; }
            set
            {
                if (value == fixedHeader) return;

                fixedHeader?.Expire();
                fixedHeader = value;
                if (value == null) return;

                AddInternal(fixedHeader);
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
                    scrollContainer.Remove(footer);
                footer = value;
                if (value == null) return;

                footer.Anchor |= Anchor.y2;
                footer.Origin |= Anchor.y2;
                scrollContainer.Add(footer);
                lastKnownScroll = float.NaN;
            }
        }

        public Drawable HeaderBackground
        {
            get { return headerBackground; }
            set
            {
                if (value == headerBackground) return;

                headerBackgroundContainer.Clear();
                headerBackground = value;
                if (value == null) return;

                headerBackgroundContainer.Add(headerBackground);

                lastKnownScroll = float.NaN;
            }
        }

        public Bindable<T> SelectedSection { get; } = new Bindable<T>();

        protected virtual FlowContainer<T> CreateScrollContentContainer()
            => new FillFlowContainer<T>
            {
                Direction = FillDirection.Vertical,
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
            };

        public override void Add(T drawable)
        {
            base.Add(drawable);
            lastKnownScroll = float.NaN;
            headerHeight = float.NaN;
            footerHeight = float.NaN;
        }

        private float headerHeight, footerHeight;
        private readonly MarginPadding originalSectionsMargin;
        private void updateSectionsMargin()
        {
            if (!Children.Any()) return;

            var newMargin = originalSectionsMargin;
            newMargin.Top += headerHeight;
            newMargin.Bottom += footerHeight;

            scrollContentContainer.Margin = newMargin;
        }

        public SectionsContainer()
        {
            AddInternal(scrollContainer = new ScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                ScrollbarVisible = false,
                Children = new Drawable[] { scrollContentContainer = CreateScrollContentContainer() }
            });
            AddInternal(headerBackgroundContainer = new Container
            {
                RelativeSizeAxes = Axes.X
            });
            originalSectionsMargin = scrollContentContainer.Margin;
        }

        public void ScrollTo(Drawable section) => scrollContainer.ScrollTo(scrollContainer.GetChildPosInContent(section) - (FixedHeader?.BoundingBox.Height ?? 0));

        public void ScrollToTop() => scrollContainer.ScrollTo(0);

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

            float currentScroll = scrollContainer.Current;

            if (currentScroll != lastKnownScroll)
            {
                lastKnownScroll = currentScroll;

                if (ExpandableHeader != null && FixedHeader != null)
                {
                    float offset = Math.Min(ExpandableHeader.LayoutSize.Y, currentScroll);

                    ExpandableHeader.Y = -offset;
                    FixedHeader.Y = -offset + ExpandableHeader.LayoutSize.Y;
                }

                headerBackgroundContainer.Height = (ExpandableHeader?.LayoutSize.Y ?? 0) + (FixedHeader?.LayoutSize.Y ?? 0);
                headerBackgroundContainer.Y = ExpandableHeader?.Y ?? 0;

                T bestMatch = null;
                float minDiff = float.MaxValue;
                float scrollOffset = FixedHeader?.LayoutSize.Y ?? 0;

                foreach (var section in Children)
                {
                    float diff = Math.Abs(scrollContainer.GetChildPosInContent(section) - currentScroll - scrollOffset);
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
