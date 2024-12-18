// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Overlays.Profile.Sections.Recent;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Profile.Sections
{
    public partial class RecentSection : ProfileSection
    {
        public override LocalisableString Title => UsersStrings.ShowExtraRecentActivityTitle;

        public override string Identifier => @"recent_activity";

        public RecentSection()
        {
            Children = new[]
            {
                new PaginatedRecentActivityContainer(User),
            };
        }
    }
}
