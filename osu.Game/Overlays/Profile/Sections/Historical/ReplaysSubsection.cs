// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Users;
using static osu.Game.Users.User;

namespace osu.Game.Overlays.Profile.Sections.Historical
{
    public class ReplaysSubsection : ChartProfileSubsection
    {
        protected override LocalisableString GraphCounterName => UsersStrings.ShowExtraHistoricalReplaysWatchedCountsCountLabel;

        public ReplaysSubsection(Bindable<User> user)
            : base(user, UsersStrings.ShowExtraHistoricalReplaysWatchedCountsTitle)
        {
        }

        protected override UserHistoryCount[] GetValues(User user) => user?.ReplaysWatchedCounts;
    }
}
