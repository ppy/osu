// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
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

        // If we are this many ms or less away from the start of the hitwindow, we switch to the scaled time in handleTime.
        private const double hitwindow_start_offset = 25;

        // Spinner radius value from OsuAutoGeneratorBase
        private const float spinner_radius = 50;

        private OsuInputManager inputManager = null!;
        private Playfield playfield = null!;

        private readonly IBindable<bool> hasReplayLoaded = new Bindable<bool>();

        private (Vector2 Position, double Time) lastHitInfo = (default, 0);
        private (double HitWindowStart, double HitWindowEnd) hitWindow = (0, 0);
        private double timeElapsedBetweenHitObjects;

        private double currentTime => playfield.Clock.CurrentTime;

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            // Grab the input manager to disable the user's cursor, and for future use
            inputManager = ((DrawableOsuRuleset)drawableRuleset).KeyBindingInputManager;
            inputManager.AllowUserCursorMovement = false;

            playfield = drawableRuleset.Playfield;

            hasReplayLoaded.BindTo(drawableRuleset.HasReplayLoaded);

            void onLoadCompleteHandler(Drawable drawable)
            {
                Vector2 screenStart = inputManager.CurrentState.Mouse.Position;
                Vector2 fieldStart = playfield.ScreenSpaceToGamefield(screenStart);
                lastHitInfo = (fieldStart, currentTime);

                playfield.OnLoadComplete -= onLoadCompleteHandler;
            }

            playfield.OnLoadComplete += onLoadCompleteHandler;

            // We want to save the position and time when the HitObject was judged for movement calculations.
            playfield.NewResult += (drawableHitObject, result) =>
            {
                Vector2 mousePos = inputManager.CurrentState.Mouse.Position;
                Vector2 fieldPos = playfield.ScreenSpaceToGamefield(mousePos);

                lastHitInfo = (fieldPos, result.TimeAbsolute);
            };
        }

        public void Update(Playfield playfield)
        {
            var nextObject = playfield.HitObjectContainer.AliveObjects.FirstOrDefault(d => !d.Judged);

            if (nextObject == null)
                return;

            double start = nextObject.HitObject.StartTime;
            timeElapsedBetweenHitObjects = currentTime - start;

            // Reduce calculations during replay, since this mod interferes with the actual replay.
            if (hasReplayLoaded.Value)
            {
                if (nextObject is DrawableSpinner replaySpinner)
                {
                    var spinner = replaySpinner.HitObject;

                    if (timeElapsedBetweenHitObjects >= 0)
                    {
                        double rate = calculateSpinnerRate(spinner);
                        spinSpinner(replaySpinner, rate);
                    }
                }

                return;
            }

            // Sliders do not have hitwindows except for the HeadCircle, so we need to check for sliders.
            double mehWindow = nextObject is DrawableSlider checkForSld
                ? checkForSld.HeadCircle.HitObject.HitWindows.WindowFor(HitResult.Meh)
                : nextObject.HitObject.HitWindows.WindowFor(HitResult.Meh);

            hitWindow = (start - mehWindow, start + mehWindow);

            var target = nextObject.Position;

            // If the hitobject doesn't appear during the time it was judged, the cursor will teleport.
            // So, we want to save the time when the hitobject first appears so the cursor can travel smoothly.
            if (nextObject.Entry?.LifetimeStart > lastHitInfo.Time)
            {
                lastHitInfo.Time = nextObject.Entry.LifetimeStart;
                lastHitInfo.Position = playfield.ToLocalSpace(inputManager.CurrentState.Mouse.Position);
            }

            switch (nextObject)
            {
                case DrawableSpinner spinnerDrawable:
                    handleSpinner(spinnerDrawable);
                    return;

                case DrawableSlider sliderDrawable:
                    if (!sliderDrawable.HeadCircle.Judged)
                        break;

                    var slider = sliderDrawable.HitObject;

                    double prog = Math.Clamp(timeElapsedBetweenHitObjects / slider.Duration, 0, 1);
                    double spans = prog * (slider.RepeatCount + 1);

                    double frac = spans % 1;
                    if ((int)spans % 2 == 1)
                        frac = 1 - frac;

                    Vector2 pathPos = sliderDrawable.Position + (slider.Path.PositionAt(frac) * sliderDrawable.Scale);
                    applyCursor(pathPos);

                    return;
            }

            double availableTime = handleTime();
            moveTowards(target, availableTime);
        }

        private double calculateSpinnerRate(Spinner spinner)
        {
            double calculatedSpeed = 1.01 * (spinner.MaximumBonusSpins + spinner.SpinsRequiredForBonus) / spinner.Duration;
            return calculatedSpeed / playfield.Clock.Rate;
        }

        private double handleTime()
        {
            // The cursor should reach the HitCircle, but inside the hit window
            // it needs to move fast enough that a player can't tap it early,
            // while still keeping the movement visually smooth.
            double hitWindowStart = hitWindow.HitWindowStart;
            double hitWindowEnd = hitWindow.HitWindowEnd;
            double lastJudgedTime = lastHitInfo.Time;

            // scaledTime: convert the remaining portion of the hit window into a value between 1 and hitwindow_start_offset.
            double scaledTime = 1 + (Math.Clamp((hitWindowEnd - lastJudgedTime) / (hitWindowEnd - hitWindowStart + hitwindow_start_offset), 0, 1) * (hitwindow_start_offset - 1));

            // If we are too close to the hitWindow, switch to scaledTime.
            double timeLeft = lastJudgedTime >= hitWindowStart - hitwindow_start_offset
                ? scaledTime
                : hitWindowStart - lastJudgedTime;

            return Math.Max(timeLeft, 1);
        }

        private void handleSpinner(DrawableSpinner spinnerDrawable)
        {
            var spinner = spinnerDrawable.HitObject;
            spinnerDrawable.HandleUserInput = false;

            if (timeElapsedBetweenHitObjects < 0)
            {
                Vector2 spinnerTargetPosition = spinner.Position + new Vector2(0, -spinner_radius);

                hitWindow.HitWindowEnd = hitWindow.HitWindowStart + spinner.Duration;
                double duration = handleTime();

                moveTowards(spinnerTargetPosition, duration);
                return;
            }

            double rate = calculateSpinnerRate(spinner);
            spinSpinner(spinnerDrawable, rate);

            double angle = 2 * Math.PI * (timeElapsedBetweenHitObjects * rate);
            Vector2 circPos = spinner.Position + new Vector2(
                -(float)Math.Sin(angle) * spinner_radius,
                -(float)Math.Cos(angle) * spinner_radius);

            applyCursor(circPos);
        }

        private void spinSpinner(DrawableSpinner spinnerDrawable, double rate)
        {
            spinnerDrawable.RotationTracker.AddRotation(float.RadiansToDegrees((float)playfield.Clock.ElapsedFrameTime * (float)rate * MathF.PI * 2.0f));
        }

        private void moveTowards(Vector2 target, double timeMs)
        {
            var (lastHitPosition, lastJudgedTime) = lastHitInfo;

            double elapsed = currentTime - lastJudgedTime;

            // How far we are (as a percentage) between the last judged object and
            // the start of the next object's hit window. The cursor moves proportionally to this percentage.
            double progress = Math.Clamp(elapsed / timeMs, 0, 1);

            Vector2 newPos = Vector2.Lerp(lastHitPosition, target, (float)progress);

            applyCursor(newPos);
        }

        private void applyCursor(Vector2 playfieldPosition)
        {
            new MousePositionAbsoluteInput { Position = playfield.ToScreenSpace(playfieldPosition) }.Apply(inputManager.CurrentState, inputManager);
        }
    }
}