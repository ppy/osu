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
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Screens.Select
{
    public class CollectionFilterDropdown : OsuDropdown<FilterControl.CollectionFilter>
    {
        private readonly IBindableList<BeatmapCollection> collections = new BindableList<BeatmapCollection>();
        private readonly IBindableList<BeatmapInfo> beatmaps = new BindableList<BeatmapInfo>();
        private readonly BindableList<FilterControl.CollectionFilter> filters = new BindableList<FilterControl.CollectionFilter>();

        public CollectionFilterDropdown()
        {
            ItemSource = filters;
        }

        [BackgroundDependencyLoader]
        private void load(BeatmapCollectionManager collectionManager)
        {
            collections.BindTo(collectionManager.Collections);
            collections.CollectionChanged += (_, __) => collectionsChanged();
            collectionsChanged();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindValueChanged(filterChanged, true);
        }

        /// <summary>
        /// Occurs when a collection has been added or removed.
        /// </summary>
        private void collectionsChanged()
        {
            var selectedItem = SelectedItem?.Value?.Collection;

            filters.Clear();
            filters.Add(new FilterControl.CollectionFilter(null));
            filters.AddRange(collections.Select(c => new FilterControl.CollectionFilter(c)));

            Current.Value = filters.SingleOrDefault(f => f.Collection == selectedItem) ?? filters[0];
        }

        /// <summary>
        /// Occurs when the <see cref="FilterControl.CollectionFilter"/> selection has changed.
        /// </summary>
        private void filterChanged(ValueChangedEvent<FilterControl.CollectionFilter> filter)
        {
            beatmaps.CollectionChanged -= filterBeatmapsChanged;

            if (filter.OldValue?.Collection != null)
                beatmaps.UnbindFrom(filter.OldValue.Collection.Beatmaps);

            if (filter.NewValue?.Collection != null)
                beatmaps.BindTo(filter.NewValue.Collection.Beatmaps);

            beatmaps.CollectionChanged += filterBeatmapsChanged;
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

        protected override string GenerateItemText(FilterControl.CollectionFilter item) => item.Collection?.Name.Value ?? "All beatmaps";

        protected override DropdownHeader CreateHeader() => new CollectionDropdownHeader();

        protected override DropdownMenu CreateMenu() => new CollectionDropdownMenu();

        private class CollectionDropdownHeader : OsuDropdownHeader
        {
            public CollectionDropdownHeader()
            {
                Height = 25;
                Icon.Size = new Vector2(16);
                Foreground.Padding = new MarginPadding { Top = 4, Bottom = 4, Left = 8, Right = 4 };
            }
        }

        private class CollectionDropdownMenu : OsuDropdownMenu
        {
            public CollectionDropdownMenu()
            {
                MaxHeight = 200;
            }

            protected override DrawableDropdownMenuItem CreateDrawableDropdownMenuItem(MenuItem item) => new CollectionDropdownMenuItem(item);
        }

        private class CollectionDropdownMenuItem : OsuDropdownMenu.DrawableOsuDropdownMenuItem
        {
            [Resolved]
            private OsuColour colours { get; set; }

            [Resolved]
            private IBindable<WorkingBeatmap> beatmap { get; set; }

            [CanBeNull]
            private readonly BindableList<BeatmapInfo> collectionBeatmaps;

            private IconButton addOrRemoveButton;

            public CollectionDropdownMenuItem(MenuItem item)
                : base(item)
            {
                collectionBeatmaps = ((DropdownMenuItem<FilterControl.CollectionFilter>)item).Value.Collection?.Beatmaps.GetBoundCopy();
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                AddRangeInternal(new Drawable[]
                {
                    addOrRemoveButton = new IconButton
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        X = -OsuScrollContainer.SCROLL_BAR_HEIGHT,
                        Scale = new Vector2(0.75f),
                        Alpha = collectionBeatmaps == null ? 0 : 1,
                        Action = addOrRemove
                    }
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
            }

            private void collectionChanged()
            {
                Debug.Assert(collectionBeatmaps != null);

                addOrRemoveButton.Enabled.Value = !beatmap.IsDefault;

                if (collectionBeatmaps.Contains(beatmap.Value.BeatmapInfo))
                {
                    addOrRemoveButton.Icon = FontAwesome.Solid.MinusSquare;
                    addOrRemoveButton.IconColour = colours.Red;
                }
                else
                {
                    addOrRemoveButton.Icon = FontAwesome.Solid.PlusSquare;
                    addOrRemoveButton.IconColour = colours.Green;
                }
            }

            private void addOrRemove()
            {
                Debug.Assert(collectionBeatmaps != null);

                if (!collectionBeatmaps.Remove(beatmap.Value.BeatmapInfo))
                    collectionBeatmaps.Add(beatmap.Value.BeatmapInfo);
            }
        }
    }
}
