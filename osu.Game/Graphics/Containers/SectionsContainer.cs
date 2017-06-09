// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
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
        public readonly ScrollContainer ScrollContainer;
        private readonly Container headerBackgroundContainer;
        private readonly FlowContainer<T> sectionsContainer;

        protected override Container<T> Content => sectionsContainer;

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
                    ScrollContainer.Remove(footer);
                footer = value;
                if (value == null) return;

                footer.Anchor |= Anchor.y2;
                footer.Origin |= Anchor.y2;
                ScrollContainer.Add(footer);
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
        }

        private float headerHeight, footerHeight;
        private readonly MarginPadding originalSectionsMargin;
        private void updateSectionsMargin()
        {
            if (!Children.Any()) return;

            var newMargin = originalSectionsMargin;
            newMargin.Top += headerHeight;
            newMargin.Bottom += footerHeight;

            sectionsContainer.Margin = newMargin;
        }

        public SectionsContainer()
        {
            AddInternal(ScrollContainer = new ScrollContainer()
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                Children = new Drawable[] { sectionsContainer = CreateScrollContentContainer() },
                Depth = float.MaxValue
            });
            AddInternal(headerBackgroundContainer = new Container
            {
                RelativeSizeAxes = Axes.X,
                Depth = float.MaxValue / 2
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

                headerBackgroundContainer.Height = (ExpandableHeader?.LayoutSize.Y ?? 0) + (FixedHeader?.LayoutSize.Y ?? 0);
                headerBackgroundContainer.Y = ExpandableHeader?.Y ?? 0;

                T bestMatch = null;
                float minDiff = float.MaxValue;

                foreach (var section in Children)
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
