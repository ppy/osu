// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Profile.Sections.Historical;
using osu.Game.Overlays.Profile.Sections.Ranks;

namespace osu.Game.Overlays.Profile.Sections
{
    public class HistoricalSection : ProfileSection
    {
        public override string Title => "Historical";

        public override string Identifier => "historical";

        public HistoricalSection()
        {
            Children = new Drawable[]
            {
                new PlayHistorySubsection(User),
                new PaginatedMostPlayedBeatmapContainer(User),
                new PaginatedScoreContainer(ScoreType.Recent, User, "Recent Plays (24h)", CounterVisibilityState.VisibleWhenZero),
                new ReplaysSubsection(User)
            };
        }
    }
}
