// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.StateChanges;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.UI;
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

        private OsuInputManager inputManager = null!;

        private List<OsuReplayFrame> replayFrames = null!;

        private int currentFrame = -1;

        public void Update(Playfield playfield)
        {
            if (replayFrames.Count == 0 || currentFrame == replayFrames.Count - 1) return;

            double time = playfield.Clock.CurrentTime;

            // Interpolate the cursor position using the replay frames.
            // Special case for the first frame is required to ensure the mouse is in a sane position until the actual time of the first frame is hit.
            while (currentFrame > 0 && Math.Abs(replayFrames[currentFrame - 1].Time - time) <= Math.Abs(replayFrames[currentFrame].Time - time))
            {
                currentFrame--;
            }

            while (currentFrame < replayFrames.Count - 1 && Math.Abs(replayFrames[currentFrame + 1].Time - time) <= Math.Abs(replayFrames[currentFrame < 0 ? 0 : currentFrame].Time - time))
            {
                currentFrame++;
            }

            if (currentFrame < 0)
                return;

            Vector2 newPosition = playfield.ToScreenSpace(replayFrames[currentFrame].Position);

            var osuPlayfield = (OsuPlayfield)playfield;

            DrawableOsuHitObject? nextObject = null;

            foreach (var h in osuPlayfield.HitObjectContainer.AliveObjects)
            {
                if (h is not DrawableOsuHitObject osuHitObject || osuHitObject.Result.HasResult)
                    continue;

                if (nextObject == null || osuHitObject.HitObject.StartTime < nextObject.HitObject.StartTime)
                {
                    nextObject = osuHitObject;
                }
            }

            if (nextObject != null)
            {
                // If we are currently "on" an object (in terms of time), we should check if it has been judged.
                // If it hasn't been judged, and we are still within the valid hit window, maybe we should clamp the cursor position to that object's position, effectively "pausing" the cursor movement along the path until the user clicks or it's too late.

                bool shouldStay = false;
                Vector2 stayPosition = Vector2.Zero;

                if (nextObject is DrawableSlider slider)
                {
                    if (!slider.HeadCircle.Result.HasResult)
                    {
                        if (time >= slider.HitObject.StartTime)
                        {
                            shouldStay = true;
                            stayPosition = slider.HitObject.StackedPosition;
                        }
                    }
                }
                else if (nextObject is DrawableHitCircle hitCircle)
                {
                    if (time >= hitCircle.HitObject.StartTime)
                    {
                        shouldStay = true;
                        stayPosition = hitCircle.HitObject.StackedPosition;
                    }
                }

                if (shouldStay)
                {
                    newPosition = playfield.ToScreenSpace(stayPosition);
                }
            }

            new MousePositionAbsoluteInput { Position = newPosition }.Apply(inputManager.CurrentState, inputManager);

            // TODO: Implement the functionality to automatically spin spinners
            // TODO: this needs to be based on user interactions to better match stable (pausing until judgement is registered).
            while (currentFrame < replayFrames.Count - 1)
            {
                if (currentFrame < 0 || Math.Abs(replayFrames[currentFrame + 1].Time - time) <= Math.Abs(replayFrames[currentFrame].Time - time))
                {
                    currentFrame++;
                }
                else
                {
                    break;
                }
            }

            if (currentFrame >= 0)
                new MousePositionAbsoluteInput { Position = playfield.ToScreenSpace(replayFrames[currentFrame].Position) }.Apply(inputManager.CurrentState, inputManager);
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            // Grab the input manager to disable the user's cursor, and for future use
            inputManager = ((DrawableOsuRuleset)drawableRuleset).KeyBindingInputManager;
            inputManager.AllowUserCursorMovement = false;

            // Generate the replay frames the cursor should follow
            replayFrames = new OsuAutoGenerator(drawableRuleset.Beatmap, drawableRuleset.Mods).Generate().Frames.Cast<OsuReplayFrame>().ToList();
            currentFrame = -1;
        }
    }
}
