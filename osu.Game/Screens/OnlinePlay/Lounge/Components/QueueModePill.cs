// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Extensions;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Lounge.Components
{
    public partial class QueueModePill : OnlinePlayPill
    {
        private readonly Room room;

        public QueueModePill(Room room)
        {
            this.room = room;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            room.PropertyChanged += onRoomPropertyChanged;
            updateRoomQueueMode();
        }

        private void onRoomPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Room.QueueMode))
                updateRoomQueueMode();
        }

        private void updateRoomQueueMode()
            => TextFlow.Text = room.QueueMode.GetLocalisableDescription();

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            room.PropertyChanged -= onRoomPropertyChanged;
        }
    }
}
