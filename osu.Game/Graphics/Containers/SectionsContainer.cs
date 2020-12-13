// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Layout;
using osu.Framework.Utils;

namespace osu.Game.Graphics.Containers
{
    /// <summary>
    /// A container that can scroll to each section inside it.
    /// </summary>
    [Cached]
    public class SectionsContainer<T> : Container<T>
        where T : Drawable
    {
        public Bindable<T> SelectedSection { get; } = new Bindable<T>();
        private Drawable lastClickedSection;

        public Drawable ExpandableHeader
        {
            get => expandableHeader;
            set
            {
                if (value == expandableHeader) return;

                if (expandableHeader != null)
                    RemoveInternal(expandableHeader);

                expandableHeader = value;

                if (value == null) return;

                AddInternal(expandableHeader);
                lastKnownScroll = float.NaN;
            }
        }

        public Drawable FixedHeader
        {
            get => fixedHeader;
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
            get => footer;
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
            get => headerBackground;
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

        protected override Container<T> Content => scrollContentContainer;

        private readonly UserTrackingScrollContainer scrollContainer;
        private readonly Container headerBackgroundContainer;
        private readonly MarginPadding originalSectionsMargin;
        private Drawable expandableHeader, fixedHeader, footer, headerBackground;
        private FlowContainer<T> scrollContentContainer;

        private float headerHeight, footerHeight;

        private float lastKnownScroll;

        public SectionsContainer()
        {
            AddRangeInternal(new Drawable[]
            {
                scrollContainer = CreateScrollContainer().With(s =>
                {
                    s.RelativeSizeAxes = Axes.Both;
                    s.Masking = true;
                    s.ScrollbarVisible = false;
                    s.Child = scrollContentContainer = CreateScrollContentContainer();
                }),
                headerBackgroundContainer = new Container
                {
                    RelativeSizeAxes = Axes.X
                }
            });

            originalSectionsMargin = scrollContentContainer.Margin;
        }

        public override void Add(T drawable)
        {
            base.Add(drawable);
            lastKnownScroll = float.NaN;
            headerHeight = float.NaN;
            footerHeight = float.NaN;
        }

        public void ScrollTo(Drawable section)
        {
            lastClickedSection = section;
            scrollContainer.ScrollTo(scrollContainer.GetChildPosInContent(section) - (FixedHeader?.BoundingBox.Height ?? 0));
        }

        public void ScrollToTop() => scrollContainer.ScrollTo(0);

        [NotNull]
        protected virtual UserTrackingScrollContainer CreateScrollContainer() => new UserTrackingScrollContainer();

        [NotNull]
        protected virtual FlowContainer<T> CreateScrollContentContainer() =>
            new FillFlowContainer<T>
            {
                Direction = FillDirection.Vertical,
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
            };

        protected override bool OnInvalidate(Invalidation invalidation, InvalidationSource source)
        {
            var result = base.OnInvalidate(invalidation, source);

            if (source == InvalidationSource.Child && (invalidation & Invalidation.DrawSize) != 0)
            {
                lastKnownScroll = -1;
                result = true;
            }

            return result;
        }

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

            if (scrollContainer.Current == lastKnownScroll)
                return;

            lastKnownScroll = scrollContainer.Current;

            if (ExpandableHeader != null && FixedHeader != null)
            {
                float offset = Math.Min(ExpandableHeader.LayoutSize.Y, scrollContainer.Current);

                ExpandableHeader.Y = -offset;
                FixedHeader.Y = -offset + ExpandableHeader.LayoutSize.Y;
            }

            headerBackgroundContainer.Height = (ExpandableHeader?.LayoutSize.Y ?? 0) + (FixedHeader?.LayoutSize.Y ?? 0);
            headerBackgroundContainer.Y = ExpandableHeader?.Y ?? 0;

            selectMostVisible();
        }

        /// <summary>
        /// Changes <see cref="SelectedSection"/> to currently most visible section
        /// </summary>
        private void selectMostVisible()
        {
            // reset last clicked section because user started scrolling themselves
            if (scrollContainer.UserScrolling)
                lastClickedSection = null;

            // we are scrolled all the way to the bottom
            // select the section user clicked for or the very last section
            if (Precision.AlmostBigger(scrollContainer.Current, scrollContainer.ScrollableExtent))
            {
                SelectedSection.Value = lastClickedSection as T ?? Children.LastOrDefault();
                return;
            }

            var sortedByVisibility = new List<T>(Children);
            sortedByVisibility.Sort(compareVisibility);
            SelectedSection.Value = sortedByVisibility.FirstOrDefault();
        }

        private int compareVisibility(T a, T b)
        {
            // note: the decision to prioritize higher sections and differentiate scroll direction
            // comes from the fact that our scroll target is the top of the screen
            //
            // if this is ever not the case, this comparison should be rewritten
            float aIndex = IndexOf(a);
            float bIndex = IndexOf(b);
            float aVisibleHeight = visibleHeight(a);
            float bVisibleHeight = visibleHeight(b);
            // visibility requirement is lower (only 80%) for the section user clicked for
            // this makes the section get selected sooner when scrolling upwards, to match it already getting selected sooner when scrolling downwards
            bool aAllVisible = Precision.AlmostBigger(aVisibleHeight / a.Height, lastClickedSection as T == a ? 0.8f : 1.0f);
            bool bAllVisible = Precision.AlmostBigger(bVisibleHeight / b.Height, lastClickedSection as T == b ? 0.8f : 1.0f);

            // if one is all visible while the other isn't
            if (aAllVisible != bAllVisible)
            {
                // don't make the 100% visible section sort earlier in the list if it's not already a higher section
                // this is so we don't select a small section as soon as it gets on screen (only unwanted while scrolling downwards)
                if (aAllVisible && aIndex < bIndex)
                    return -1;
                if (bAllVisible && bIndex < aIndex)
                    return 1;
            }

            // if both are all visible - prioritize higher sections
            if (aAllVisible && bAllVisible)
                return aIndex.CompareTo(bIndex);

            // if neither are all visible - just compare which is more visible
            return bVisibleHeight.CompareTo(aVisibleHeight);
        }

        private float visibleHeight(T section)
        {
            float currentScroll = scrollContainer.Current;
            float scrollOffset = FixedHeader?.LayoutSize.Y ?? 0;
            float sectionPosition = scrollContainer.GetChildPosInContent(section) - scrollOffset;

            float top = Math.Max(sectionPosition, currentScroll);
            float bottom = Math.Min(sectionPosition + section.Height, currentScroll + scrollContainer.DisplayableContent);
            return bottom - top;
        }

        private void updateSectionsMargin()
        {
            if (!Children.Any()) return;

            var newMargin = originalSectionsMargin;
            newMargin.Top += headerHeight;
            newMargin.Bottom += footerHeight;

            scrollContentContainer.Margin = newMargin;
        }
    }
}
