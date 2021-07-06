// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Rooms;
using osu.Game.Online.Rooms.RoomStatuses;
using osuTK;
using osuTK.Graphics;

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
            StatusPill statusPill;
            EndDatePart endDatePart;

            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(2),
                Children = new Drawable[]
                {
                    statusPill = new StatusPill(),
                    endDatePart = new EndDatePart
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Font = OsuFont.GetFont(weight: FontWeight.SemiBold, size: 12)
                    }
                }
            };

            statusPill.EndDate.BindTo(EndDate);
            statusPill.Status.BindTo(Status);
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

        private class StatusPill : CompositeDrawable
        {
            public readonly IBindable<DateTimeOffset?> EndDate = new Bindable<DateTimeOffset?>();
            public readonly IBindable<RoomStatus> Status = new Bindable<RoomStatus>();

            [Resolved]
            private OsuColour colours { get; set; }

            private Drawable background;
            private SpriteText statusText;

            [BackgroundDependencyLoader]
            private void load()
            {
                Size = new Vector2(60, 16);

                InternalChildren = new[]
                {
                    background = new Circle { RelativeSizeAxes = Axes.Both },
                    statusText = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Font = OsuFont.GetFont(weight: FontWeight.SemiBold, size: 12),
                        Colour = Color4.Black
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                EndDate.BindValueChanged(_ => updateDisplay());
                Status.BindValueChanged(_ => updateDisplay(), true);
            }

            private void updateDisplay()
            {
                RoomStatus status = EndDate.Value < DateTimeOffset.Now ? new RoomStatusEnded() : Status.Value ?? new RoomStatusOpen();

                background.FadeColour(status.GetAppropriateColour(colours), 100);
                statusText.Text = status.Message;
            }
        }
    }
}
