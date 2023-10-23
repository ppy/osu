// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Localisation;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.Mods
{
    public class ModNoStop : Mod, IApplicableToPlayerConfiguration
    {
        public override string Name => "No Stop";
        public override string Acronym => "NT";
        public override LocalisableString Description => "Start and no stop";
        public override double ScoreMultiplier => 1;
        public override ModType Type => ModType.DifficultyIncrease;
        public override Type[] IncompatibleMods => new[] { typeof(ModAutoplay) };

        public void ApplyConfiguration(PlayerConfiguration configuration)
        {
            configuration.AllowPause = false;
        }
    }
}
