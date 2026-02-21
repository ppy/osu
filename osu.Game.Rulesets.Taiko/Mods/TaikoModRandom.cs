// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Beatmaps;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModRandom : ModRandom, IApplicableToBeatmap
    {
        public override LocalisableString Description => @"Shuffle around the colours!";

        [SettingSource("Randomization Ratio", "Approximately how much of the beatmap should be randomized.", SettingControlType = typeof(SettingsPercentageSlider<double>))]
        public BindableNumber<double> RandomizationRatio { get; } = new BindableDouble(1)
        {
            MinValue = 0,
            MaxValue = 1,
            Precision = 0.01,
        };

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            var taikoBeatmap = (TaikoBeatmap)beatmap;

            Seed.Value ??= RNG.Next();
            var rng = new Random((int)Seed.Value);

            foreach (var obj in taikoBeatmap.HitObjects)
            {
                if (obj is Hit hit)
                {
                    // Complete (100%) randomization is a 50/50 chance of flipping.
                    double flipChance = RandomizationRatio.Value / 2;
                    if (rng.NextDouble() < flipChance)
                        hit.Type = hit.Type == HitType.Centre ? HitType.Rim : HitType.Centre;
                }
            }
        }
    }
}
