// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.StateChanges;
using osu.Framework.Localisation;
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

        // When currentTime equals the start of the hitwindow minus the start offset, we start reducing availableTime 
        // from this value down to 1 when currentTime equals the end of the hitwindow minus the end offset.
        // This ensures that, if we enter the window late, we still have some room for natural cursor movement.
        private const double hitwindow_start_offset = 20;
        private const double hitwindow_end_offset = 5;

        // The spinner radius value from OsuAutoGeneratorBase
        private const float spinner_radius = 50;

        private OsuInputManager inputManager = null!;
        private Playfield playfield = null!;

        private readonly IBindable<bool> hasReplayLoaded = new Bindable<bool>();

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            // Grab the input manager to disable the user's cursor, and for future use
            inputManager = ((DrawableOsuRuleset)drawableRuleset).KeyBindingInputManager;
            inputManager.AllowUserCursorMovement = false;

            playfield = drawableRuleset.Playfield;

            hasReplayLoaded.BindTo(drawableRuleset.HasReplayLoaded);
        }

        public void Update(Playfield playfield)
        {
            if (hasReplayLoaded.Value)
                return;

            double currentTime = playfield.Clock.CurrentTime;

            var nextObject = playfield.HitObjectContainer.AliveObjects.FirstOrDefault(d => !d.Judged);

            if (nextObject == null)
                return;

            // Sliders do not have windows except for the HeadCircle, so we need to check for sliders.
            double mehWindow = nextObject is DrawableSlider checkForSld
                ? checkForSld.HeadCircle.HitObject.HitWindows.WindowFor(HitResult.Meh)
                : nextObject.HitObject.HitWindows.WindowFor(HitResult.Meh);

            var pos = playfield.ToLocalSpace(inputManager.CurrentState.Mouse.Position);
            var target = nextObject.Position;

            double start = nextObject.HitObject.StartTime;

            switch (nextObject)
            {
                case DrawableSpinner spinnerDrawable:
                    handleSpinner(spinnerDrawable, currentTime, start, pos);
                    return;

                case DrawableSlider sliderDrawable:
                    if (!sliderDrawable.HeadCircle.Judged)
                        break;

                    var slider = sliderDrawable.HitObject;
                    double elapsed = currentTime - start;

                    if (elapsed + mehWindow >= 0 && elapsed < slider.Duration)
                    {
                        double prog = Math.Clamp(elapsed / slider.Duration, 0, 1);
                        double spans = (prog * (slider.RepeatCount + 1));
                        spans = (spans > 1 && spans % 2 > 1) ? 1 - (spans % 1) : spans % 1;

                        Vector2 pathPos = sliderDrawable.Position + (slider.Path.PositionAt(spans) * sliderDrawable.Scale);

                        applyCursor(pathPos);
                    }

                    return;
            }

            double hitWindowStart = start - mehWindow - hitwindow_start_offset;
            double hitWindowEnd = start + mehWindow - hitwindow_end_offset;

            // Compute how many ms remain for cursor movement toward the hit-object
            // If we’re already inside the hit-window, scale remaining time from hitwindow_start_offset to 1 ms
            // proportionally to the fraction of window left (clamped to [0,1]) to prevent extremely fast cursor movement.
            // Otherwise, use the time until the window opens so the cursor reaches the HitObject at hitWindowStart.
            double availableTime = currentTime >= hitWindowStart
                ? 1 + (Math.Clamp((hitWindowEnd - currentTime) / (hitWindowEnd - hitWindowStart), 0, 1) * (hitwindow_start_offset - 1))
                : (start - mehWindow - currentTime);

            moveTowards(pos, target, availableTime);
        }

        private void handleSpinner(DrawableSpinner spinnerDrawable, double currentTime, double start, Vector2 pos)
        {
            var spinner = spinnerDrawable.HitObject;

            spinnerDrawable.RotationTracker.Tracking = spinnerDrawable.RotationTracker.IsSpinnableTime;
            spinnerDrawable.HandleUserInput = false;

            double elapsed = currentTime - start;

            // Before spinner starts, move to position.
            if (elapsed < 0)
            {
                Vector2 spinnerTargetPosition = spinner.Position + new Vector2(
                    -(float)Math.Sin(0) * spinner_radius,
                    -(float)Math.Cos(0) * spinner_radius);

                double duration = currentTime >= start - hitwindow_start_offset
                    ? 1 + (Math.Clamp(((spinner.Duration - hitwindow_end_offset) - (elapsed + hitwindow_start_offset)) / (spinner.Duration - hitwindow_end_offset), 0, 1) * (hitwindow_start_offset - 1))
                    : -elapsed;

                moveTowards(pos, spinnerTargetPosition, duration);

                return;
            }

            double calculatedSpeed = 1.01 * (spinner.MaximumBonusSpins + spinner.SpinsRequiredForBonus) / spinner.Duration;

            // Rotate around centre
            double rate = calculatedSpeed / playfield.Clock.Rate;

            if (rate <= 0)
                return;

            double angle = 2 * Math.PI * (elapsed * rate);
            Vector2 circPos = spinner.Position + new Vector2(
                -(float)Math.Sin(angle) * spinner_radius,
                -(float)Math.Cos(angle) * spinner_radius);

            double rateElapsedTime = playfield.Clock.ElapsedFrameTime;

            // Automatically spin spinner.
            spinnerDrawable.RotationTracker.AddRotation(float.RadiansToDegrees((float)rateElapsedTime * (float)rate * MathF.PI * 2.0f));

            applyCursor(circPos);
        }

        private void moveTowards(Vector2 current, Vector2 target, double timeMs)
        {
            // Calculate the straight-line distance between current and target positions,
            // then compute the constant velocity (units per ms) needed to cover that distance.
            // Convert to per-frame displacement: velocity × elapsed frame time.
            float distance = Vector2.Distance(current, target);
            float velocity = distance / (float)timeMs;
            float displacement = velocity * (float)playfield.Clock.ElapsedFrameTime;

            // If we'd overshoot, snap exactly to target; otherwise move along the unit (normalized) direction.
            Vector2 newPos = displacement >= distance
                ? target
                : current + ((target - current).Normalized() * displacement);

            applyCursor(newPos);
        }

        private void applyCursor(Vector2 playfieldPosition)
        {
            new MousePositionAbsoluteInput { Position = playfield.ToScreenSpace(playfieldPosition) }.Apply(inputManager.CurrentState, inputManager);
        }
    }
}
