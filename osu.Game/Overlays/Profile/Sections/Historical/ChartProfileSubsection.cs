// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
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

            if (values?.Length > 1)
            {
                chart.Values = fillZeroValues(values);
                Show();
                return;
            }

            Hide();
        }

        private UserHistoryCount[] fillZeroValues(UserHistoryCount[] values)
        {
            var newValues = new List<UserHistoryCount> { values[0] };
            var newLast = values[0];

            for (int i = 1; i < values.Length; i++)
            {
                while (hasMissingDates(newLast, values[i]))
                {
                    newValues.Add(newLast = new UserHistoryCount
                    {
                        Count = 0,
                        Date = newLast.Date.AddMonths(1)
                    });
                }

                newValues.Add(newLast = values[i]);
            }

            return newValues.ToArray();

            static bool hasMissingDates(UserHistoryCount prev, UserHistoryCount current)
            {
                var possibleCurrent = prev.Date.AddMonths(1);
                return possibleCurrent < current.Date;
            }
        }

        protected abstract UserHistoryCount[] GetValues(User user);
    }
}
