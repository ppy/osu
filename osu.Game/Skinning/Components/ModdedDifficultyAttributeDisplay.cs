// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Localisation.SkinComponents;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Utils;

namespace osu.Game.Skinning.Components
{
    public partial class ModdedDifficultyAttributeDisplay : ModdedAttributeDisplay
    {
        [SettingSource(typeof(BeatmapAttributeTextStrings), nameof(BeatmapAttributeTextStrings.Attribute), nameof(BeatmapAttributeTextStrings.AttributeDescription))]
        public Bindable<BeatmapDifficultyAttribute> Attribute { get; } = new();

        [SettingSource("Account for Rate", "Approach Rate and Accuracy will use effective value, accounting for rate-changing mods.")]
        public Bindable<bool> AccountForRate { get; } = new();

        private static readonly ImmutableDictionary<BeatmapDifficultyAttribute, LocalisableString> label_dictionary = new Dictionary<BeatmapDifficultyAttribute, LocalisableString>
        {
            [BeatmapDifficultyAttribute.CircleSize] = BeatmapsetsStrings.ShowStatsCs,
            [BeatmapDifficultyAttribute.Accuracy] = BeatmapsetsStrings.ShowStatsAccuracy,
            [BeatmapDifficultyAttribute.HPDrain] = BeatmapsetsStrings.ShowStatsDrain,
            [BeatmapDifficultyAttribute.ApproachRate] = BeatmapsetsStrings.ShowStatsAr,
        }.ToImmutableDictionary();

        protected override LocalisableString AttributeLabel => label_dictionary[Attribute.Value];

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Attribute.BindValueChanged(_ => UpdateValue());
        }
        protected override void UpdateValue()
        {
            BeatmapDifficulty difficulty = new BeatmapDifficulty(BeatmapInfo.Difficulty);

            foreach (var mod in Mods.Value.OfType<IApplicableToDifficulty>())
                mod.ApplyToDifficulty(difficulty);

            if (AccountForRate.Value)
            {
                double rate = ModUtils.CalculateRateWithMods(Mods.Value);
                Ruleset ruleset = Ruleset.Value.CreateInstance();
                difficulty = ruleset.GetRateAdjustedDisplayDifficulty(difficulty, rate);
            }

            Current.Value = (Attribute.Value switch
            {
                BeatmapDifficultyAttribute.CircleSize => difficulty.CircleSize,
                BeatmapDifficultyAttribute.HPDrain => difficulty.DrainRate,
                BeatmapDifficultyAttribute.Accuracy => difficulty.OverallDifficulty,
                BeatmapDifficultyAttribute.ApproachRate => difficulty.ApproachRate,
                _ => 0,
            }).ToLocalisableString(@"F2");
        }
    }

    public enum BeatmapDifficultyAttribute
    {
        CircleSize,
        HPDrain,
        Accuracy,
        ApproachRate
    }
}
