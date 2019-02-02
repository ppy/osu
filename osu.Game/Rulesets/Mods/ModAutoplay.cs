// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Replays;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModAutoplay<T> : ModAutoplay, IApplicableToRulesetContainer<T>
        where T : HitObject
    {
        protected virtual Score CreateReplayScore(Beatmap<T> beatmap) => new Score { Replay = new Replay() };

        public override bool HasImplementation => GetType().GenericTypeArguments.Length == 0;

        public virtual void ApplyToRulesetContainer(RulesetContainer<T> rulesetContainer) => rulesetContainer.SetReplayScore(CreateReplayScore(rulesetContainer.Beatmap));
    }

    public abstract class ModAutoplay : Mod, IApplicableFailOverride
    {
        public override string Name => "Autoplay";
        public override string Acronym => "AT";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_auto;
        public override ModType Type => ModType.Automation;
        public override string Description => "Watch a perfect automated play through the song.";
        public override double ScoreMultiplier => 1;
        public bool AllowFail => false;
        public override Type[] IncompatibleMods => new[] { typeof(ModRelax), typeof(ModSuddenDeath), typeof(ModNoFail) };
    }
}
