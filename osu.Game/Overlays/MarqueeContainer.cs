// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
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
        public Bindable<bool> AllowScrolling { get; } = new BindableBool(true);

        /// <summary>
        /// The <see cref="Anchor"/> to anchor the content to if it does not overflow.
        /// </summary>
        public Anchor NonOverflowingContentAnchor { get; init; } = Anchor.TopLeft;

        public Bindable<Func<Drawable>> CreateContent = new Bindable<Func<Drawable>>();

        private const float initial_move_delay = 1000;
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

            AllowScrolling.BindValueChanged(_ => ScheduleAfterChildren(updateScrolling));
            CreateContent.BindValueChanged(_ =>
            {
                flow.Clear();
                flow.Add(mainContent = CreateContent.Value.Invoke());
                flow.Add(fillerContent = CreateContent.Value.Invoke().With(d => d.Alpha = 0));
                ScheduleAfterChildren(updateScrolling);
            }, true);
        }

        private void updateScrolling()
        {
            float overflowWidth = mainContent.DrawWidth + padding - DrawWidth;

            if (overflowWidth > 0 && AllowScrolling.Value)
            {
                fillerContent.Alpha = 1;
                flow.Anchor = Anchor.TopLeft;
                flow.Origin = Anchor.TopLeft;

                float targetX = mainContent.DrawWidth + padding;

                flow.MoveToX(0)
                    .Delay(initial_move_delay)
                    .MoveToX(-targetX, targetX * 1000 / pixels_per_second)
                    .Loop();
            }
            else
            {
                fillerContent.Alpha = 0;
                flow.ClearTransforms();
                flow.X = 0;
                flow.Anchor = NonOverflowingContentAnchor;
                flow.Origin = NonOverflowingContentAnchor;
            }
        }
    }
}
