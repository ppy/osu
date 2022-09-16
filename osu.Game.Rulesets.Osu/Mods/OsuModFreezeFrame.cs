using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
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
    public class OsuModFreezeFrame : ModWithVisibilityAdjustment, IHidesApproachCircles, IApplicableToDrawableRuleset<OsuHitObject>
    {
        public override string Name => "Freeze Frame";

        public override string Acronym => "FF";

        public override double ScoreMultiplier => 1;

        public override LocalisableString Description => "Burn the notes into your memory";

        public override ModType Type => ModType.Fun;

        public override IconUsage? Icon => FontAwesome.Solid.Camera;

        [SettingSource("Measure", "How often the hitcircles should be Grouped to freeze")]
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

        //Todo: find better way to represent these Enums to the player
        public enum BeatDivisor
        {
            Quarter_Measure,
            Half_Measure,
            Single_Measure,
            Double_Measure,
            Quadruple_Measure
        }
    }
}

