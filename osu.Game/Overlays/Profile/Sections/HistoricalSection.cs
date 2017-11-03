// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Profile.Sections.Ranks;

namespace osu.Game.Overlays.Profile.Sections
{
    public class HistoricalSection : ProfileSection
    {
        public override string Title => "Historical";

        public override string Identifier => "historical";

        public HistoricalSection()
        {
            Child = new PaginatedScoreContainer(ScoreType.Recent, User, "Recent Plays (24h)", "No performance records. :(");
        }
    }
}
