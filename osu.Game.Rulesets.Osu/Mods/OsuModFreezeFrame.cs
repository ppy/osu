// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.ComponentModel;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModFreezeFrame : ModWithVisibilityAdjustment, IApplicableToDrawableRuleset<OsuHitObject>
    {
        public override string Name => "Freeze Frame";

        public override string Acronym => "FR";

        public override double ScoreMultiplier => 1;

        public override LocalisableString Description => "Burn the notes into your memory.";

        public override ModType Type => ModType.Fun;

        [SettingSource("Beat Divisor", "How often the hitobjects should be grouped according to BPM")]
        public Bindable<BeatDivisor> Divisor { get; } = new Bindable<BeatDivisor>(BeatDivisor.Single_Measure);

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            (drawableRuleset.Playfield as OsuPlayfield)?.FollowPoints.Hide();
        }

        public override void ApplyToBeatmap(IBeatmap beatmap)
        {
            base.ApplyToBeatmap(beatmap);

            foreach (var obj in beatmap.HitObjects.OfType<OsuHitObject>())
            {
                // The +1s below are added due to First HitCircle in each measure not appearing appropriately without them.
                var lastTimingPoint = beatmap.ControlPointInfo.TimingPointAt(obj.StartTime + 1);
                double controlPointDifference = obj.StartTime + 1 - lastTimingPoint.Time;
                double remainder = controlPointDifference % (lastTimingPoint.BeatLength * getMeasure(Divisor.Value)) - 1;
                double finalPreempt = obj.TimePreempt + remainder;
                applyFadeInAdjustment(obj);

                void applyFadeInAdjustment(OsuHitObject osuObject)
                {
                    osuObject.TimePreempt = finalPreempt;
                    foreach (var nested in osuObject.NestedHitObjects.OfType<OsuHitObject>())
                        applyFadeInAdjustment(nested);
                }
            }
        }

        protected override void ApplyIncreasedVisibilityState(DrawableHitObject hitObject, ArmedState state) { }

        protected override void ApplyNormalVisibilityState(DrawableHitObject hitObject, ArmedState state) { }

        private float getMeasure(BeatDivisor divisor)
        {
            switch (divisor)
            {
                case BeatDivisor.Quarter_Measure:
                    return 0.25f;

                case BeatDivisor.Half_Measure:
                    return 0.5f;

                case BeatDivisor.Single_Measure:
                    return 1;

                case BeatDivisor.Double_Measure:
                    return 2;

                case BeatDivisor.Quadruple_Measure:
                    return 4;

                default:
                    throw new ArgumentOutOfRangeException(nameof(divisor), divisor, null);
            }
        }

        public enum BeatDivisor
        {
            [Description("1/4")]
            Quarter_Measure,

            [Description("1/2")]
            Half_Measure,

            [Description("1")]
            Single_Measure,

            [Description("2")]
            Double_Measure,

            [Description("4")]
            Quadruple_Measure
        }
    }
}
