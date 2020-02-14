// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osu.Game.Online.Multiplayer;
using osuTK;

namespace osu.Game.Screens.Multi
{
    public class DrawableRoomPlaylist : RearrangeableListContainer<PlaylistItem>
    {
        public readonly Bindable<PlaylistItem> SelectedItem = new Bindable<PlaylistItem>();

        private readonly bool allowEdit;
        private readonly bool allowSelection;

        public DrawableRoomPlaylist(bool allowEdit, bool allowSelection)
        {
            this.allowEdit = allowEdit;
            this.allowSelection = allowSelection;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            SelectedItem.BindValueChanged(item =>
            {
                if (item.OldValue != null && ItemMap.TryGetValue(item.OldValue, out var oldItem))
                    ((DrawableRoomPlaylistItem)oldItem).Deselect();

                if (item.NewValue != null && ItemMap.TryGetValue(item.NewValue, out var newItem))
                    ((DrawableRoomPlaylistItem)newItem).Select();
            }, true);

            Items.ItemsRemoved += items =>
            {
                if (items.Any(i => i == SelectedItem.Value))
                    SelectedItem.Value = null;
            };
        }

        protected override ScrollContainer<Drawable> CreateScrollContainer() => new OsuScrollContainer
        {
            ScrollbarVisible = false
        };

        protected override FillFlowContainer<RearrangeableListItem<PlaylistItem>> CreateListFillFlowContainer() => new FillFlowContainer<RearrangeableListItem<PlaylistItem>>
        {
            LayoutDuration = 200,
            LayoutEasing = Easing.OutQuint,
            Spacing = new Vector2(0, 2)
        };

        protected override RearrangeableListItem<PlaylistItem> CreateDrawable(PlaylistItem item) => new DrawableRoomPlaylistItem(item, allowEdit, allowSelection)
        {
            RequestSelection = requestSelection,
            RequestDeletion = requestDeletion
        };

        private void requestSelection(PlaylistItem item) => SelectedItem.Value = item;

        private void requestDeletion(PlaylistItem item)
        {
            if (SelectedItem.Value == item)
            {
                if (Items.Count == 1)
                    SelectedItem.Value = null;
                else
                    SelectedItem.Value = Items.GetNext(item) ?? Items[^2];
            }

            Items.Remove(item);
        }
    }
}
