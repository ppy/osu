// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.StateChanges;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.UI;
using osu.Framework.Logging;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModAutopilot : Mod, IApplicableFailOverride, IUpdatableByPlayfield, IApplicableToDrawableRuleset<OsuHitObject>
    {
        public override string Name => "Autopilot";
        public override string Acronym => "AP";
        public override IconUsage? Icon => OsuIcon.ModAutopilot;
        public override ModType Type => ModType.Automation;
        public override string Description => @"Automatic cursor movement - just follow the rhythm.";
        public override double ScoreMultiplier => 1;
        public override Type[] IncompatibleMods => new[] { typeof(OsuModSpunOut), typeof(ModRelax), typeof(ModFailCondition), typeof(ModNoFail), typeof(ModAutoplay) };

        public bool PerformFail() => false;

        public bool RestartOnFail => false;

        private OsuInputManager inputManager;

        private IFrameStableClock gameplayClock;

        private List<OsuReplayFrame> replayFrames;

        private int currentFrame;

        int cachedFrames = 1;
        private const float relax_leniency = 3f;

        public void Update(Playfield playfield)
        {
            if (currentFrame == replayFrames.Count - 1) return;

            double time = playfield.Clock.CurrentTime;

            bool breakNow = false;
            bool pause = false;
            foreach (var h in playfield.HitObjectContainer.AliveObjects.OfType<DrawableOsuHitObject>())
            {
                // We are not yet close enough to the object time
                if(time < h.HitObject.StartTime - relax_leniency) 
                    break;

                // Already hit or beyond the hittable end time.
                // Here a relax_leniency is used instead of end time because end time causes lags for some reason
                if(h.IsHit || time > h.HitObject.StartTime + relax_leniency)
                    continue;

                switch (h)
                {
                    case DrawableHitCircle circle:
                        pause = true;
                        breakNow = true;
                        break;

                    case DrawableSlider slider:
                        breakNow = true;
                        break;

                    case DrawableSpinner _:
                        breakNow = true;
                        break;
                }

                if(breakNow) {
                    break;
                }
            }

            // If it's time to move the cursor, check if it needs to be paused 
            // (to account for player error when tapping)
            // If not, catch up to the current position required
            if(Math.Abs(replayFrames[currentFrame + cachedFrames].Time - time) <= Math.Abs(replayFrames[currentFrame].Time - time))
            {
                if(pause) {
                    cachedFrames++;
                } else {
                    currentFrame += cachedFrames;
                    cachedFrames = 1;
                }
            }

            new MousePositionAbsoluteInput { Position = playfield.ToScreenSpace(replayFrames[currentFrame].Position) }.Apply(inputManager.CurrentState, inputManager);
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            gameplayClock = drawableRuleset.FrameStableClock;

            // Grab the input manager to disable the user's cursor, and for future use
            inputManager = (OsuInputManager)drawableRuleset.KeyBindingInputManager;
            inputManager.AllowUserCursorMovement = false;

            // Generate the replay frames the cursor should follow
            replayFrames = new OsuAutoGenerator(drawableRuleset.Beatmap, drawableRuleset.Mods).Generate().Frames.Cast<OsuReplayFrame>().ToList();
        }
    }
}
