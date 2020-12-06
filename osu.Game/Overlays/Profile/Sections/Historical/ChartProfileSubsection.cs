// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Users;
using static osu.Game.Users.User;

namespace osu.Game.Overlays.Profile.Sections.Historical
{
    public abstract class ChartProfileSubsection : ProfileSubsection
    {
        private ProfileLineChart chart;

        protected ChartProfileSubsection(Bindable<User> user, string headerText)
            : base(user, headerText)
        {
        }

        protected override Drawable CreateContent() => new Container
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Padding = new MarginPadding
            {
                Top = 10,
                Left = 20,
                Right = 40
            },
            Child = chart = new ProfileLineChart()
        };

        protected override void LoadComplete()
        {
            base.LoadComplete();
            User.BindValueChanged(onUserChanged, true);
        }

        private void onUserChanged(ValueChangedEvent<User> e)
        {
            var values = GetValues(e.NewValue);

            if (values == null || values.Length <= 1)
            {
                Hide();
                return;
            }

            chart.Values = fillZeroValues(values);
            Show();
        }

        /// <summary>
        /// Add entries for any missing months (filled with zero values).
        /// </summary>
        private UserHistoryCount[] fillZeroValues(UserHistoryCount[] historyEntries)
        {
            var filledHistoryEntries = new List<UserHistoryCount>();

            foreach (var entry in historyEntries)
            {
                var lastFilled = filledHistoryEntries.LastOrDefault();

                while (lastFilled?.Date.AddMonths(1) < entry.Date)
                {
                    filledHistoryEntries.Add(lastFilled = new UserHistoryCount
                    {
                        Count = 0,
                        Date = lastFilled.Date.AddMonths(1)
                    });
                }

                filledHistoryEntries.Add(entry);
            }

            return filledHistoryEntries.ToArray();
        }

        protected abstract UserHistoryCount[] GetValues(User user);
    }
}
