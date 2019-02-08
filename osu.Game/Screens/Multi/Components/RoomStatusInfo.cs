// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.RoomStatuses;

namespace osu.Game.Screens.Multi.Components
{
    public class RoomStatusInfo : CompositeDrawable
    {
        private readonly RoomBindings bindings = new RoomBindings();

        public RoomStatusInfo(Room room)
        {
            bindings.Room = room;

            AutoSizeAxes = Axes.Both;

            StatusPart statusPart;
            EndDatePart endDatePart;

            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    statusPart = new StatusPart
                    {
                        TextSize = 14,
                        Font = "Exo2.0-Bold"
                    },
                    endDatePart = new EndDatePart { TextSize = 14 }
                }
            };

            statusPart.EndDate.BindTo(bindings.EndDate);
            statusPart.Status.BindTo(bindings.Status);
            statusPart.Availability.BindTo(bindings.Availability);
            endDatePart.EndDate.BindTo(bindings.EndDate);
        }

        private class EndDatePart : DrawableDate
        {
            public readonly IBindable<DateTimeOffset> EndDate = new Bindable<DateTimeOffset>();

            public EndDatePart()
                : base(DateTimeOffset.UtcNow)
            {
                EndDate.BindValueChanged(d => Date = d);
            }

            protected override string Format()
            {
                var diffToNow = Date.Subtract(DateTimeOffset.Now);

                if (diffToNow.TotalSeconds < -5)
                    return $"Closed {base.Format()}";

                if (diffToNow.TotalSeconds < 0)
                    return "Closed";

                if (diffToNow.TotalSeconds < 5)
                    return "Closing soon";

                return $"Closing {base.Format()}";
            }
        }

        private class StatusPart : EndDatePart
        {
            public readonly IBindable<RoomStatus> Status = new Bindable<RoomStatus>();
            public readonly IBindable<RoomAvailability> Availability = new Bindable<RoomAvailability>();

            [Resolved]
            private OsuColour colours { get; set; }

            public StatusPart()
            {
                EndDate.BindValueChanged(_ => Format());
                Status.BindValueChanged(_ => Format());
                Availability.BindValueChanged(_ => Format());
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Text = Format();
            }

            protected override string Format()
            {
                if (!IsLoaded)
                    return string.Empty;

                RoomStatus status = Date < DateTimeOffset.Now ? new RoomStatusEnded() : Status.Value ?? new RoomStatusOpen();

                this.FadeColour(status.GetAppropriateColour(colours), 100);
                return $"{Availability.Value.GetDescription()}, {status.Message}";
            }
        }
    }
}
