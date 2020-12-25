// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Online.Rooms;
using osu.Game.Online.Rooms.RoomStatuses;

namespace osu.Game.Screens.OnlinePlay.Components
{
    public class RoomStatusInfo : OnlinePlayComposite
    {
        public RoomStatusInfo()
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
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
                        Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 14)
                    },
                    endDatePart = new EndDatePart { Font = OsuFont.GetFont(size: 14) }
                }
            };

            statusPart.EndDate.BindTo(EndDate);
            statusPart.Status.BindTo(Status);
            statusPart.Availability.BindTo(Availability);
            endDatePart.EndDate.BindTo(EndDate);
        }

        private class EndDatePart : DrawableDate
        {
            public readonly IBindable<DateTimeOffset?> EndDate = new Bindable<DateTimeOffset?>();

            public EndDatePart()
                : base(DateTimeOffset.UtcNow)
            {
                EndDate.BindValueChanged(date =>
                {
                    // If null, set a very large future date to prevent unnecessary schedules.
                    Date = date.NewValue ?? DateTimeOffset.Now.AddYears(1);
                }, true);
            }

            protected override string Format()
            {
                if (EndDate.Value == null)
                    return string.Empty;

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
