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

        public void ScrollTo(Drawable section)
        {
            lastClickedSection = section;
            scrollContainer.ScrollTo(scrollContainer.GetChildPosInContent(section) - scrollContainer.DisplayableContent * scroll_y_centre - (FixedHeader?.BoundingBox.Height ?? 0));
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
                lastKnownScroll = null;
                result = true;
            }

            return result;
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

                var smallestSectionHeight = Children.Count > 0 ? Children.Min(d => d.Height) : 0;

                // scroll offset is our fixed header height if we have it plus 10% of content height
                // plus 5% to fix floating point errors and to not have a section instantly unselect when scrolling upwards
                // but the 5% can't be bigger than our smallest section height, otherwise it won't get selected correctly
                float selectionLenienceAboveSection = Math.Min(smallestSectionHeight / 2.0f, scrollContainer.DisplayableContent * 0.05f);

                float scrollCentre = fixedHeaderSize + scrollContainer.DisplayableContent * scroll_y_centre + selectionLenienceAboveSection;

                if (Precision.AlmostBigger(0, scrollContainer.Current))
                    SelectedSection.Value = lastClickedSection as T ?? Children.FirstOrDefault();
                else if (Precision.AlmostBigger(scrollContainer.Current, scrollContainer.ScrollableExtent))
                    SelectedSection.Value = lastClickedSection as T ?? Children.LastOrDefault();
                else
                {
                    SelectedSection.Value = Children
                                            .TakeWhile(section => scrollContainer.GetChildPosInContent(section) - currentScroll - scrollCentre <= 0)
                                            .LastOrDefault() ?? Children.FirstOrDefault();
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
