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

        // Values to make cursor movement seem more natural.
        // Assuming people cant tap within (MinStart) ms within an interval.
        // Most likely needs a review.
        private const double min_start = 20;
        private const double min_end = 5;

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

            double mehWindow = nextObject.HitObject.HitWindows.WindowFor(HitResult.Meh);

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

                    mehWindow = slider.HitWindows.WindowFor(HitResult.Meh);

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

            double hitWindowStart = start - mehWindow - min_start;
            double hitWindowEnd = start + mehWindow - min_end;
            double availableTime = currentTime >= hitWindowStart
                ? 1 + (Math.Clamp((hitWindowEnd - currentTime) / (hitWindowEnd - hitWindowStart), 0, 1) * (min_start - 1))
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

                double duration = currentTime >= start - min_start
                    ? 1 + (Math.Clamp(((spinner.Duration - min_end) - (elapsed + min_start)) / (spinner.Duration - min_end), 0, 1) * (min_start - 1))
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
            float distance = Vector2.Distance(current, target);
            float velocity = distance / (float)timeMs;
            float displacement = velocity * (float)playfield.Clock.ElapsedFrameTime;

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
