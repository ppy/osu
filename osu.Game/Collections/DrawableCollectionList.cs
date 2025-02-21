// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
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
        public new MarginPadding Padding
        {
            get => base.Padding;
            set => base.Padding = value;
        }

        protected override ScrollContainer<Drawable> CreateScrollContainer() => scroll = new Scroll();

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        private Scroll scroll = null!;

        private IDisposable? realmSubscription;

        private Flow flow = null!;

        public IEnumerable<Drawable> OrderedItems => flow.FlowingChildren;

        public string SearchTerm
        {
            get => flow.SearchTerm;
            set => flow.SearchTerm = value;
        }

        protected override FillFlowContainer<RearrangeableListItem<Live<BeatmapCollection>>> CreateListFillFlowContainer() => flow = new Flow
        {
            DragActive = { BindTarget = DragActive }
        };

        protected override void LoadComplete()
        {
            base.LoadComplete();

            realmSubscription = realm.RegisterForNotifications(r => r.All<BeatmapCollection>().OrderBy(c => c.Name), collectionsChanged);
        }

        /// <summary>
        /// When non-null, signifies that a new collection was created and should be presented to the user.
        /// </summary>
        private Guid? lastCreated;

        protected override void OnItemsChanged()
        {
            base.OnItemsChanged();

            if (lastCreated != null)
            {
                var createdItem = flow.Children.SingleOrDefault(item => item.Model.Value.ID == lastCreated);

                if (createdItem != null)
                    scroll.ScrollTo(createdItem);

                lastCreated = null;
            }
        }

        private void collectionsChanged(IRealmCollection<BeatmapCollection> collections, ChangeSet? changes)
        {
            if (changes == null)
            {
                Items.AddRange(collections.AsEnumerable().Select(c => c.ToLive(realm)));
                return;
            }

            foreach (int i in changes.DeletedIndices.OrderDescending())
                Items.RemoveAt(i);

            foreach (int i in changes.InsertedIndices)
                Items.Insert(i, collections[i].ToLive(realm));

            if (changes.InsertedIndices.Length == 1)
                lastCreated = collections[changes.InsertedIndices[0]].ID;

            foreach (int i in changes.NewModifiedIndices)
            {
                var updatedItem = collections[i];

                Items.RemoveAt(i);
                Items.Insert(i, updatedItem.ToLive(realm));
            }
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
                ScrollbarOverlapsContent = false;

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
                content.Height = ((Flow)Child).Children.Sum(c => c.IsPresent ? c.DrawHeight + 5 : 0);
            }

            /// <summary>
            /// Replaces the current <see cref="PlaceholderItem"/> with a new one, and returns the previous.
            /// </summary>
            /// <returns>The current <see cref="PlaceholderItem"/>.</returns>
            public DrawableCollectionListItem ReplacePlaceholder()
            {
                var previous = PlaceholderItem;

                placeholderContainer.Clear(false);
                placeholderContainer.Add(PlaceholderItem = new NewCollectionEntryItem());

                return previous;
            }
        }

        private partial class NewCollectionEntryItem : DrawableCollectionListItem
        {
            [Resolved]
            private RealmAccess realm { get; set; } = null!;

            public NewCollectionEntryItem()
                : base(new BeatmapCollection().ToLiveUnmanaged(), false)
            {
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                TextBox.OnCommit += (sender, newText) =>
                {
                    if (string.IsNullOrEmpty(TextBox.Text))
                        return;

                    realm.Write(r => r.Add(new BeatmapCollection(TextBox.Text)));
                    TextBox.Text = string.Empty;
                };
            }
        }

        /// <summary>
        /// The flow of <see cref="DrawableCollectionListItem"/>. Disables layout easing unless a drag is in progress.
        /// </summary>
        private partial class Flow : SearchContainer<RearrangeableListItem<Live<BeatmapCollection>>>
        {
            public readonly IBindable<bool> DragActive = new Bindable<bool>();

            public Flow()
            {
                Spacing = new Vector2(0, 5);
                LayoutEasing = Easing.OutQuint;

                Padding = new MarginPadding { Right = 5 };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                DragActive.BindValueChanged(active => LayoutDuration = active.NewValue ? 200 : 0);
            }
        }
    }
}
