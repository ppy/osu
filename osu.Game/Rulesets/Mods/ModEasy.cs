// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Beatmaps;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModEasy : Mod, IApplicableToDifficulty
    {
        public override string Name => "Easy";
        public override string ShortenedName => "EZ";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_easy;
        public override ModType Type => ModType.DifficultyReduction;
        public override double ScoreMultiplier => 0.5;
        public override bool Ranked => true;
        public override Type[] IncompatibleMods => new[] { typeof(ModHardRock) };

        public void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            const float ratio = 0.5f;
            difficulty.CircleSize *= ratio;
            difficulty.ApproachRate *= ratio;
            difficulty.DrainRate *= ratio;
            difficulty.OverallDifficulty *= ratio;
        }
    }
}
