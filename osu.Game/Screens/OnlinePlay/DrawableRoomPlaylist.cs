// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osu.Game.Online.Rooms;
using osuTK;

namespace osu.Game.Screens.OnlinePlay
{
    /// <summary>
    /// A list scrollable list which displays the <see cref="PlaylistItem"/>s in a <see cref="Room"/>.
    /// </summary>
    public class DrawableRoomPlaylist : OsuRearrangeableListContainer<PlaylistItem>
    {
        /// <summary>
        /// The currently-selected item, used to show a border around items.
        /// May be updated by playlist items if <see cref="AllowSelection"/> is <c>true</c>.
        /// </summary>
        public readonly Bindable<PlaylistItem> SelectedItem = new Bindable<PlaylistItem>();

        /// <summary>
        /// Invoked when an item is requested to be deleted.
        /// </summary>
        public Action<PlaylistItem> DeletionRequested;

        /// <summary>
        /// Invoked when an item requests its results to be shown.
        /// </summary>
        public Action<PlaylistItem> ShowResultsRequested;

        private bool allowReordering;

        /// <summary>
        /// Whether to allow reordering items in the playlist.
        /// </summary>
        public bool AllowReordering
        {
            get => allowReordering;
            set
            {
                allowReordering = value;

                foreach (var item in ListContainer.OfType<DrawableRoomPlaylistItem>())
                    item.AllowReordering = value;
            }
        }

        private bool allowDeletion;

        /// <summary>
        /// Whether to allow deleting items from the playlist.
        /// If <c>true</c>, requests to delete items may be satisfied via <see cref="DeletionRequested"/>.
        /// </summary>
        public bool AllowDeletion
        {
            get => allowDeletion;
            set
            {
                allowDeletion = value;

                foreach (var item in ListContainer.OfType<DrawableRoomPlaylistItem>())
                    item.AllowDeletion = value;
            }
        }

        private bool allowSelection;

        /// <summary>
        /// Whether to allow selecting items from the playlist.
        /// If <c>true</c>, clicking on items in the playlist will change the value of <see cref="SelectedItem"/>.
        /// </summary>
        public bool AllowSelection
        {
            get => allowSelection;
            set
            {
                allowSelection = value;

                foreach (var item in ListContainer.OfType<DrawableRoomPlaylistItem>())
                    item.AllowSelection = value;
            }
        }

        private bool allowShowingResults;

        /// <summary>
        /// Whether to allow items to request their results to be shown.
        /// If <c>true</c>, requests to show the results may be satisfied via <see cref="ShowResultsRequested"/>.
        /// </summary>
        public bool AllowShowingResults
        {
            get => allowShowingResults;
            set
            {
                allowShowingResults = value;

                foreach (var item in ListContainer.OfType<DrawableRoomPlaylistItem>())
                    item.AllowShowingResults = value;
            }
        }

        private bool showItemOwners;

        /// <summary>
        /// Whether to show the avatar of users which own each playlist item.
        /// </summary>
        public bool ShowItemOwners
        {
            get => showItemOwners;
            set
            {
                showItemOwners = value;

                foreach (var item in ListContainer.OfType<DrawableRoomPlaylistItem>())
                    item.ShowItemOwner = value;
            }
        }

        protected override ScrollContainer<Drawable> CreateScrollContainer() => base.CreateScrollContainer().With(d =>
        {
            d.ScrollbarVisible = false;
        });

        protected override FillFlowContainer<RearrangeableListItem<PlaylistItem>> CreateListFillFlowContainer() => new FillFlowContainer<RearrangeableListItem<PlaylistItem>>
        {
            Spacing = new Vector2(0, 2)
        };

        protected override OsuRearrangeableListItem<PlaylistItem> CreateOsuDrawable(PlaylistItem item) => new DrawableRoomPlaylistItem(item)
        {
            SelectedItem = { BindTarget = SelectedItem },
            RequestDeletion = i => DeletionRequested?.Invoke(i),
            AllowReordering = AllowReordering,
            AllowDeletion = AllowDeletion,
            AllowSelection = AllowSelection,
            AllowShowingResults = AllowShowingResults,
            ShowItemOwner = ShowItemOwners,
            ShowResultsRequested = i => ShowResultsRequested?.Invoke(i)
        };
    }
}
