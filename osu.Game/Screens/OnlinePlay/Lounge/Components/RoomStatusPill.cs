// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Online.Rooms;
using osu.Game.Localisation;

namespace osu.Game.Screens.OnlinePlay.Lounge.Components
{
    /// <summary>
    /// A pill that displays the room's current status.
    /// </summary>
    public partial class RoomStatusPill : OnlinePlayPill
    {
        [Resolved]
        private OsuColour colours { get; set; } = null!;

        protected override FontUsage Font => base.Font.With(weight: FontWeight.SemiBold);

        private readonly Room room;

        public RoomStatusPill(Room room)
        {
            this.room = room;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            TextFlow.Colour = Colour4.Black;
            Pill.Background.Alpha = 1;

            room.PropertyChanged += onRoomPropertyChanged;

            // Timed update required to track rooms which have hit the end time, see `HasEnded`.
            Scheduler.AddDelayed(updateDisplay, 1000, true);
            updateDisplay();
            FinishTransforms(true);
        }

        private void onRoomPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Room.Status):
                case nameof(Room.EndDate):
                case nameof(Room.HasPassword):
                    updateDisplay();
                    break;
            }
        }

        private void updateDisplay()
        {
            Pill.Background.FadeColour(colours.ForRoomStatus(room), 100);

            if (room.HasEnded)
                TextFlow.Text = RoomStatusPillStrings.Ended;
            else
            {
                switch (room.Status)
                {
                    case RoomStatus.Playing:
                        TextFlow.Text = RoomStatusPillStrings.Playing;
                        break;

                    default:
                        TextFlow.Text = room.HasPassword ? RoomStatusPillStrings.OpenPrivate : RoomStatusPillStrings.Open;
                        break;
                }
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            room.PropertyChanged -= onRoomPropertyChanged;
        }
    }
}
