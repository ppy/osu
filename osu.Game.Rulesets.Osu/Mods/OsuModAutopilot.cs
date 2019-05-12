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

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModAutopilot : Mod, IApplicableFailOverride, IUpdatableByPlayfield, IApplicableToDrawableRuleset<OsuHitObject>
    {
        public override string Name => "Autopilot";
        public override string Acronym => "AP";
        public override IconUsage Icon => OsuIcon.ModAutopilot;
        public override ModType Type => ModType.Automation;
        public override string Description => @"Automatic cursor movement - just follow the rhythm.";
        public override double ScoreMultiplier => 1;
        public override Type[] IncompatibleMods => new[] { typeof(OsuModSpunOut), typeof(ModRelax), typeof(ModSuddenDeath), typeof(ModNoFail), typeof(ModAutoplay) };

        public bool AllowFail => false;

        private OsuInputManager inputManager;

        private List<OsuReplayFrame> replayFrames;
        private int frameIndex = 0;

        public void Update(Playfield playfield)
        {
            // If we are on the last replay frame, no need to do anything
            if (frameIndex == replayFrames.Count - 1)
            {
                return;
            }

            // Check if we are closer to the next replay frame then the current one
            if (Math.Abs(replayFrames[frameIndex].Time - playfield.Time.Current) >= Math.Abs(replayFrames[frameIndex + 1].Time - playfield.Time.Current))
            {
                // If we are, move to the next frame, and update the mouse position
                frameIndex++;
                new MousePositionAbsoluteInput() { Position = playfield.ToScreenSpace(replayFrames[frameIndex].Position) }.Apply(inputManager.CurrentState, inputManager);
            }

            // TODO: Implement the functionality to automatically spin spinners
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            // Grab the input manager to disable the user's cursor, and for future use
            inputManager = (OsuInputManager)drawableRuleset.KeyBindingInputManager;
            inputManager.AllowUserCursorMovement = false;

            // Generate the replay frames the cursor should follow
            replayFrames = new OsuAutoGenerator(drawableRuleset.Beatmap).Generate().Frames.Cast<OsuReplayFrame>().ToList();
        }
    }
}
