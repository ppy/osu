// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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

        protected override void OnUserChanged(ValueChangedEvent<User> e)
        {
            var values = GetValues(e.NewValue);

            if (values?.Length > 1)
            {
                chart.Values = values;
                Show();
                return;
            }

            Hide();
        }

        protected abstract UserHistoryCount[] GetValues(User user);
    }
}
