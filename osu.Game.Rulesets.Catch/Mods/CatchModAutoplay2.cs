// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Replays;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Users;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModAutoplay2 : ModAutoplay<CatchHitObject>
    {
        public override string Name => "Autoplay2";
        public override string ShortenedName => "A2";
        public override Type[] IncompatibleMods => new[] { typeof(CatchModRelax), typeof(CatchModSuddenDeath), typeof(CatchModNoFail), typeof(CatchModAutoplay) };

        protected override Score CreateReplayScore(Beatmap<CatchHitObject> beatmap)
        {
            return new Score
            {
                User = new User { Username = "osu!salad!" },
                Replay = new CatchAutoGenerator2(beatmap).Generate(),
            };
        }

        public override void ApplyToRulesetContainer(RulesetContainer<CatchHitObject> rulesetContainer)
        {
            if (rulesetContainer is CatchRulesetContainer)
                ((CatchRulesetContainer)rulesetContainer).AutoMod = true;
            base.ApplyToRulesetContainer(rulesetContainer);
        }
    }
}
