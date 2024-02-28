// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Graphics;

namespace osu.Game.Screens.OnlinePlay.Lounge.Components
{
    public partial class EndDateInfo : OnlinePlayComposite
    {
        public EndDateInfo()
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new EndDatePart
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Font = OsuFont.GetFont(weight: FontWeight.SemiBold, size: 12),
                EndDate = { BindTarget = EndDate }
            };
        }

        private partial class EndDatePart : DrawableDate
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
    }
}
