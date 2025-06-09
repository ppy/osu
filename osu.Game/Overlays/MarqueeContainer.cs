// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace osu.Game.Overlays
{
    public partial class MarqueeContainer : CompositeDrawable
    {
        /// <summary>
        /// Whether the marquee should be allowed to scroll the content if it overflows.
        /// Note that upon changing the value of this, any existing scrolls will be terminated instantly.
        /// </summary>
        public bool AllowScrolling
        {
            get => allowScrolling;
            set
            {
                allowScrolling = value;
                ScheduleAfterChildren(updateScrolling);
            }
        }

        private bool allowScrolling = true;

        /// <summary>
        /// Time in milliseconds before scrolling begins.
        /// </summary>
        public double InitialMoveDelay { get; set; } = 1000;

        /// <summary>
        /// The <see cref="Anchor"/> to anchor the content to if it does not overflow.
        /// </summary>
        public Anchor NonOverflowingContentAnchor { get; init; } = Anchor.TopLeft;

        public Func<Drawable>? CreateContent
        {
            set
            {
                createContent = value;
                if (IsLoaded)
                    updateContent();
            }
        }

        private Func<Drawable>? createContent;

        private const float pixels_per_second = 50;
        private const float padding = 15;

        private Drawable mainContent = null!;
        private Drawable fillerContent = null!;
        private FillFlowContainer flow = null!;

        public MarqueeContainer()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = flow = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Anchor = NonOverflowingContentAnchor,
                Origin = NonOverflowingContentAnchor,
                Spacing = new Vector2(padding),
                Padding = new MarginPadding { Horizontal = padding },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            updateContent();
        }

        private void updateContent()
        {
            flow.Clear();

            if (createContent == null)
                return;

            flow.Add(mainContent = createContent());
            flow.Add(fillerContent = createContent().With(d => d.Alpha = 0));
            ScheduleAfterChildren(updateScrolling);
        }

        private void updateScrolling()
        {
            float overflowWidth = mainContent.DrawWidth + padding - DrawWidth;

            if (overflowWidth > 0 && AllowScrolling)
            {
                fillerContent.Alpha = 1;
                flow.Anchor = Anchor.TopLeft;
                flow.Origin = Anchor.TopLeft;

                float targetX = mainContent.DrawWidth + padding;

                flow.MoveToX(0)
                    .Delay(InitialMoveDelay)
                    .MoveToX(-targetX, targetX * 1000 / pixels_per_second)
                    .Loop();
            }
            else
            {
                fillerContent.Alpha = 0;
                flow.ClearTransforms();
                flow.MoveToX(0, 300, Easing.OutQuint);
                flow.Anchor = NonOverflowingContentAnchor;
                flow.Origin = NonOverflowingContentAnchor;
            }
        }
    }
}
