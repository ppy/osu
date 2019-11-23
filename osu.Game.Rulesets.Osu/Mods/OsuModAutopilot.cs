﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
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

        private int currentFrame;

        public void Update(Playfield playfield)
        {
            if (currentFrame == replayFrames.Count - 1) return;

            double time = playfield.Time.Current;

            // Very naive implementation of autopilot based on proximity to replay frames.
            // TODO: this needs to be based on user interactions to better match stable (pausing until judgement is registered).
            if (Math.Abs(replayFrames[currentFrame + 1].Time - time) <= Math.Abs(replayFrames[currentFrame].Time - time))
            {
                currentFrame++;
                new MousePositionAbsoluteInput { Position = playfield.ToScreenSpace(replayFrames[currentFrame].Position) }.Apply(inputManager.CurrentState, inputManager);
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
