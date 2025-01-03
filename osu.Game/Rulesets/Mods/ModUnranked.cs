// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mods
{
    public class ModUnranked : Mod, IApplicableToScoreProcessor
    {
        public override string Name => @"Unranked";
        public override string Acronym => @"UR";
        public override ModType Type => ModType.Conversion;
        public override LocalisableString Description => @"As the name suggests, makes this play unranked.";
        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(ModAutoplay), typeof(ModCinema) }).ToArray();
        public override double ScoreMultiplier => 1.0;
        public override bool UserPlayable => true;
        public override bool ValidForMultiplayer => true;
        public override bool ValidForMultiplayerAsFreeMod => true;

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            return;
        }

        public ScoreRank AdjustRank(ScoreRank rank, double accuracy) => rank;
    }
}
