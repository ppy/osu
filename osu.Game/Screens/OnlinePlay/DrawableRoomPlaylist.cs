// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
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

        private readonly bool allowEdit;
        private readonly bool allowSelection;

        public DrawableRoomPlaylist(bool allowEdit, bool allowSelection, bool reverse = false)
        {
            this.allowEdit = allowEdit;
            this.allowSelection = allowSelection;

            ((ReversibleFillFlowContainer)ListContainer).Reverse = reverse;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // Scheduled since items are removed and re-added upon rearrangement
            Items.CollectionChanged += (_, args) => Schedule(() =>
            {
                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Remove:
                        if (allowSelection && args.OldItems.Contains(SelectedItem))
                            SelectedItem.Value = null;
                        break;
                }
            });
        }

        protected override ScrollContainer<Drawable> CreateScrollContainer() => base.CreateScrollContainer().With(d =>
        {
            d.ScrollbarVisible = false;
        });

        protected override FillFlowContainer<RearrangeableListItem<PlaylistItem>> CreateListFillFlowContainer() => new ReversibleFillFlowContainer
        {
            Spacing = new Vector2(0, 2)
        };

        protected override OsuRearrangeableListItem<PlaylistItem> CreateOsuDrawable(PlaylistItem item) => new DrawableRoomPlaylistItem(item, allowEdit, allowSelection)
        {
            SelectedItem = { BindTarget = SelectedItem },
            RequestDeletion = requestDeletion
        };

        private void requestDeletion(PlaylistItem item)
        {
            if (allowSelection && SelectedItem.Value == item)
            {
                if (Items.Count == 1)
                    SelectedItem.Value = null;
                else
                    SelectedItem.Value = Items.GetNext(item) ?? Items[^2];
            }

            Items.Remove(item);
        }

        private class ReversibleFillFlowContainer : FillFlowContainer<RearrangeableListItem<PlaylistItem>>
        {
            private bool reverse;

            public bool Reverse
            {
                get => reverse;
                set
                {
                    reverse = value;
                    Invalidate();
                }
            }

            public override IEnumerable<Drawable> FlowingChildren => Reverse ? base.FlowingChildren.OrderBy(d => -GetLayoutPosition(d)) : base.FlowingChildren;
        }
    }
}
