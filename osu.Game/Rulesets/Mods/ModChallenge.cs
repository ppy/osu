// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModChallenge : Mod, IApplicableFailOverride
    {
        public override double ScoreMultiplier => 1.0;
        public override bool RequiresConfiguration => true;
        public override ModType Type => ModType.Challenge;

        public override Type[] IncompatibleMods => new[]
        {
            typeof(ModAutoplay),
            typeof(ModNoFail)
        };

        public bool PerformFail() => true;

        public bool RestartOnFail => false;
    }
}
