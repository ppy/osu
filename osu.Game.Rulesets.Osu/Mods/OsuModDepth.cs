// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
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
        public override string Acronym => "DP";
        public override IconUsage? Icon => FontAwesome.Solid.Cube;
        public override ModType Type => ModType.Fun;
        public override LocalisableString Description => "3D. Almost.";
        public override double ScoreMultiplier => 1;
        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(OsuModMagnetised), typeof(OsuModRepel), typeof(OsuModFreezeFrame), typeof(ModWithVisibilityAdjustment) }).ToArray();

        private static readonly Vector3 camera_position = new Vector3(OsuPlayfield.BASE_SIZE.X * 0.5f, OsuPlayfield.BASE_SIZE.Y * 0.5f, -200);
        private readonly float sliderMinDepth = depthForScale(1.5f); // Depth at which slider's scale will be 1.5f

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
            // Hide follow points as they won't make any sense.
            // Judgements can potentially be turned on in a future where they display at a position relative to their drawable counterpart.
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

            foreach (var entry in playfield.HitObjectContainer.AliveEntries)
            {
                var drawable = entry.Value;

                switch (drawable)
                {
                    case DrawableHitCircle circle:
                        processHitObject(time, circle);
                        break;

                    case DrawableSlider slider:
                        processSlider(time, slider);
                        break;
                }
            }
        }

        private void processHitObject(double time, DrawableOsuHitObject drawable)
        {
            var hitObject = drawable.HitObject;

            // Circles are always moving at the constant speed. They'll fade out before reaching the camera even at extreme conditions (AR 11, max depth).
            double speed = MaxDepth.Value / hitObject.TimePreempt;
            double appearTime = hitObject.StartTime - hitObject.TimePreempt;
            float z = MaxDepth.Value - (float)((Math.Max(time, appearTime) - appearTime) * speed);

            float scale = scaleForDepth(z);
            drawable.Position = toPlayfieldPosition(scale, hitObject.StackedPosition);
            drawable.Scale = new Vector2(scale);
        }

        private void processSlider(double time, DrawableSlider drawableSlider)
        {
            var hitObject = drawableSlider.HitObject;

            double baseSpeed = MaxDepth.Value / hitObject.TimePreempt;
            double appearTime = hitObject.StartTime - hitObject.TimePreempt;

            // Allow slider to move at a constant speed if its scale at the end time will be lower than 1.5f
            float zEnd = MaxDepth.Value - (float)((Math.Max(hitObject.StartTime + hitObject.Duration, appearTime) - appearTime) * baseSpeed);

            if (zEnd > sliderMinDepth)
            {
                processHitObject(time, drawableSlider);
                return;
            }

            double offsetAfterStartTime = hitObject.Duration + 500;
            double slowSpeed = Math.Min(-sliderMinDepth / offsetAfterStartTime, baseSpeed);

            double decelerationTime = hitObject.TimePreempt * 0.2;
            float decelerationDistance = (float)(decelerationTime * (baseSpeed + slowSpeed) * 0.5);

            float z;

            if (time < hitObject.StartTime - decelerationTime)
            {
                float fullDistance = decelerationDistance + (float)(baseSpeed * (hitObject.TimePreempt - decelerationTime));
                z = fullDistance - (float)((Math.Max(time, appearTime) - appearTime) * baseSpeed);
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
                z = -(float)((Math.Min(time, endTime) - hitObject.StartTime) * slowSpeed);
            }

            float scale = scaleForDepth(z);
            drawableSlider.Position = toPlayfieldPosition(scale, hitObject.StackedPosition);
            drawableSlider.Scale = new Vector2(scale);
        }

        private static float scaleForDepth(float depth) => -camera_position.Z / Math.Max(1f, depth - camera_position.Z);

        private static float depthForScale(float scale) => -camera_position.Z / scale + camera_position.Z;

        private static Vector2 toPlayfieldPosition(float scale, Vector2 positionAtZeroDepth)
        {
            return (positionAtZeroDepth - camera_position.Xy) * scale + camera_position.Xy;
        }
    }
}
