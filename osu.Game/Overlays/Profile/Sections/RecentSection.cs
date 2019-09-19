// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
