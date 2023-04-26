// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Profile.Sections.Historical;
using osu.Game.Overlays.Profile.Sections.Ranks;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Profile.Sections
{
    public partial class HistoricalSection : ProfileSection
    {
        public override LocalisableString Title => UsersStrings.ShowExtraHistoricalTitle;

        public override string Identifier => @"historical";

        public HistoricalSection()
        {
            Children = new Drawable[]
            {
                new PlayHistorySubsection(User),
                new PaginatedMostPlayedBeatmapContainer(User),
                new PaginatedScoreContainer(ScoreType.Recent, User, UsersStrings.ShowExtraHistoricalRecentPlaysTitle),
                new ReplaysSubsection(User)
            };
        }
    }
}
