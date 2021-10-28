// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
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

        private T lastClickedSection;

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
                lastKnownScroll = null;
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
                lastKnownScroll = null;
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
                lastKnownScroll = null;
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

                lastKnownScroll = null;
            }
        }

        protected override Container<T> Content => scrollContentContainer;

        private readonly UserTrackingScrollContainer scrollContainer;
        private readonly Container headerBackgroundContainer;
        private readonly MarginPadding originalSectionsMargin;
        private Drawable expandableHeader, fixedHeader, footer, headerBackground;
        private FlowContainer<T> scrollContentContainer;

        private float? headerHeight, footerHeight;

        private float? lastKnownScroll;

        /// <summary>
        /// The percentage of the container to consider the centre-point for deciding the active section (and scrolling to a requested section).
        /// </summary>
        private const float scroll_y_centre = 0.1f;

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

            Debug.Assert(drawable != null);

            lastKnownScroll = null;
            headerHeight = null;
            footerHeight = null;
        }

        public void ScrollTo(Drawable target)
        {
            lastKnownScroll = null;

            float fixedHeaderSize = FixedHeader?.BoundingBox.Height ?? 0;

            // implementation similar to ScrollIntoView but a bit more nuanced.
            float top = scrollContainer.GetChildPosInContent(target);

            float bottomScrollExtent = scrollContainer.ScrollableExtent - fixedHeaderSize;
            float scrollTarget = top - fixedHeaderSize - scrollContainer.DisplayableContent * scroll_y_centre;

            if (scrollTarget > bottomScrollExtent)
                scrollContainer.ScrollToEnd();
            else
                scrollContainer.ScrollTo(scrollTarget);

            if (target is T section)
                lastClickedSection = section;
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
            bool result = base.OnInvalidate(invalidation, source);

            if (source == InvalidationSource.Child && (invalidation & Invalidation.DrawSize) != 0)
            {
                InvalidateScrollPosition();
                result = true;
            }

            return result;
        }

        protected void InvalidateScrollPosition()
        {
            Schedule(() =>
            {
                lastKnownScroll = null;
                lastClickedSection = null;
            });
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            float fixedHeaderSize = FixedHeader?.LayoutSize.Y ?? 0;
            float expandableHeaderSize = ExpandableHeader?.LayoutSize.Y ?? 0;

            float headerH = expandableHeaderSize + fixedHeaderSize;
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

                // reset last clicked section because user started scrolling themselves
                if (scrollContainer.UserScrolling)
                    lastClickedSection = null;

                if (ExpandableHeader != null && FixedHeader != null)
                {
                    float offset = Math.Min(expandableHeaderSize, currentScroll);

                    ExpandableHeader.Y = -offset;
                    FixedHeader.Y = -offset + expandableHeaderSize;
                }

                headerBackgroundContainer.Height = expandableHeaderSize + fixedHeaderSize;
                headerBackgroundContainer.Y = ExpandableHeader?.Y ?? 0;

                float smallestSectionHeight = Children.Count > 0 ? Children.Min(d => d.Height) : 0;

                // scroll offset is our fixed header height if we have it plus 10% of content height
                // plus 5% to fix floating point errors and to not have a section instantly unselect when scrolling upwards
                // but the 5% can't be bigger than our smallest section height, otherwise it won't get selected correctly
                float selectionLenienceAboveSection = Math.Min(smallestSectionHeight / 2.0f, scrollContainer.DisplayableContent * 0.05f);

                float scrollCentre = fixedHeaderSize + scrollContainer.DisplayableContent * scroll_y_centre + selectionLenienceAboveSection;

                var presentChildren = Children.Where(c => c.IsPresent);

                if (lastClickedSection != null)
                    SelectedSection.Value = lastClickedSection;
                else if (Precision.AlmostBigger(0, scrollContainer.Current))
                    SelectedSection.Value = presentChildren.FirstOrDefault();
                else if (Precision.AlmostBigger(scrollContainer.Current, scrollContainer.ScrollableExtent))
                    SelectedSection.Value = presentChildren.LastOrDefault();
                else
                {
                    SelectedSection.Value = presentChildren
                                            .TakeWhile(section => scrollContainer.GetChildPosInContent(section) - currentScroll - scrollCentre <= 0)
                                            .LastOrDefault() ?? presentChildren.FirstOrDefault();
                }
            }
        }

        private void updateSectionsMargin()
        {
            if (!Children.Any()) return;

            var newMargin = originalSectionsMargin;

            newMargin.Top += (headerHeight ?? 0);
            newMargin.Bottom += (footerHeight ?? 0);

            scrollContentContainer.Margin = newMargin;
        }
    }
}
