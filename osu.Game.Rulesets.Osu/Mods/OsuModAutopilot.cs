// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.StateChanges;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModAutopilot : Mod, IUpdatableByPlayfield, IApplicableToDrawableRuleset<OsuHitObject>
    {
        public override string Name => "Autopilot";
        public override string Acronym => "AP";
        public override IconUsage? Icon => OsuIcon.ModAutopilot;
        public override ModType Type => ModType.Automation;
        public override LocalisableString Description => @"Automatic cursor movement - just follow the rhythm.";
        public override double ScoreMultiplier => 0.1;

        public override Type[] IncompatibleMods => new[]
        {
            typeof(OsuModSpunOut),
            typeof(ModRelax),
            typeof(ModAutoplay),
            typeof(OsuModMagnetised),
            typeof(OsuModRepel),
            typeof(ModTouchDevice)
        };

        // Values to make cursor movement seem more natural.
        // Assuming people cant tap within (MinStart) ms within an interval.
        // Most likely needs a review.
        private const double MinStart = 20;
        private const double MinEnd = 5;

        // The spinner radius value from OsuAutoGeneratorBase
        private const float SpinnerRadius = 50;
        private const double BaseRps = 500 / 60;

        private OsuInputManager inputManager = null!;
        private Func<HitWindows, double> hitWindowLookup = null!;
        private bool ifReplay;

        public void Update(Playfield playfield)
        {
            if (ifReplay)
                return;

            double currentTime = playfield.Clock.CurrentTime;

            // First alive, unjudged object.
            var active = playfield.HitObjectContainer.AliveObjects
                        .OfType<DrawableOsuHitObject>()
                        .FirstOrDefault(d => !d.Judged);

            if (active == null)
                return;

            var pos = playfield.ToLocalSpace(inputManager.CurrentState.Mouse.Position);
            var target = active.Position;

            var start = active.HitObject.StartTime;

            // Timing of the HitObject's hit window.
            double window = active is DrawableSlider sld
                ? hitWindowLookup(sld.HeadCircle.HitObject.HitWindows)
                : hitWindowLookup(active.HitObject.HitWindows);

            if (active is DrawableSpinner spinnerDrawable)
            {
                var spinner = spinnerDrawable.HitObject;

                spinnerDrawable.RotationTracker.Tracking = spinnerDrawable.RotationTracker.IsSpinnableTime;
                spinnerDrawable.HandleUserInput = false;

                double elapsed = currentTime - start;

                // Before spinner starts, move to position.
                if (elapsed < 0)
                {
                    Vector2 spinnerTargetPosition = spinner.Position + new Vector2(
                        -(float)Math.Sin(0) * SpinnerRadius,
                        -(float)Math.Cos(0) * SpinnerRadius);

                    double duration = currentTime >= start - MinStart
                    ? 1 + Math.Clamp((elapsed) / (spinner.Duration - MinEnd), 0, 1) * (MinStart - 1)
                    : -elapsed;

                    MoveTowards(pos, spinnerTargetPosition, duration, playfield);

                    return;
                }

                // Rotate around centre
                double rate = BaseRps / (playfield.Clock.Rate * 1000);

                if (rate <= 0)
                    return;

                double angle = 2 * Math.PI * (elapsed * rate);
                Vector2 circPos = spinner.Position + new Vector2(
                    -(float)Math.Sin(angle) * SpinnerRadius,
                    -(float)Math.Cos(angle) * SpinnerRadius);

                double rateElapsedTime = playfield.Clock.ElapsedFrameTime;

                // Automatically spin spinner.
                spinnerDrawable.RotationTracker.AddRotation(float.RadiansToDegrees((float)rateElapsedTime * (float)rate * MathF.PI * 2.0f));

                ApplyCursor(circPos, playfield);

                return;
            }

            if (active is DrawableSlider sliderDrawable && sliderDrawable.HeadCircle.Judged)
            {
                var slider = sliderDrawable.HitObject;
                double elapsed = currentTime - start;

                if (elapsed + window >= 0 && elapsed < slider.Duration)
                {
                    double prog = Math.Clamp(elapsed / slider.Duration, 0, 1);
                    double spans = (prog * (slider.RepeatCount + 1));
                    spans = (spans > 1 && spans % 2 > 1) ? 1 - spans % 1 : spans % 1;

                    Vector2 pathPos = sliderDrawable.Position + slider.Path.PositionAt(spans) * sliderDrawable.Scale;

                    ApplyCursor(pathPos, playfield);
                }

                return;
            }

            // Hit circle movement
            double hitWindowStart = start - window - MinStart;
            double hitWindowEnd = start + window - MinEnd;
            double availableTime = currentTime >= hitWindowStart
                    ? 1 + Math.Clamp((hitWindowEnd - currentTime) / (hitWindowEnd - hitWindowStart), 0, 1) * (MinStart - 1)
                    : (start - window - currentTime);

            MoveTowards(pos, target, availableTime, playfield);
        }

        private void MoveTowards(Vector2 current, Vector2 target, double timeMs, Playfield pf)
        {
            float distance = Vector2.Distance(current, target);
            float velocity = distance / (float)timeMs;
            float displacement = velocity * (float)pf.Clock.ElapsedFrameTime;

            Vector2 newPos = displacement >= distance
                ? target
                : current + (target - current).Normalized() * displacement;

            ApplyCursor(newPos, pf);
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            // Grab the input manager to disable the user's cursor, and for future use
            inputManager = ((DrawableOsuRuleset)drawableRuleset).KeyBindingInputManager;
            inputManager.AllowUserCursorMovement = false;

            // Without this, replay starts having a little seizure when rewinding
            // due to how Update calculates mouse positions.
            ifReplay = drawableRuleset.HasReplayLoaded.Value;
            drawableRuleset.HasReplayLoaded.BindValueChanged(
                e => ifReplay = e.NewValue,
                runOnceImmediately: true
            );

            // HitWindow lookup setup for future HitObjects.
            hitWindowLookup = hw => hw.WindowFor(HitResult.Meh);
        }

        private void ApplyCursor(Vector2 playfieldPosition, Playfield playfield)
        {
            new MousePositionAbsoluteInput
            {
                Position = playfield.ToScreenSpace(playfieldPosition)
            }.Apply(inputManager.CurrentState, inputManager);
        }
    }
}