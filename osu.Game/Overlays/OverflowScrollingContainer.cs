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
    public partial class OverflowScrollingContainer : CompositeDrawable
    {
        public Anchor NonOverflowingContentAnchor { get; init; } = Anchor.TopLeft;

        public Bindable<Func<Drawable>> CreateContent = new Bindable<Func<Drawable>>();

        private const float initial_move_delay = 1000;
        private const float pixels_per_second = 50;
        private const float padding = 15;

        private Drawable mainContent = null!;
        private Drawable fillerContent = null!;
        private FillFlowContainer flow = null!;

        public OverflowScrollingContainer()
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
                Spacing = new Vector2(padding),
                Padding = new MarginPadding { Horizontal = padding },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            CreateContent.BindValueChanged(_ =>
            {
                flow.Clear();
                flow.Add(mainContent = CreateContent.Value.Invoke());
                flow.Add(fillerContent = CreateContent.Value.Invoke().With(d => d.Alpha = 0));
                ScheduleAfterChildren(updateText);
            }, true);
        }

        private void updateText()
        {
            fillerContent.Alpha = 0;

            flow.ClearTransforms();
            flow.X = 0;

            float overflowWidth = mainContent.DrawWidth + padding - DrawWidth;

            if (overflowWidth > 0)
            {
                fillerContent.Alpha = 1;

                float targetX = mainContent.DrawWidth + padding;

                flow.MoveToX(0)
                    .Delay(initial_move_delay)
                    .MoveToX(-targetX, targetX * 1000 / pixels_per_second)
                    .Loop();
                flow.Anchor = Anchor.TopLeft;
                flow.Origin = Anchor.TopLeft;
            }
            else
            {
                flow.Anchor = NonOverflowingContentAnchor;
                flow.Origin = NonOverflowingContentAnchor;
            }
        }
    }
}
