// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.ComponentModel;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Lounge.Components
{
    public partial class EndDateInfo : CompositeDrawable
    {
        private readonly Room room;

        public EndDateInfo(Room room)
        {
            this.room = room;
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new EndDatePart(room)
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Font = OsuFont.GetFont(weight: FontWeight.SemiBold, size: 12)
            };
        }

        private partial class EndDatePart : DrawableDate
        {
            private readonly Room room;

            public EndDatePart(Room room)
                : base(DateTimeOffset.UtcNow)
            {
                this.room = room;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                room.PropertyChanged += onRoomPropertyChanged;
                updateEndDate();
            }

            private void onRoomPropertyChanged(object? sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == nameof(Room.EndDate))
                    updateEndDate();
            }

            private void updateEndDate()
            {
                // If null, set a very large future date to prevent unnecessary schedules.
                Date = room.EndDate ?? DateTimeOffset.Now.AddYears(1);
            }

            protected override LocalisableString Format()
            {
                if (room.EndDate == null)
                    return string.Empty;

                var diffToNow = Date.Subtract(DateTimeOffset.Now);

                if (diffToNow.TotalSeconds < -5)
                    return LocalisableString.Interpolate($"Closed {base.Format()}");

                if (diffToNow.TotalSeconds < 0)
                    return "Closed";

                if (diffToNow.TotalSeconds < 5)
                    return "Closing soon";

                return LocalisableString.Interpolate($"Closing {base.Format()}");
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);
                room.PropertyChanged -= onRoomPropertyChanged;
            }
        }
    }
}
