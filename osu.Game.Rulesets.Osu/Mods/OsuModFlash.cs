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
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModFlash : ModWithVisibilityAdjustment, IHidesApproachCircles, IApplicableToDrawableRuleset<OsuHitObject>
    {
        public override string Name => "Freeze frame";

        public override string Acronym => "FF";

        public override double ScoreMultiplier => 1;

        public override LocalisableString Description => "Burn the notes into your memory";

        public override ModType Type => ModType.Fun;

        public override IconUsage? Icon => FontAwesome.Solid.Fire;

        public override Type[] IncompatibleMods => new[] { typeof(OsuModTarget), typeof(OsuModStrictTracking) };

        [SettingSource("Beat divisor")]
        public BindableFloat BeatDivisor { get; } = new BindableFloat(1)
        {
            MinValue = .25f,
            MaxValue = 5,
            Precision = .25f
        };

        public override void ApplyToBeatmap(IBeatmap beatmap)
        {
            base.ApplyToBeatmap(beatmap);

            foreach (var obj in beatmap.HitObjects.OfType<OsuHitObject>())
            {
                applyFadeInAdjustment(obj);
                var point = beatmap.ControlPointInfo.TimingPointAt(obj.StartTime);
                obj.TimePreempt += obj.StartTime % (point.BeatLength * BeatDivisor.Value);
            }

            static void applyFadeInAdjustment(OsuHitObject osuObject)
            {
                osuObject.TimeFadeIn = osuObject.TimePreempt;
                foreach (var nested in osuObject.NestedHitObjects.OfType<OsuHitObject>())
                    applyFadeInAdjustment(nested);
            }
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            (drawableRuleset.Playfield as OsuPlayfield)?.FollowPoints.Hide();
        }

        protected override void ApplyIncreasedVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
        }

        protected override void ApplyNormalVisibilityState(DrawableHitObject hitObject, ArmedState state) => applyFrozenState(hitObject, state);

        private void applyFrozenState(DrawableHitObject drawable, ArmedState state)
        {
            if (drawable is DrawableSpinner)
                return;

            var h = (OsuHitObject)drawable.HitObject;

            switch (drawable)
            {
                case DrawableHitCircle circle:
                    using (circle.BeginAbsoluteSequence(h.StartTime - h.TimePreempt))
                    {
                        circle.ApproachCircle.Hide();
                    }

                    break;
            }
        }
    }
}

