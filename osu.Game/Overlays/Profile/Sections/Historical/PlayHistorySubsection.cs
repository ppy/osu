// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Users;
using static osu.Game.Users.User;

namespace osu.Game.Overlays.Profile.Sections.Historical
{
    public class PlayHistorySubsection : ChartProfileSubsection
    {
        public PlayHistorySubsection(Bindable<User> user)
            : base(user, "Play History")
        {
        }

        protected override UserHistoryCount[] GetValues(User user) => user?.MonthlyPlaycounts;
    }
}
