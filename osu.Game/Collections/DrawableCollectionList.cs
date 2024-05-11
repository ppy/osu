// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Database;
using osu.Game.Graphics.Containers;
using osuTK;
using Realms;

namespace osu.Game.Collections
{
    /// <summary>
    /// Visualises a list of <see cref="BeatmapCollection"/>s.
    /// </summary>
    public partial class DrawableCollectionList : OsuRearrangeableListContainer<Live<BeatmapCollection>>
    {
        protected override ScrollContainer<Drawable> CreateScrollContainer() => scroll = new Scroll();

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        private Scroll scroll = null!;

        private IDisposable? realmSubscription;

        protected override FillFlowContainer<RearrangeableListItem<Live<BeatmapCollection>>> CreateListFillFlowContainer() => new Flow
        {
            DragActive = { BindTarget = DragActive }
        };

        protected override void LoadComplete()
        {
            base.LoadComplete();

            realmSubscription = realm.RegisterForNotifications(r => r.All<BeatmapCollection>().OrderBy(c => c.Name), collectionsChanged);
        }

        private void collectionsChanged(IRealmCollection<BeatmapCollection> collections, ChangeSet? changes)
        {
            Items.Clear();
            Items.AddRange(collections.AsEnumerable().Select(c => c.ToLive(realm)));
        }

        protected override OsuRearrangeableListItem<Live<BeatmapCollection>> CreateOsuDrawable(Live<BeatmapCollection> item)
        {
            if (item.ID == scroll.PlaceholderItem.Model.ID)
                return scroll.ReplacePlaceholder();

            return new DrawableCollectionListItem(item, true);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            realmSubscription?.Dispose();
        }

        /// <summary>
        /// The scroll container for this <see cref="DrawableCollectionList"/>.
        /// Contains the main flow of <see cref="DrawableCollectionListItem"/> and attaches a placeholder item to the end of the list.
        /// </summary>
        /// <remarks>
        /// Use <see cref="ReplacePlaceholder"/> to transfer the placeholder into the main list.
        /// </remarks>
        private partial class Scroll : OsuScrollContainer
        {
            /// <summary>
            /// The currently-displayed placeholder item.
            /// </summary>
            public DrawableCollectionListItem PlaceholderItem { get; private set; } = null!;

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
                Debug.Assert(PlaceholderItem != null);
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
                placeholderContainer.Add(PlaceholderItem = new DrawableCollectionListItem(new BeatmapCollection().ToLiveUnmanaged(), false));

                return previous;
            }
        }

        /// <summary>
        /// The flow of <see cref="DrawableCollectionListItem"/>. Disables layout easing unless a drag is in progress.
        /// </summary>
        private partial class Flow : FillFlowContainer<RearrangeableListItem<Live<BeatmapCollection>>>
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
