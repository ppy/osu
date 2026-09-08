// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Legacy;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModCrumblingCircles : Mod, IApplicableToHitObject, IApplicableToBeatmapProcessor, IApplicableToDifficulty
    {
        public override string Name => "Crumbling Circles";
        public override string Acronym => "CC";
        public override LocalisableString Description => "The more you play, the smaller the circles get!";
        public override ModType Type => ModType.Fun;
        public override double ScoreMultiplier => 1d;
        public override bool Ranked => false;

        private const float target_size_precision_f = 0.1f;

        private float initialCircleSize;
        private int initialObjectCount;

        private float? currentCircleSize =>
            (initialCircleSize - TargetCircleSize.Value) / initialObjectCount * hitObjectCount + TargetCircleSize.Value;

        private int hitObjectCount;

        [SettingSource("Target Circle Size", "The size of the circles at the end of the map.", SettingControlType = typeof(DifficultyAdjustSettingsControl))]
        public DifficultyBindable TargetCircleSize { get; set; } = new DifficultyBindable(7)
        {
            Precision = target_size_precision_f,
            MinValue = 0,
            MaxValue = 10,
            ExtendedMaxValue = 11,
        };

        [SettingSource("Extended Limits", "Adjust target size beyond sane limits.")]
        public BindableBool ExtendedLimits { get; } = new BindableBool();

        public OsuModCrumblingCircles()
        {
            TargetCircleSize.ExtendedLimits.BindTo(ExtendedLimits);
        }

        public void ApplyToHitObject(HitObject hitObject)
        {
            var osuObject = (OsuHitObject)hitObject;

            // Spinners don't need to have a size change
            if (osuObject is not Spinner)
                applyCurrentCircleSize(osuObject);

            hitObjectCount--;
        }

        public void ApplyToBeatmapProcessor(IBeatmapProcessor beatmapProcessor)
        {
            // We use the Beatmap Processor to populate these values before the hit object changes.
            initialObjectCount = hitObjectCount = beatmapProcessor.Beatmap.HitObjects.Count - 1;
            initialCircleSize = beatmapProcessor.Beatmap.Difficulty.CircleSize;
        }

        private void applyCurrentCircleSize(OsuHitObject osuObject)
        {
            osuObject.Scale = LegacyRulesetExtensions.CalculateScaleFromCircleSize(currentCircleSize ?? initialCircleSize, true);
            osuObject.NestedHitObjects.ForEach(o => applyCurrentCircleSize((OsuHitObject)o));
        }

        public void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            // Ensure we do not overflow the max value in case circle size is being adjusted to 10 by another mod.
            if (difficulty.CircleSize > 9.8f)
                return;

            // Set the possible target value range based on current circle size diff.
            TargetCircleSize.MinValue = difficulty.CircleSize + target_size_precision_f;
        }
    }
}
