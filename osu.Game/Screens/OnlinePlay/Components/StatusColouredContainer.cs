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
            updateRoomStatus();
        }

        private void onRoomPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Room.Status))
                updateRoomStatus();
        }

        private void updateRoomStatus()
        {
            this.FadeColour(colours.ForRoomCategory(room.Category) ?? room.Status.GetAppropriateColour(colours), transitionDuration);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            room.PropertyChanged -= onRoomPropertyChanged;
        }
    }
}
