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
            if (currentFrame == replayFrames.Count - 1) return;

            double time = playfield.Clock.CurrentTime;

            while (currentFrame < replayFrames.Count - 1 && time > replayFrames[currentFrame + 1].Time)
            {
                currentFrame++;
            }

            // Very naive implementation of autopilot based on interpolation between replay frames.
            // Special case for the first frame is required to ensure the mouse is in a sane position until the actual time of the first frame is hit.
            // TODO: this needs to be based on user interactions to better match stable (pausing until judgement is registered).
            Vector2 position = Vector2.Zero;
            if (currentFrame < 0)
            {
                position = replayFrames.First().Position;
            }
            else if (currentFrame == replayFrames.Count - 1)
            {
                position = replayFrames.Last().Position;
            }
            else
            {
                position = Interpolation.ValueAt(time, replayFrames[currentFrame].Position, replayFrames[currentFrame + 1].Position, replayFrames[currentFrame].Time, replayFrames[currentFrame + 1].Time);
            }
            new MousePositionAbsoluteInput { Position = playfield.ToScreenSpace(position) }.Apply(inputManager.CurrentState, inputManager);

            // TODO: Implement the functionality to automatically spin spinners
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            // Grab the input manager to disable the user's cursor, and for future use
            inputManager = ((DrawableOsuRuleset)drawableRuleset).KeyBindingInputManager;
            inputManager.AllowUserCursorMovement = false;

            // Generate the replay frames the cursor should follow
            replayFrames = new OsuAutoGenerator(drawableRuleset.Beatmap, drawableRuleset.Mods).Generate().Frames.Cast<OsuReplayFrame>().ToList();

            drawableRuleset.UseResumeOverlay = false;
        }
    }
}
