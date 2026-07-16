// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.Database;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osuTK;
using Realms;

namespace osu.Game.Screens.Select
{
    /// <summary>
    /// A dropdown to select the collection to be used to filter results.
    /// </summary>
    public partial class CollectionDropdown : ShearedDropdown<CollectionFilterMenuItem> // TODO: partial class under FilterControl?
    {
        /// <summary>
        /// Whether to show the "manage collections..." menu item in the dropdown.
        /// </summary>
        protected virtual bool ShowManageCollectionsItem => true;

        private readonly BindableList<CollectionFilterMenuItem> filters = new BindableList<CollectionFilterMenuItem>();

        [Resolved]
        private ManageCollectionsDialog? manageCollectionsDialog { get; set; }

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        private IDisposable? realmSubscription;

        private readonly CollectionFilterMenuItem allBeatmapsItem = new AllBeatmapsCollectionFilterMenuItem();

        public CollectionDropdown()
            : base(CollectionsStrings.Collection)
        {
            ItemSource = filters;

            Current.Value = allBeatmapsItem;
            AlwaysShowSearchBar = true;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            realmSubscription = realm.RegisterForNotifications(r => r.All<BeatmapCollection>(), collectionsChanged);

            Current.BindValueChanged(selectionChanged);
        }

        private void SortFilters()
        {
            var sorted = filters.ToList()
                .OrderBy(filter => filter.CollectionName.ToString(), StringComparer.OrdinalIgnoreCase)
                .ToList();
            filters.Clear();
            filters.AddRange(sorted);
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
                SortFilters();
            }
            else
            {
                var selectedId = SelectedItem?.Value?.Collection?.ID;

                // rebuild list
                filters.Clear();
                filters.Add(allBeatmapsItem);
                filters.AddRange(collections.Select(c => new CollectionFilterMenuItem(c.ToLive(realm))));

                if (ShowManageCollectionsItem)
                    filters.Add(new ManageCollectionsFilterMenuItem());
                SortFilters();

                // if still exists
                if (selectedId.HasValue)
                {
                    var selectedFilter = filters.FirstOrDefault(filter => filter.Collection?.ID == selectedId);
                    if (selectedFilter != null)
                        Current.Value = selectedFilter;
                    else
                        Current.Value = allBeatmapsItem;
                }
                else
                {
                    Current.Value = allBeatmapsItem;
                }
            }
        }

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
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            realmSubscription?.Dispose();
        }

        protected override LocalisableString GenerateItemText(CollectionFilterMenuItem item) => item.CollectionName;

        protected sealed override DropdownMenu CreateMenu() => CreateCollectionMenu();

        protected virtual ShearedCollectionDropdownMenu CreateCollectionMenu() => new ShearedCollectionDropdownMenu();

        protected partial class ShearedCollectionDropdownMenu : ShearedDropdownMenu
        {
            public ShearedCollectionDropdownMenu()
            {
                MaxHeight = 200;
            }

            protected override DrawableDropdownMenuItem CreateDrawableDropdownMenuItem(MenuItem item) => new DrawableCollectionMenuItem(item)
            {
                BackgroundColourHover = HoverColour,
                BackgroundColourSelected = SelectionColour
            };
        }

        protected partial class DrawableCollectionMenuItem : ShearedDropdownMenu.ShearedMenuItem
        {
            private IconButton addOrRemoveButton = null!;

            private bool beatmapInCollection;

            private readonly Live<BeatmapCollection>? collection;

            [Resolved]
            private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

            public DrawableCollectionMenuItem(MenuItem item)
                : base(item)
            {
                collection = ((DropdownMenuItem<CollectionFilterMenuItem>)item).Value.Collection;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                AddInternal(addOrRemoveButton = new NoFocusChangeIconButton
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Shear = -OsuGame.SHEAR,
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
                        addOrRemoveButton.TooltipText = beatmapInCollection ? CollectionsStrings.RemoveSelectedBeatmap : CollectionsStrings.AddSelectedBeatmap;

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

                Task.Run(() => collection.PerformWrite(c =>
                {
                    if (!c.BeatmapMD5Hashes.Remove(beatmap.Value.BeatmapInfo.MD5Hash))
                        c.BeatmapMD5Hashes.Add(beatmap.Value.BeatmapInfo.MD5Hash);
                }));
            }

            protected override Drawable CreateContent() => (Content)base.CreateContent();

            private partial class NoFocusChangeIconButton : IconButton
            {
                public override bool ChangeFocusOnClick => false;
            }
        }
    }
}
