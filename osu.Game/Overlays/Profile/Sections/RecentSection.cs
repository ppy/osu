// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Overlays.Profile.Sections.Recent;

namespace osu.Game.Overlays.Profile.Sections
{
    public class RecentSection : ProfileSection
    {
        public override string Title => "Recent";

        public override string Identifier => "recent_activity";

        public RecentSection()
        {
            Children = new[]
            {
                new PaginatedRecentActivityContainer(User, null, @"This user hasn't done anything notable recently!"),
            };
        }
    }
}
