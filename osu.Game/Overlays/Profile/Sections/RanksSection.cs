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
                best = new ScoreContainer(ScoreType.Best, User, "Best Performance", true),
                first = new ScoreContainer(ScoreType.Firsts, User, "First Place Ranks"),
            };
        }
    }
}
