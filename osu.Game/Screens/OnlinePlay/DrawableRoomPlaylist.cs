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
    /// A scrollable list which displays the <see cref="PlaylistItem"/>s in a <see cref="Room"/>.
    /// </summary>
    public class DrawableRoomPlaylist : OsuRearrangeableListContainer<PlaylistItem>
    {
        /// <summary>
        /// The currently-selected item. Selection is visually represented with a border.
        /// May be updated by clicking playlist items if <see cref="AllowSelection"/> is <c>true</c>.
        /// </summary>
        public readonly Bindable<PlaylistItem> SelectedItem = new Bindable<PlaylistItem>();

        /// <summary>
        /// Invoked when an item is requested to be deleted.
        /// </summary>
        public Action<PlaylistItem> RequestDeletion;

        /// <summary>
        /// Invoked when an item requests its results to be shown.
        /// </summary>
        public Action<PlaylistItem> RequestResults;

        /// <summary>
        /// Invoked when an item requests to be edited.
        /// </summary>
        public Action<PlaylistItem> RequestEdit;

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
        /// If <c>true</c>, requests to delete items may be satisfied via <see cref="RequestDeletion"/>.
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
        /// If <c>true</c>, requests to show the results may be satisfied via <see cref="RequestResults"/>.
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

        private bool allowEditing;

        /// <summary>
        /// Whether to allow items to be edited.
        /// If <c>true</c>, requests to edit items may be satisfied via <see cref="RequestEdit"/>.
        /// </summary>
        public bool AllowEditing
        {
            get => allowEditing;
            set
            {
                allowEditing = value;

                foreach (var item in ListContainer.OfType<DrawableRoomPlaylistItem>())
                    item.AllowEditing = value;
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

        protected sealed override OsuRearrangeableListItem<PlaylistItem> CreateOsuDrawable(PlaylistItem item) => CreateDrawablePlaylistItem(item).With(d =>
        {
            d.SelectedItem.BindTarget = SelectedItem;
            d.RequestDeletion = i => RequestDeletion?.Invoke(i);
            d.RequestResults = i => RequestResults?.Invoke(i);
            d.RequestEdit = i => RequestEdit?.Invoke(i);
            d.AllowReordering = AllowReordering;
            d.AllowDeletion = AllowDeletion;
            d.AllowSelection = AllowSelection;
            d.AllowShowingResults = AllowShowingResults;
            d.AllowEditing = AllowEditing;
            d.ShowItemOwner = ShowItemOwners;
        });

        protected virtual DrawableRoomPlaylistItem CreateDrawablePlaylistItem(PlaylistItem item) => new DrawableRoomPlaylistItem(item);
    }
}
