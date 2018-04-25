// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mods
{
    public class ModAutoplay<T> : ModAutoplay, IApplicableToRulesetContainer<T>
        where T : HitObject
    {
        protected virtual Score CreateReplayScore(Beatmap<T> beatmap) => new Score { Replay = new Replay() };

        public override bool HasImplementation => GetType().GenericTypeArguments.Length == 0;

        public virtual void ApplyToRulesetContainer(RulesetContainer<T> rulesetContainer) => rulesetContainer.SetReplay(CreateReplayScore(rulesetContainer.Beatmap)?.Replay);
    }

    public abstract class ModAutoplay : Mod, IApplicableFailOverride
    {
        public override string Name => "Autoplay";
        public override string ShortenedName => "AT";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_auto;
        public override string Description => "Watch a perfect automated play through the song.";
        public override double ScoreMultiplier => 0;
        public bool AllowFail => false;
        public override Type[] IncompatibleMods => new[] { typeof(ModRelax), typeof(ModSuddenDeath), typeof(ModNoFail) };
    }
}
