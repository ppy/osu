// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osuTK;
using Realms;

namespace osu.Game.Collections
{
    /// <summary>
    /// A dropdown to select the <see cref="CollectionFilterMenuItem"/> to filter beatmaps using.
    /// </summary>
    public class CollectionFilterDropdown : OsuDropdown<CollectionFilterMenuItem>
    {
        /// <summary>
        /// Whether to show the "manage collections..." menu item in the dropdown.
        /// </summary>
        protected virtual bool ShowManageCollectionsItem => true;

        public Action? RequestFilter { private get; set; }

        private readonly BindableList<CollectionFilterMenuItem> filters = new BindableList<CollectionFilterMenuItem>();

        [Resolved]
        private ManageCollectionsDialog? manageCollectionsDialog { get; set; }

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        public CollectionFilterDropdown()
        {
            ItemSource = filters;

            Current.Value = new AllBeatmapsCollectionFilterMenuItem();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            realm.RegisterForNotifications(r => r.All<BeatmapCollection>(), collectionsChanged);

            Current.BindValueChanged(currentChanged);
        }

        private void collectionsChanged(IRealmCollection<BeatmapCollection> collections, ChangeSet? changes, Exception error)
        {
            var selectedItem = SelectedItem?.Value?.Collection;

            filters.Clear();
            filters.Add(new AllBeatmapsCollectionFilterMenuItem());
            filters.AddRange(collections.Select(c => new CollectionFilterMenuItem(c.ToLive(realm))));

            if (ShowManageCollectionsItem)
                filters.Add(new ManageCollectionsFilterMenuItem());

            Current.Value = filters.SingleOrDefault(f => f.Collection != null && f.Collection.ID == selectedItem?.ID) ?? filters[0];

            // Trigger a re-filter if the current item was in the change set.
            if (selectedItem != null && changes != null)
            {
                foreach (int index in changes.ModifiedIndices)
                {
                    if (collections[index].ID == selectedItem.ID)
                        RequestFilter?.Invoke();
                }
            }
        }

        private void currentChanged(ValueChangedEvent<CollectionFilterMenuItem> filter)
        {
            // May be null during .Clear().
            if (filter.NewValue == null)
                return;

            // Never select the manage collection filter - rollback to the previous filter.
            // This is done after the above since it is important that bindable is unbound from OldValue, which is lost after forcing it back to the old value.
            if (filter.NewValue is ManageCollectionsFilterMenuItem)
            {
                Current.Value = filter.OldValue;
                manageCollectionsDialog?.Show();
                return;
            }

            // This dropdown be weird.
            // We only care about filtering if the actual collection has changed.
            if (filter.OldValue?.Collection != null || filter.NewValue?.Collection != null)
                RequestFilter?.Invoke();
        }

        protected override LocalisableString GenerateItemText(CollectionFilterMenuItem item) => item.CollectionName;

        protected sealed override DropdownHeader CreateHeader() => CreateCollectionHeader().With(d => d.SelectedItem.BindTarget = Current);

        protected sealed override DropdownMenu CreateMenu() => CreateCollectionMenu();

        protected virtual CollectionDropdownHeader CreateCollectionHeader() => new CollectionDropdownHeader();

        protected virtual CollectionDropdownMenu CreateCollectionMenu() => new CollectionDropdownMenu();

        public class CollectionDropdownHeader : OsuDropdownHeader
        {
            public readonly Bindable<CollectionFilterMenuItem> SelectedItem = new Bindable<CollectionFilterMenuItem>();

            public CollectionDropdownHeader()
            {
                Height = 25;
                Icon.Size = new Vector2(16);
                Foreground.Padding = new MarginPadding { Top = 4, Bottom = 4, Left = 8, Right = 4 };
            }
        }

        protected class CollectionDropdownMenu : OsuDropdownMenu
        {
            public CollectionDropdownMenu()
            {
                MaxHeight = 200;
            }

            protected override DrawableDropdownMenuItem CreateDrawableDropdownMenuItem(MenuItem item) => new CollectionDropdownMenuItem(item)
            {
                BackgroundColourHover = HoverColour,
                BackgroundColourSelected = SelectionColour
            };
        }

        protected class CollectionDropdownMenuItem : OsuDropdownMenu.DrawableOsuDropdownMenuItem
        {
            protected new CollectionFilterMenuItem Item => ((DropdownMenuItem<CollectionFilterMenuItem>)base.Item).Value;

            private IconButton addOrRemoveButton = null!;

            private bool beatmapInCollection;

            [Resolved]
            private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

            public CollectionDropdownMenuItem(MenuItem item)
                : base(item)
            {
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                AddInternal(addOrRemoveButton = new IconButton
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    X = -OsuScrollContainer.SCROLL_BAR_HEIGHT,
                    Scale = new Vector2(0.65f),
                    Action = addOrRemove,
                });
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                if (Item.Collection != null)
                {
                    beatmap.BindValueChanged(_ =>
                    {
                        Debug.Assert(Item.Collection != null);

                        beatmapInCollection = Item.Collection.PerformRead(c => c.BeatmapMD5Hashes.Contains(beatmap.Value.BeatmapInfo.MD5Hash));

                        addOrRemoveButton.Enabled.Value = !beatmap.IsDefault;
                        addOrRemoveButton.Icon = beatmapInCollection ? FontAwesome.Solid.MinusSquare : FontAwesome.Solid.PlusSquare;
                        addOrRemoveButton.TooltipText = beatmapInCollection ? "Remove selected beatmap" : "Add selected beatmap";

                        updateButtonVisibility();
                    }, true);
                }

                updateButtonVisibility();
            }

            protected override bool OnHover(HoverEvent e)
            {
                updateButtonVisibility();
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                updateButtonVisibility();
                base.OnHoverLost(e);
            }

            protected override void OnSelectChange()
            {
                base.OnSelectChange();
                updateButtonVisibility();
            }

            private void updateButtonVisibility()
            {
                if (Item.Collection == null)
                    addOrRemoveButton.Alpha = 0;
                else
                    addOrRemoveButton.Alpha = IsHovered || IsPreSelected || beatmapInCollection ? 1 : 0;
            }

            private void addOrRemove()
            {
                Debug.Assert(Item.Collection != null);

                Item.Collection.PerformWrite(c =>
                {
                    if (!c.BeatmapMD5Hashes.Remove(beatmap.Value.BeatmapInfo.MD5Hash))
                        c.BeatmapMD5Hashes.Add(beatmap.Value.BeatmapInfo.MD5Hash);
                });
            }

            protected override Drawable CreateContent() => (Content)base.CreateContent();
        }
    }
}
