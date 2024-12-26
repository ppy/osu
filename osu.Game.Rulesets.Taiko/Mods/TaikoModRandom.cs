// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Beatmaps;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModRandom : ModRandom, IApplicableToBeatmap
    {
        public override LocalisableString Description => @"Shuffle around the colours!";
        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(TaikoModSwap)).ToArray();

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            Seed.Value ??= RNG.Next();
            var rng = new Random((int)Seed.Value);

            var taikoBeatmap = (TaikoBeatmap)beatmap;
            taikoBeatmap.InvertTypes(_ => rng.Next(2) == 0);
        }
    }
}
