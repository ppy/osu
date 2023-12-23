// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
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
    /// A dropdown to select the collection to be used to filter results.
    /// </summary>
    public partial class CollectionDropdown : OsuDropdown<CollectionFilterMenuItem>
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

        private IDisposable? realmSubscription;

        private readonly CollectionFilterMenuItem allBeatmapsItem = new AllBeatmapsCollectionFilterMenuItem();

        public CollectionDropdown()
        {
            ItemSource = filters;

            Current.Value = allBeatmapsItem;
            AlwaysShowSearchBar = true;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            realmSubscription = realm.RegisterForNotifications(r => r.All<BeatmapCollection>().OrderBy(c => c.Name), collectionsChanged);

            Current.BindValueChanged(selectionChanged);
        }

        private void collectionsChanged(IRealmCollection<BeatmapCollection> collections, ChangeSet? changes)
        {
            if (changes == null)
            {
                filters.Clear();
                filters.Add(allBeatmapsItem);
                filters.AddRange(collections.Select(c => new CollectionFilterMenuItem(c.ToLive(realm))));
                if (ShowManageCollectionsItem)
                    filters.Add(new ManageCollectionsFilterMenuItem());
            }
            else
            {
                foreach (int i in changes.DeletedIndices)
                    filters.RemoveAt(i + 1);

                foreach (int i in changes.InsertedIndices)
                    filters.Insert(i + 1, new CollectionFilterMenuItem(collections[i].ToLive(realm)));

                var selectedItem = SelectedItem?.Value;

                foreach (int i in changes.NewModifiedIndices)
                {
                    var updatedItem = collections[i];

                    // This is responsible for updating the state of the +/- button and the collection's name.
                    // TODO: we can probably make the menu items update with changes to avoid this.
                    filters.RemoveAt(i + 1);
                    filters.Insert(i + 1, new CollectionFilterMenuItem(updatedItem.ToLive(realm)));

                    if (updatedItem.ID == selectedItem?.Collection?.ID)
                    {
                        // This current update and schedule is required to work around dropdown headers not updating text even when the selected item
                        // changes. It's not great but honestly the whole dropdown menu structure isn't great. This needs to be fixed, but I'll issue
                        // a warning that it's going to be a frustrating journey.
                        Current.Value = allBeatmapsItem;
                        Schedule(() =>
                        {
                            // current may have changed before the scheduled call is run.
                            if (Current.Value != allBeatmapsItem)
                                return;

                            Current.Value = filters.SingleOrDefault(f => f.Collection?.ID == selectedItem.Collection?.ID) ?? filters[0];
                        });

                        // Trigger an external re-filter if the current item was in the change set.
                        RequestFilter?.Invoke();
                        break;
                    }
                }
            }
        }

        private Live<BeatmapCollection>? lastFiltered;

        private void selectionChanged(ValueChangedEvent<CollectionFilterMenuItem> filter)
        {
            // May be null during .Clear().
            if (filter.NewValue.IsNull())
                return;

            // Never select the manage collection filter - rollback to the previous filter.
            // This is done after the above since it is important that bindable is unbound from OldValue, which is lost after forcing it back to the old value.
            if (filter.NewValue is ManageCollectionsFilterMenuItem)
            {
                Current.Value = filter.OldValue;
                manageCollectionsDialog?.Show();
                return;
            }

            var newCollection = filter.NewValue.Collection;

            // This dropdown be weird.
            // We only care about filtering if the actual collection has changed.
            if (newCollection != lastFiltered)
            {
                RequestFilter?.Invoke();
                lastFiltered = newCollection;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            realmSubscription?.Dispose();
        }

        protected override LocalisableString GenerateItemText(CollectionFilterMenuItem item) => item.CollectionName;

        protected sealed override DropdownHeader CreateHeader() => CreateCollectionHeader();

        protected sealed override DropdownMenu CreateMenu() => CreateCollectionMenu();

        protected virtual CollectionDropdownHeader CreateCollectionHeader() => new CollectionDropdownHeader();

        protected virtual CollectionDropdownMenu CreateCollectionMenu() => new CollectionDropdownMenu();

        public partial class CollectionDropdownHeader : OsuDropdownHeader
        {
            public CollectionDropdownHeader()
            {
                Height = 25;
                Icon.Size = new Vector2(16);
                Foreground.Padding = new MarginPadding { Top = 4, Bottom = 4, Left = 8, Right = 4 };
            }
        }

        protected partial class CollectionDropdownMenu : OsuDropdownMenu
        {
            public CollectionDropdownMenu()
            {
                MaxHeight = 200;
            }

            protected override DrawableDropdownMenuItem CreateDrawableDropdownMenuItem(MenuItem item) => new CollectionDropdownDrawableMenuItem(item)
            {
                BackgroundColourHover = HoverColour,
                BackgroundColourSelected = SelectionColour
            };
        }

        protected partial class CollectionDropdownDrawableMenuItem : OsuDropdownMenu.DrawableOsuDropdownMenuItem
        {
            private IconButton addOrRemoveButton = null!;

            private bool beatmapInCollection;

            private readonly Live<BeatmapCollection>? collection;

            [Resolved]
            private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

            public CollectionDropdownDrawableMenuItem(MenuItem item)
                : base(item)
            {
                collection = ((DropdownMenuItem<CollectionFilterMenuItem>)item).Value.Collection;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                AddInternal(addOrRemoveButton = new IconButton
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    X = -OsuScrollContainer.SCROLL_BAR_WIDTH,
                    Scale = new Vector2(0.65f),
                    Action = addOrRemove,
                });
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                if (collection != null)
                {
                    beatmap.BindValueChanged(_ =>
                    {
                        beatmapInCollection = collection.PerformRead(c => c.BeatmapMD5Hashes.Contains(beatmap.Value.BeatmapInfo.MD5Hash));

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
                if (collection == null)
                    addOrRemoveButton.Alpha = 0;
                else
                    addOrRemoveButton.Alpha = IsHovered || IsPreSelected || beatmapInCollection ? 1 : 0;
            }

            private void addOrRemove()
            {
                Debug.Assert(collection != null);

                collection.PerformWrite(c =>
                {
                    if (!c.BeatmapMD5Hashes.Remove(beatmap.Value.BeatmapInfo.MD5Hash))
                        c.BeatmapMD5Hashes.Add(beatmap.Value.BeatmapInfo.MD5Hash);
                });
            }

            protected override Drawable CreateContent() => (Content)base.CreateContent();
        }
    }
}
