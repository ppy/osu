// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osuTK;

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

        private readonly BindableWithCurrent<CollectionFilterMenuItem> current = new BindableWithCurrent<CollectionFilterMenuItem>();

        public new Bindable<CollectionFilterMenuItem> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        private readonly IBindableList<BeatmapCollection> collections = new BindableList<BeatmapCollection>();
        private readonly IBindableList<IBeatmapInfo> beatmaps = new BindableList<IBeatmapInfo>();
        private readonly BindableList<CollectionFilterMenuItem> filters = new BindableList<CollectionFilterMenuItem>();

        [Resolved(CanBeNull = true)]
        private ManageCollectionsDialog manageCollectionsDialog { get; set; }

        [Resolved(CanBeNull = true)]
        private CollectionManager collectionManager { get; set; }

        public CollectionFilterDropdown()
        {
            ItemSource = filters;
            Current.Value = new AllBeatmapsCollectionFilterMenuItem();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (collectionManager != null)
                collections.BindTo(collectionManager.Collections);

            // Dropdown has logic which triggers a change on the bindable with every change to the contained items.
            // This is not desirable here, as it leads to multiple filter operations running even though nothing has changed.
            // An extra bindable is enough to subvert this behaviour.
            base.Current = Current;

            collections.BindCollectionChanged((_, __) => collectionsChanged(), true);
            Current.BindValueChanged(filterChanged, true);
        }

        /// <summary>
        /// Occurs when a collection has been added or removed.
        /// </summary>
        private void collectionsChanged()
        {
            var selectedItem = SelectedItem?.Value?.Collection;

            filters.Clear();
            filters.Add(new AllBeatmapsCollectionFilterMenuItem());
            filters.AddRange(collections.Select(c => new CollectionFilterMenuItem(c)));

            if (ShowManageCollectionsItem)
                filters.Add(new ManageCollectionsFilterMenuItem());

            Current.Value = filters.SingleOrDefault(f => f.Collection != null && f.Collection == selectedItem) ?? filters[0];
        }

        /// <summary>
        /// Occurs when the <see cref="CollectionFilterMenuItem"/> selection has changed.
        /// </summary>
        private void filterChanged(ValueChangedEvent<CollectionFilterMenuItem> filter)
        {
            // Binding the beatmaps will trigger a collection change event, which results in an infinite-loop. This is rebound later, when it's safe to do so.
            beatmaps.CollectionChanged -= filterBeatmapsChanged;

            if (filter.OldValue?.Collection != null)
                beatmaps.UnbindFrom(filter.OldValue.Collection.Beatmaps);

            if (filter.NewValue?.Collection != null)
                beatmaps.BindTo(filter.NewValue.Collection.Beatmaps);

            beatmaps.CollectionChanged += filterBeatmapsChanged;

            // Never select the manage collection filter - rollback to the previous filter.
            // This is done after the above since it is important that bindable is unbound from OldValue, which is lost after forcing it back to the old value.
            if (filter.NewValue is ManageCollectionsFilterMenuItem)
            {
                Current.Value = filter.OldValue;
                manageCollectionsDialog?.Show();
            }
        }

        /// <summary>
        /// Occurs when the beatmaps contained by a <see cref="BeatmapCollection"/> have changed.
        /// </summary>
        private void filterBeatmapsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // The filtered beatmaps have changed, without the filter having changed itself. So a change in filter must be notified.
            // Note that this does NOT propagate to bound bindables, so the FilterControl must bind directly to the value change event of this bindable.
            Current.TriggerChange();
        }

        protected override LocalisableString GenerateItemText(CollectionFilterMenuItem item) => item.CollectionName.Value;

        protected sealed override DropdownHeader CreateHeader() => CreateCollectionHeader().With(d =>
        {
            d.SelectedItem.BindTarget = Current;
        });

        protected sealed override DropdownMenu CreateMenu() => CreateCollectionMenu();

        protected virtual CollectionDropdownHeader CreateCollectionHeader() => new CollectionDropdownHeader();

        protected virtual CollectionDropdownMenu CreateCollectionMenu() => new CollectionDropdownMenu();

        public class CollectionDropdownHeader : OsuDropdownHeader
        {
            public readonly Bindable<CollectionFilterMenuItem> SelectedItem = new Bindable<CollectionFilterMenuItem>();
            private readonly Bindable<string> collectionName = new Bindable<string>();

            protected override LocalisableString Label
            {
                get => base.Label;
                set { } // See updateText().
            }

            public CollectionDropdownHeader()
            {
                Height = 25;
                Icon.Size = new Vector2(16);
                Foreground.Padding = new MarginPadding { Top = 4, Bottom = 4, Left = 8, Right = 4 };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                SelectedItem.BindValueChanged(_ => updateBindable(), true);
            }

            private void updateBindable()
            {
                collectionName.UnbindAll();

                if (SelectedItem.Value != null)
                    collectionName.BindTo(SelectedItem.Value.CollectionName);

                collectionName.BindValueChanged(_ => updateText(), true);
            }

            // Dropdowns don't bind to value changes, so the real name is copied directly from the selected item here.
            private void updateText() => base.Label = collectionName.Value;
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
            [NotNull]
            protected new CollectionFilterMenuItem Item => ((DropdownMenuItem<CollectionFilterMenuItem>)base.Item).Value;

            [Resolved]
            private OsuColour colours { get; set; }

            [Resolved]
            private IBindable<WorkingBeatmap> beatmap { get; set; }

            [CanBeNull]
            private readonly BindableList<IBeatmapInfo> collectionBeatmaps;

            [NotNull]
            private readonly Bindable<string> collectionName;

            private IconButton addOrRemoveButton;
            private Content content;
            private bool beatmapInCollection;

            public CollectionDropdownMenuItem(MenuItem item)
                : base(item)
            {
                collectionBeatmaps = Item.Collection?.Beatmaps.GetBoundCopy();
                collectionName = Item.CollectionName.GetBoundCopy();
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

                if (collectionBeatmaps != null)
                {
                    collectionBeatmaps.CollectionChanged += (_, __) => collectionChanged();
                    beatmap.BindValueChanged(_ => collectionChanged(), true);
                }

                // Although the DrawableMenuItem binds to value changes of the item's text, the item is an internal implementation detail of Dropdown that has no knowledge
                // of the underlying CollectionFilter value and its accompanying name, so the real name has to be copied here. Without this, the collection name wouldn't update when changed.
                collectionName.BindValueChanged(name => content.Text = name.NewValue, true);

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

            private void collectionChanged()
            {
                Debug.Assert(collectionBeatmaps != null);

                beatmapInCollection = collectionBeatmaps.Contains(beatmap.Value.BeatmapInfo);

                addOrRemoveButton.Enabled.Value = !beatmap.IsDefault;
                addOrRemoveButton.Icon = beatmapInCollection ? FontAwesome.Solid.MinusSquare : FontAwesome.Solid.PlusSquare;
                addOrRemoveButton.TooltipText = beatmapInCollection ? "Remove selected beatmap" : "Add selected beatmap";

                updateButtonVisibility();
            }

            protected override void OnSelectChange()
            {
                base.OnSelectChange();
                updateButtonVisibility();
            }

            private void updateButtonVisibility()
            {
                if (collectionBeatmaps == null)
                    addOrRemoveButton.Alpha = 0;
                else
                    addOrRemoveButton.Alpha = IsHovered || IsPreSelected || beatmapInCollection ? 1 : 0;
            }

            private void addOrRemove()
            {
                Debug.Assert(collectionBeatmaps != null);

                if (!collectionBeatmaps.Remove(beatmap.Value.BeatmapInfo))
                    collectionBeatmaps.Add(beatmap.Value.BeatmapInfo);
            }

            protected override Drawable CreateContent() => content = (Content)base.CreateContent();
        }
    }
}
