// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Replays;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Users;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModAutoplay : ModAutoplay<CatchHitObject>
    {
        protected override Score CreateReplayScore(Beatmap<CatchHitObject> beatmap)
        {
            return new Score
            {
                User = new User { Username = "osu!salad!" },
                Replay = new CatchAutoGenerator(beatmap).Generate(),
            };
        }
    }
}
