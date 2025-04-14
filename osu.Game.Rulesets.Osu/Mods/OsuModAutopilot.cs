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

        private int currentReplayFrameIndex;

        public void Update(Playfield playfield)
        {
            if (currentReplayFrameIndex == replayFrames.Count - 1)
            {
                return;
            }

            double currentTime = playfield.Clock.CurrentTime;
            var currentReplayFrame = replayFrames[currentReplayFrameIndex];
            var nextReplayFrame = replayFrames[currentReplayFrameIndex + 1];

            var closestUnjudgedHitObject = playfield.HitObjectContainer.AliveObjects.OfType<DrawableOsuHitObject>().FirstOrDefault(x => !x.Judged);
            bool pauseMousePositionUpdates = closestUnjudgedHitObject switch
            {
                DrawableHitCircle circle => isMouseDirectlyOverHitObject(playfield, circle),
                DrawableSlider slider => isMouseDirectlyOverHitObject(playfield, slider) && !slider.HeadCircle.Judged,
                _ => false
            };

            if (!pauseMousePositionUpdates)
            {
                var mousePosition = Interpolation.ValueAt(currentTime, currentReplayFrame.Position, nextReplayFrame.Position, currentReplayFrame.Time, nextReplayFrame.Time);
                new MousePositionAbsoluteInput { Position = playfield.ToScreenSpace(mousePosition) }.Apply(inputManager.CurrentState, inputManager);
            }

            if (currentTime >= nextReplayFrame.Time)
            {
                currentReplayFrameIndex++;
            }

            // TODO: Implement the functionality to automatically spin spinners
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            // Grab the input manager to disable the user's cursor, and for future use
            inputManager = ((DrawableOsuRuleset)drawableRuleset).KeyBindingInputManager;
            inputManager.AllowUserCursorMovement = false;

            // Generate the replay frames the cursor should follow
            replayFrames = new OsuAutoGenerator(drawableRuleset.Beatmap, drawableRuleset.Mods).Generate().Frames.Cast<OsuReplayFrame>().ToList();
        }

        private bool isMouseDirectlyOverHitObject(Playfield playfield, DrawableOsuHitObject hitObject)
        {
            var localSpaceMousePosition = playfield.ToLocalSpace(inputManager.CurrentState.Mouse.Position);
            return Vector2.Distance(localSpaceMousePosition, hitObject.Position) < 1;
        }
    }
}
