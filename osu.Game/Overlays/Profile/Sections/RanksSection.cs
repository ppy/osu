// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Overlays.Profile.Sections.Ranks;
using osu.Game.Online.API.Requests;
using osu.Game.Users;

namespace osu.Game.Overlays.Profile.Sections
{
    public class RanksSection : ProfileSection
    {
        public override string Title => "Ranks";

        public override string Identifier => "top_ranks";

        private readonly ScoreContainer best, first;

        public RanksSection()
        {
            Children = new Drawable[]
            {
                best = new ScoreContainer.PPScoreContainer(ScoreType.Best, "Best Performance", "No awesome performance records yet. :(", true),
                first = new ScoreContainer.PPScoreContainer(ScoreType.Firsts, "First Place Ranks", "No awesome performance records yet. :("),
            };
        }

        public override User User
        {
            get
            {
                return base.User;
            }

            set
            {
                base.User = value;
                best.User = value;
                first.User = value;
            }
        }
    }
}
