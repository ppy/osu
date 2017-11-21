// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Overlays.Profile.Sections.Ranks;
using osu.Game.Online.API.Requests;

namespace osu.Game.Overlays.Profile.Sections
{
    public class RanksSection : ProfileSection
    {
        public override string Title => "Ranks";

        public override string Identifier => "top_ranks";

        private readonly PaginatedScoreContainer bestScores;
        private readonly PaginatedScoreContainer firstsScores;

        public RanksSection()
        {
            Children = new[]
            {
                bestScores = new PaginatedScoreContainer(ScoreType.Best, User, "Best Performance", "No performance records. :(", true),
                firstsScores = new PaginatedScoreContainer(ScoreType.Firsts, User, "First Place Ranks", "No awesome performance records yet. :("),
            };
        }

        protected override void OnPlayModeChanged()
        {
            base.OnPlayModeChanged();

            bestScores.ApplyPlaymode(PlayMode);
            firstsScores.ApplyPlaymode(PlayMode);
        }
    }
}
