// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Utils;
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
        private readonly float minDepth = depthForScale(1.5f);

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

        private void applyTransform(DrawableHitObject drawable, ArmedState state)
        {
            switch (drawable)
            {
                case DrawableHitCircle circle:
                    if (!ShowApproachCircles.Value)
                    {
                        var hitObject = (OsuHitObject)drawable.HitObject;
                        double appearTime = hitObject.StartTime - hitObject.TimePreempt;

                        using (circle.BeginAbsoluteSequence(appearTime))
                            circle.ApproachCircle.Hide();
                    }

                    break;
            }
        }

        public void Update(Playfield playfield)
        {
            double time = playfield.Time.Current;

            foreach (var drawable in playfield.HitObjectContainer.AliveObjects)
            {
                if (drawable is not DrawableOsuHitObject d)
                    continue;

                switch (d)
                {
                    case DrawableHitCircle:
                    case DrawableSlider:
                        processObject(time, d);
                        break;
                }
            }
        }

        private void processObject(double time, DrawableOsuHitObject drawable)
        {
            var hitObject = drawable.HitObject;

            double baseSpeed = MaxDepth.Value / hitObject.TimePreempt;
            double hitObjectDuration = hitObject is Slider s ? s.Duration : 0.0;
            double offsetAfterStartTime = hitObjectDuration + hitObject.MaximumJudgementOffset + 500;
            double slowSpeed = -minDepth / offsetAfterStartTime;

            float decelerationDistance = MaxDepth.Value * 0.2f;
            double decelerationTime = (slowSpeed - baseSpeed) * 2 * decelerationDistance / (slowSpeed * slowSpeed - baseSpeed * baseSpeed);

            float z;

            if (time < hitObject.StartTime - decelerationTime)
            {
                double appearTime = hitObject.StartTime - hitObject.TimePreempt;
                float fullDistance = decelerationDistance + (float)(baseSpeed * (hitObject.TimePreempt - decelerationTime));
                z = Interpolation.ValueAt(Math.Max(time, appearTime), fullDistance, decelerationDistance, appearTime, hitObject.StartTime - decelerationTime);
            }
            else if (time < hitObject.StartTime)
            {
                double timeOffset = time - (hitObject.StartTime - decelerationTime);
                double deceleration = (slowSpeed - baseSpeed) / decelerationTime;
                z = decelerationDistance - (float)(baseSpeed * timeOffset + deceleration * timeOffset * timeOffset * 0.5);
            }
            else
            {
                double endTime = hitObject.StartTime + offsetAfterStartTime;
                z = Interpolation.ValueAt(Math.Min(time, endTime), 0f, minDepth, hitObject.StartTime, endTime);
            }

            float scale = scaleForDepth(z);
            drawable.Position = positionAtDepth(scale, hitObject.Position);
            drawable.Scale = new Vector2(scale);
        }

        private static float scaleForDepth(float depth) => 100 / (depth - camera_position.Z);

        private static float depthForScale(float scale) => 100 / scale + camera_position.Z;

        private static Vector2 positionAtDepth(float scale, Vector2 positionAtZeroDepth)
        {
            return (positionAtZeroDepth - camera_position.Xy) * scale + camera_position.Xy;
        }
    }
}
