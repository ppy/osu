// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.Game.Collections
{
    /// <summary>
    /// Visualises a list of <see cref="BeatmapCollection"/>s.
    /// </summary>
    public class DrawableCollectionList : OsuRearrangeableListContainer<BeatmapCollection>
    {
        private Scroll scroll;

        protected override ScrollContainer<Drawable> CreateScrollContainer() => scroll = new Scroll();

        protected override FillFlowContainer<RearrangeableListItem<BeatmapCollection>> CreateListFillFlowContainer() => new Flow
        {
            DragActive = { BindTarget = DragActive }
        };

        protected override OsuRearrangeableListItem<BeatmapCollection> CreateOsuDrawable(BeatmapCollection item)
        {
            if (item == scroll.PlaceholderItem.Model)
                return scroll.ReplacePlaceholder();

            return new DrawableCollectionListItem(item, true);
        }

        /// <summary>
        /// The scroll container for this <see cref="DrawableCollectionList"/>.
        /// Contains the main flow of <see cref="DrawableCollectionListItem"/> and attaches a placeholder item to the end of the list.
        /// </summary>
        /// <remarks>
        /// Use <see cref="ReplacePlaceholder"/> to transfer the placeholder into the main list.
        /// </remarks>
        private class Scroll : OsuScrollContainer
        {
            /// <summary>
            /// The currently-displayed placeholder item.
            /// </summary>
            public DrawableCollectionListItem PlaceholderItem { get; private set; }

            protected override Container<Drawable> Content => content;
            private readonly Container content;

            private readonly Container<DrawableCollectionListItem> placeholderContainer;

            public Scroll()
            {
                ScrollbarVisible = false;
                Padding = new MarginPadding(10);

                base.Content.Add(new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    LayoutDuration = 200,
                    LayoutEasing = Easing.OutQuint,
                    Children = new Drawable[]
                    {
                        content = new Container { RelativeSizeAxes = Axes.X },
                        placeholderContainer = new Container<DrawableCollectionListItem>
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y
                        }
                    }
                });

                ReplacePlaceholder();
            }

            protected override void Update()
            {
                base.Update();

                // AutoSizeAxes cannot be used as the height should represent the post-layout-transform height at all times, so that the placeholder doesn't bounce around.
                content.Height = ((Flow)Child).Children.Sum(c => c.DrawHeight + 5);
            }

            /// <summary>
            /// Replaces the current <see cref="PlaceholderItem"/> with a new one, and returns the previous.
            /// </summary>
            /// <returns>The current <see cref="PlaceholderItem"/>.</returns>
            public DrawableCollectionListItem ReplacePlaceholder()
            {
                var previous = PlaceholderItem;

                placeholderContainer.Clear(false);
                placeholderContainer.Add(PlaceholderItem = new DrawableCollectionListItem(new BeatmapCollection(), false));

                return previous;
            }
        }

        /// <summary>
        /// The flow of <see cref="DrawableCollectionListItem"/>. Disables layout easing unless a drag is in progress.
        /// </summary>
        private class Flow : FillFlowContainer<RearrangeableListItem<BeatmapCollection>>
        {
            public readonly IBindable<bool> DragActive = new Bindable<bool>();

            public Flow()
            {
                Spacing = new Vector2(0, 5);
                LayoutEasing = Easing.OutQuint;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                DragActive.BindValueChanged(active => LayoutDuration = active.NewValue ? 200 : 0);
            }
        }
    }
}
