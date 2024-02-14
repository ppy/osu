// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Users;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public partial class TotalPlayTime : CompositeDrawable, IHasTooltip
    {
        public readonly Bindable<UserStatistics?> UserStatistics = new Bindable<UserStatistics?>();

        public LocalisableString TooltipText { get; set; }

        private ProfileValueDisplay info = null!;

        public TotalPlayTime()
        {
            AutoSizeAxes = Axes.Both;

            TooltipText = "0 hours";
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = info = new ProfileValueDisplay(minimumWidth: 140)
            {
                Title = UsersStrings.ShowStatsPlayTime,
            };

            UserStatistics.BindValueChanged(updateTime, true);
        }

        private void updateTime(ValueChangedEvent<UserStatistics?> statistics)
        {
            int? playTime = statistics.NewValue?.PlayTime;
            TooltipText = (playTime ?? 0) / 3600 + " hours";
            info.Content = formatTime(playTime);
        }

        private string formatTime(int? secondsNull)
        {
            if (secondsNull == null) return "0h 0m";

            int seconds = secondsNull.Value;
            string time = "";

            int days = seconds / 86400;
            seconds -= days * 86400;
            if (days > 0)
                time += days + "d ";

            int hours = seconds / 3600;
            seconds -= hours * 3600;
            time += hours + "h ";

            int minutes = seconds / 60;
            time += minutes + "m";

            return time;
        }
    }
}
