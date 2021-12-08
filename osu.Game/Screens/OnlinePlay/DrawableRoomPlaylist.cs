// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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

        private readonly bool allowEdit;
        private readonly bool allowSelection;
        private readonly bool showItemOwner;

        public DrawableRoomPlaylist(bool allowEdit, bool allowSelection, bool showItemOwner = false)
        {
            this.allowEdit = allowEdit;
            this.allowSelection = allowSelection;
            this.showItemOwner = showItemOwner;
        }

        protected override ScrollContainer<Drawable> CreateScrollContainer() => base.CreateScrollContainer().With(d =>
        {
            d.ScrollbarVisible = false;
        });

        protected override FillFlowContainer<RearrangeableListItem<PlaylistItem>> CreateListFillFlowContainer() => new FillFlowContainer<RearrangeableListItem<PlaylistItem>>
        {
            Spacing = new Vector2(0, 2)
        };

        protected override OsuRearrangeableListItem<PlaylistItem> CreateOsuDrawable(PlaylistItem item) => new DrawableRoomPlaylistItem(item, allowEdit, allowSelection, showItemOwner)
        {
            SelectedItem = { BindTarget = SelectedItem },
            RequestDeletion = i => DeletionRequested?.Invoke(i)
        };
    }
}
