// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Replays;
using osu.Game.Users;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModAutoplay : ModAutoplay<TaikoHitObject>
    {
        protected override Score CreateReplayScore(Beatmap<TaikoHitObject> beatmap)
        {
            return new Score
            {
                User = new User { Username = "mekkadosu!" },
                Replay = new TaikoAutoGenerator(beatmap).Generate(),
            };
        }
    }
}
