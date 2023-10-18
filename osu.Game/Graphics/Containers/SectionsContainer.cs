// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Layout;
using osu.Framework.Logging;
using osu.Framework.Threading;
using osu.Framework.Utils;

namespace osu.Game.Graphics.Containers
{
    /// <summary>
    /// A container that can scroll to each section inside it.
    /// </summary>
    [Cached]
    public partial class SectionsContainer<T> : Container<T>
        where T : Drawable
    {
        public Bindable<T?> SelectedSection { get; } = new Bindable<T?>();

        private T? lastClickedSection;

        protected override Container<T> Content => scrollContentContainer;

        private readonly UserTrackingScrollContainer scrollContainer;
        private readonly Container headerBackgroundContainer;
        private readonly MarginPadding originalSectionsMargin;

        private Drawable? fixedHeader;

        private Drawable? footer;
        private Drawable? headerBackground;

        private FlowContainer<T> scrollContentContainer = null!;

        private float? headerHeight, footerHeight;

        private float? lastKnownScroll;

        /// <summary>
        /// The percentage of the container to consider the centre-point for deciding the active section (and scrolling to a requested section).
        /// </summary>
        private const float scroll_y_centre = 0.1f;

        private Drawable? expandableHeader;

        public Drawable? ExpandableHeader
        {
            get => expandableHeader;
            set
            {
                if (value == expandableHeader) return;

                if (expandableHeader != null)
                    RemoveInternal(expandableHeader, false);

                expandableHeader = value;

                if (value == null) return;

                AddInternal(expandableHeader);

                lastKnownScroll = null;
            }
        }

        public Drawable? FixedHeader
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

        public Drawable? Footer
        {
            get => footer;
            set
            {
                if (value == footer) return;

                if (footer != null)
                    scrollContainer.Remove(footer, false);

                footer = value;

                if (footer == null) return;

                footer.Anchor |= Anchor.y2;
                footer.Origin |= Anchor.y2;

                scrollContainer.Add(footer);
                lastKnownScroll = null;
            }
        }

        public Drawable? HeaderBackground
        {
            get => headerBackground;
            set
            {
                if (value == headerBackground) return;

                headerBackgroundContainer.Clear();
                headerBackground = value;

                if (headerBackground != null)
                {
                    headerBackgroundContainer.Add(headerBackground);
                    lastKnownScroll = null;
                }
            }
        }

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

        private ScheduledDelegate? scrollToTargetDelegate;

        public void ScrollTo(Drawable target)
        {
            Logger.Log($"Scrolling to {target}..");

            lastKnownScroll = null;

            float scrollTarget = getScrollTargetForDrawable(target);

            if (scrollTarget > scrollContainer.ScrollableExtent)
                scrollContainer.ScrollToEnd();
            else
                scrollContainer.ScrollTo(scrollTarget);

            if (target is T section)
                lastClickedSection = section;

            // Content may load in as a scroll occurs, changing the scroll target we need to aim for.
            // This scheduled operation ensures that we keep trying until actually arriving at the target.
            scrollToTargetDelegate?.Cancel();
            scrollToTargetDelegate = Scheduler.AddDelayed(() =>
            {
                if (scrollContainer.UserScrolling)
                {
                    Logger.Log("Scroll operation interrupted by user scroll");
                    scrollToTargetDelegate?.Cancel();
                    scrollToTargetDelegate = null;
                    return;
                }

                if (Precision.AlmostEquals(scrollContainer.Current, scrollTarget, 1))
                {
                    Logger.Log($"Finished scrolling to {target}!");
                    scrollToTargetDelegate?.Cancel();
                    scrollToTargetDelegate = null;
                    return;
                }

                if (!Precision.AlmostEquals(getScrollTargetForDrawable(target), scrollTarget, 1))
                {
                    Logger.Log($"Reattempting scroll to {target} due to change in position");
                    ScrollTo(target);
                }
            }, 50, true);
        }

        private float getScrollTargetForDrawable(Drawable target)
        {
            // implementation similar to ScrollIntoView but a bit more nuanced.
            return scrollContainer.GetChildPosInContent(target) - scrollContainer.DisplayableContent * scroll_y_centre;
        }

        public void ScrollToTop() => scrollContainer.ScrollTo(0);

        protected virtual UserTrackingScrollContainer CreateScrollContainer() => new UserTrackingScrollContainer();

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
            lastKnownScroll = null;
            lastClickedSection = null;
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

                var flowChildren = scrollContentContainer.FlowingChildren.OfType<T>();

                float smallestSectionHeight = flowChildren.Any() ? flowChildren.Min(d => d.Height) : 0;

                // scroll offset is our fixed header height if we have it plus 10% of content height
                // plus 5% to fix floating point errors and to not have a section instantly unselect when scrolling upwards
                // but the 5% can't be bigger than our smallest section height, otherwise it won't get selected correctly
                float selectionLenienceAboveSection = Math.Min(smallestSectionHeight / 2.0f, scrollContainer.DisplayableContent * 0.05f);

                float scrollCentre = fixedHeaderSize + scrollContainer.DisplayableContent * scroll_y_centre + selectionLenienceAboveSection;

                var presentChildren = flowChildren.Where(c => c.IsPresent);

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

            // if a fixed header is present, apply top padding for it
            // to make the scroll container aware of its displayable area.
            // (i.e. for page up/down to work properly)
            scrollContainer.Padding = new MarginPadding { Top = FixedHeader?.LayoutSize.Y ?? 0 };

            var newMargin = originalSectionsMargin;
            newMargin.Top += (ExpandableHeader?.LayoutSize.Y ?? 0);
            newMargin.Bottom += (footerHeight ?? 0);

            scrollContentContainer.Margin = newMargin;
        }
    }
}
