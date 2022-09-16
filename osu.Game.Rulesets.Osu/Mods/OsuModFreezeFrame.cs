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
        public override string Name => "Freeze frame";

        public override string Acronym => "FF";

        public override double ScoreMultiplier => 1;

        public override LocalisableString Description => "Burn the notes into your memory";

        public override ModType Type => ModType.Fun;

        public override IconUsage? Icon => FontAwesome.Solid.Camera;
        public override Type[] IncompatibleMods => new[] { typeof(OsuModTarget), typeof(OsuModStrictTracking) };

        [SettingSource("Beat divisor")]
        public Bindable<BeatDivisor> Divisor { get; } = new Bindable<BeatDivisor>(BeatDivisor.Measure);

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            (drawableRuleset.Playfield as OsuPlayfield)?.FollowPoints.Hide();
        }

        public override void ApplyToBeatmap(IBeatmap beatmap)
        {
            base.ApplyToBeatmap(beatmap);

            foreach (var obj in beatmap.HitObjects.OfType<OsuHitObject>())
            {
                var lastTimingPoint = beatmap.ControlPointInfo.TimingPointAt(obj.StartTime + 1);
                // +1 is added due to First HitCircle in each measure not appearing appropriately without it
                double controlPointDifference = obj.StartTime + 1 - lastTimingPoint.Time;
                double remainder = controlPointDifference % (lastTimingPoint.BeatLength * getMeasure(Divisor.Value));

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

                case BeatDivisor.Measure:
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
            Quarter_Measure,
            Half_Measure,
            Measure,
            Double_Measure,
            Quadruple_Measure
        }
    }
}

