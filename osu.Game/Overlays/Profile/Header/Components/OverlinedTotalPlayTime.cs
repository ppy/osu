// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Localisation;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public class OverlinedTotalPlayTime : CompositeDrawable, IHasTooltip
    {
        public readonly Bindable<APIUser> User = new Bindable<APIUser>();

        public LocalisableString TooltipText { get; set; }

        private OverlinedInfoContainer info;

        public OverlinedTotalPlayTime()
        {
            AutoSizeAxes = Axes.Both;

            TooltipText = "0 hours";
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            InternalChild = info = new OverlinedInfoContainer
            {
                Title = UsersStrings.ShowStatsPlayTime,
                LineColour = colourProvider.Highlight1,
            };

            User.BindValueChanged(updateTime, true);
        }

        private void updateTime(ValueChangedEvent<APIUser> user)
        {
            TooltipText = (user.NewValue?.Statistics?.PlayTime ?? 0) / 3600 + " hours";
            info.Content = formatTime(user.NewValue?.Statistics?.PlayTime);
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
