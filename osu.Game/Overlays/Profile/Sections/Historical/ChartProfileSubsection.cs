// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Overlays.Profile.Sections.Historical
{
    public abstract partial class ChartProfileSubsection : ProfileSubsection
    {
        private ProfileLineChart chart = null!;

        /// <summary>
        /// Text describing the value being plotted on the graph, which will be displayed as a prefix to the value in the history graph tooltip.
        /// </summary>
        protected abstract LocalisableString GraphCounterName { get; }

        protected ChartProfileSubsection(Bindable<UserProfileData?> user, LocalisableString headerText)
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
            Child = chart = new ProfileLineChart(GraphCounterName)
        };

        protected override void LoadComplete()
        {
            base.LoadComplete();
            User.BindValueChanged(onUserChanged, true);
        }

        private void onUserChanged(ValueChangedEvent<UserProfileData?> e)
        {
            var values = GetValues(e.NewValue?.User);

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
        private APIUserHistoryCount[] fillZeroValues(APIUserHistoryCount[] historyEntries)
        {
            var filledHistoryEntries = new List<APIUserHistoryCount>();

            foreach (var entry in historyEntries)
            {
                var lastFilled = filledHistoryEntries.LastOrDefault();

                while (lastFilled?.Date.AddMonths(1) < entry.Date)
                {
                    filledHistoryEntries.Add(lastFilled = new APIUserHistoryCount
                    {
                        Count = 0,
                        Date = lastFilled.Date.AddMonths(1)
                    });
                }

                filledHistoryEntries.Add(entry);
            }

            return filledHistoryEntries.ToArray();
        }

        protected abstract APIUserHistoryCount[]? GetValues(APIUser? user);
    }
}
