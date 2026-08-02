// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Online.Rooms;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Lounge.Components
{
    public partial class RoomSpecialCategoryPill : OnlinePlayPill
    {
        private readonly Room room;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        protected override FontUsage Font => base.Font.With(weight: FontWeight.SemiBold);

        public RoomSpecialCategoryPill(Room room)
        {
            this.room = room;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Pill.Background.Alpha = 1;
            TextFlow.Colour = Color4.Black;

            room.PropertyChanged += onRoomPropertyChanged;
            updateRoomCategory();
        }

        private void onRoomPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Room.Category))
                updateRoomCategory();
        }

        private void updateRoomCategory()
        {
            TextFlow.Text = room.Category.GetLocalisableDescription();
            Pill.Background.Colour = colours.ForRoomCategory(room.Category) ?? colours.Pink;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            room.PropertyChanged -= onRoomPropertyChanged;
        }
    }
}
