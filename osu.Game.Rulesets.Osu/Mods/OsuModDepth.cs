// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModDepth : ModWithVisibilityAdjustment, IUpdatableByPlayfield, IApplicableToDrawableRuleset<OsuHitObject>
    {
        public override string Name => "Depth";
        public override string Acronym => "DH";
        public override IconUsage? Icon => FontAwesome.Solid.Cube;
        public override ModType Type => ModType.Fun;
        public override LocalisableString Description => "3D. Almost.";
        public override double ScoreMultiplier => 1;
        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(OsuModMagnetised), typeof(OsuModRepel), typeof(OsuModFreezeFrame), typeof(ModWithVisibilityAdjustment) }).ToArray();

        private static readonly Vector3 camera_position = new Vector3(OsuPlayfield.BASE_SIZE.X * 0.5f, OsuPlayfield.BASE_SIZE.Y * 0.5f, -100);

        [SettingSource("Maximum depth", "How far away objects appear.", 0)]
        public BindableFloat MaxDepth { get; } = new BindableFloat(100)
        {
            Precision = 10,
            MinValue = 50,
            MaxValue = 200
        };

        [SettingSource("Show Approach Circles", "Whether approach circles should be visible.", 1)]
        public BindableBool ShowApproachCircles { get; } = new BindableBool(true);

        protected override void ApplyIncreasedVisibilityState(DrawableHitObject hitObject, ArmedState state) => applyTransform(hitObject, state);

        protected override void ApplyNormalVisibilityState(DrawableHitObject hitObject, ArmedState state) => applyTransform(hitObject, state);

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            // Hide judgment displays and follow points as they won't make any sense.
            // Judgements can potentially be turned on in a future where they display at a position relative to their drawable counterpart.
            drawableRuleset.Playfield.DisplayJudgements.Value = false;
            (drawableRuleset.Playfield as OsuPlayfield)?.FollowPoints.Hide();
        }

        public override void ApplyToDrawableHitObject(DrawableHitObject dho)
        {
            base.ApplyToDrawableHitObject(dho);

            switch (dho)
            {
                case DrawableSliderHead:
                case DrawableSliderTail:
                case DrawableSliderTick:
                case DrawableSliderRepeat:
                    return;

                case DrawableHitCircle:
                case DrawableSlider:
                    dho.Anchor = Anchor.Centre;
                    break;
            }
        }

        private void applyTransform(DrawableHitObject drawable, ArmedState state)
        {
            switch (drawable)
            {
                case DrawableSliderHead head:
                    if (!ShowApproachCircles.Value)
                    {
                        var hitObject = (OsuHitObject)drawable.HitObject;
                        double appearTime = hitObject.StartTime - hitObject.TimePreempt;

                        using (head.BeginAbsoluteSequence(appearTime))
                            head.ApproachCircle.Hide();
                    }

                    break;

                case DrawableSliderTail:
                case DrawableSliderTick:
                case DrawableSliderRepeat:
                    return;

                case DrawableHitCircle circle:

                    if (!ShowApproachCircles.Value)
                    {
                        var hitObject = (OsuHitObject)drawable.HitObject;
                        double appearTime = hitObject.StartTime - hitObject.TimePreempt;

                        using (circle.BeginAbsoluteSequence(appearTime))
                            circle.ApproachCircle.Hide();
                    }

                    setStartPosition(drawable);
                    break;

                case DrawableSlider:
                    setStartPosition(drawable);
                    break;
            }
        }

        private void setStartPosition(DrawableHitObject drawable)
        {
            var hitObject = (OsuHitObject)drawable.HitObject;

            float d = mappedDepth(MaxDepth.Value);

            using (drawable.BeginAbsoluteSequence(hitObject.StartTime - hitObject.TimePreempt))
            {
                drawable.MoveTo(positionAtDepth(d, hitObject.Position));
                drawable.ScaleTo(new Vector2(d));
            }
        }

        public void Update(Playfield playfield)
        {
            double time = playfield.Time.Current;

            foreach (var drawable in playfield.HitObjectContainer.AliveObjects)
            {
                switch (drawable)
                {
                    case DrawableSliderHead:
                    case DrawableSliderTail:
                    case DrawableSliderTick:
                    case DrawableSliderRepeat:
                        continue;

                    case DrawableHitCircle:
                    case DrawableSlider:
                        var hitObject = (OsuHitObject)drawable.HitObject;

                        double appearTime = hitObject.StartTime - hitObject.TimePreempt;
                        double moveDuration = hitObject.TimePreempt;
                        float z = time > appearTime + moveDuration ? 0 : (MaxDepth.Value - (float)((time - appearTime) / moveDuration * MaxDepth.Value));

                        float d = mappedDepth(z);
                        drawable.Position = positionAtDepth(d, hitObject.Position);
                        drawable.Scale = new Vector2(d);
                        break;
                }
            }
        }

        private static float mappedDepth(float depth) => 100 / (depth - camera_position.Z);

        private static Vector2 positionAtDepth(float mappedDepth, Vector2 positionAtZeroDepth)
        {
            return (positionAtZeroDepth - camera_position.Xy) * mappedDepth;
        }
    }
}
