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
    public class DrawableRoomPlaylist : OsuRearrangeableListContainer<PlaylistItem>
    {
        public readonly Bindable<PlaylistItem> SelectedItem = new Bindable<PlaylistItem>();

        /// <summary>
        /// Invoked when an item is requested to be deleted.
        /// </summary>
        public Action<PlaylistItem> DeletionRequested;

        /// <summary>
        /// Invoked to request showing the results for an item.
        /// </summary>
        public Action<PlaylistItem> ShowResultsRequested;

        private bool allowReordering;

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
