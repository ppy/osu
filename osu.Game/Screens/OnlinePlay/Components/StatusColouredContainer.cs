// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Online.Rooms;
using Container = osu.Framework.Graphics.Containers.Container;

namespace osu.Game.Screens.OnlinePlay.Components
{
    public partial class StatusColouredContainer : Container
    {
        [Resolved]
        private OsuColour colours { get; set; } = null!;

        private readonly double transitionDuration;
        private readonly Room room;

        public StatusColouredContainer(Room room, double transitionDuration = 100)
        {
            this.room = room;
            this.transitionDuration = transitionDuration;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            room.PropertyChanged += onRoomPropertyChanged;

            // Timed update required to track rooms which have hit the end time, see `HasEnded`.
            Scheduler.AddDelayed(updateRoomStatus, 1000, true);
            updateRoomStatus();
        }

        private void onRoomPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Room.Category):
                case nameof(Room.Status):
                case nameof(Room.EndDate):
                case nameof(Room.HasPassword):
                    updateRoomStatus();
                    break;
            }
        }

        private void updateRoomStatus()
        {
            this.FadeColour(colours.ForRoomCategory(room.Category) ?? colours.ForRoomStatus(room), transitionDuration);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            room.PropertyChanged -= onRoomPropertyChanged;
        }
    }
}
