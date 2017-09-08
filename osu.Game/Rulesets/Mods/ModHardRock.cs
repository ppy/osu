// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Beatmaps;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModHardRock : Mod, IApplicableToDifficulty
    {
        public override string Name => "Hard Rock";
        public override string ShortenedName => "HR";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_hardrock;
        public override ModType Type => ModType.DifficultyIncrease;
        public override string Description => "Everything just got a bit harder...";
        public override Type[] IncompatibleMods => new[] { typeof(ModEasy) };

        public void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            const float ratio = 1.4f;
            difficulty.CircleSize *= 1.3f; // CS uses a custom 1.3 ratio.
            difficulty.ApproachRate *= ratio;
            difficulty.DrainRate *= ratio;
            difficulty.OverallDifficulty *= ratio;
        }
    }
}
