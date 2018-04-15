// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModAutoplay : ModAutoplay<OsuHitObject>
    {
        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(OsuModAutopilot)).Append(typeof(OsuModSpunOut)).ToArray();

        protected override Score CreateReplayScore(Beatmap<OsuHitObject> beatmap)
        {
            return new Score
            {
                Replay = new OsuAutoGenerator(beatmap).Generate()
            };
        }
    }
}
