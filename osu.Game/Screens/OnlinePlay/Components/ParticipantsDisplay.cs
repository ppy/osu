// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Components
{
    public partial class ParticipantsDisplay : CompositeDrawable
    {
        public readonly Bindable<string> Details = new Bindable<string>();

        private readonly Room room;

        public ParticipantsDisplay(Room room, Direction direction)
        {
            this.room = room;
            OsuScrollContainer scroll;
            ParticipantsList list;

            AddInternal(scroll = new OsuScrollContainer(direction)
            {
                Child = list = new ParticipantsList(room)
            });

            switch (direction)
            {
                case Direction.Horizontal:
                    AutoSizeAxes = Axes.Y;
                    RelativeSizeAxes = Axes.X;

                    scroll.RelativeSizeAxes = Axes.X;
                    scroll.Height = ParticipantsList.TILE_SIZE + OsuScrollContainer.SCROLL_BAR_WIDTH + OsuScrollContainer.SCROLL_BAR_PADDING * 2;

                    list.RelativeSizeAxes = Axes.Y;
                    list.AutoSizeAxes = Axes.X;
                    break;

                case Direction.Vertical:
                    RelativeSizeAxes = Axes.Both;

                    scroll.RelativeSizeAxes = Axes.Both;

                    list.RelativeSizeAxes = Axes.X;
                    list.AutoSizeAxes = Axes.Y;
                    break;
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            room.PropertyChanged += onRoomPropertyChanged;
            updateRoomParticipantCount();
        }

        private void onRoomPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Room.MaxParticipants):
                case nameof(Room.ParticipantCount):
                    updateRoomParticipantCount();
                    break;
            }
        }

        private void updateRoomParticipantCount()
            => Details.Value = room.MaxParticipants != null ? $"{room.ParticipantCount}/{room.MaxParticipants}" : room.ParticipantCount.ToString();

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            room.PropertyChanged -= onRoomPropertyChanged;
        }
    }
}
