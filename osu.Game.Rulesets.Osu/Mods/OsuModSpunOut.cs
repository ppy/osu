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
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.UI;
using static osu.Game.Input.Handlers.ReplayInputHandler;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModSpunOut : Mod, IUpdatableByPlayfield, IApplicableToDrawableRuleset<OsuHitObject>
    {
        public override string Name => "Spun Out";
        public override string Acronym => "SO";
        public override IconUsage? Icon => OsuIcon.ModSpunOut;
        public override ModType Type => ModType.DifficultyReduction;
        public override string Description => @"Spinners will be automatically completed";
        public override bool Ranked => true;
        public override double ScoreMultiplier => 0.9;
        private List<OsuReplayFrame> replayFrames;
        private int currentFrame;
        public void Update(Playfield playfield)
        {
            bool requiresHold = false;
            foreach (var drawable in playfield.HitObjectContainer.AliveObjects)
            {
                double time = playfield.Time.Current;
                if (requiresHold |= (drawable is DrawableSpinner))
                {
                    inputManager.AllowUserCursorMovement = false;
                    if (currentFrame == replayFrames.Count - 1)
                    {
                        return;
                    }
                    else if (Math.Abs(replayFrames[currentFrame + 1].Time - time) <= Math.Abs(replayFrames[currentFrame].Time - time))
                    {
                        currentFrame++;
                        addAction(requiresHold);
                        new MousePositionAbsoluteInput { Position = playfield.ToScreenSpace(replayFrames[currentFrame].Position) }.Apply(inputManager.CurrentState, inputManager);
                    }
                }
                else
                {
                    inputManager.AllowUserCursorMovement = true;
                }
            }
        }
        private bool wasHit;
        private OsuInputManager inputManager;
        private void addAction(bool hitting)
        {
            if (wasHit == hitting)
                return;
            wasHit = hitting;
            var state = new ReplayState<OsuAction>
            {
                PressedActions = new List<OsuAction>()
            };
            if (hitting)
            {
                state.PressedActions.Add(OsuAction.LeftButton);
            }
            state.Apply(inputManager.CurrentState, inputManager);
        }
        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            // grab the input manager for future use.
            inputManager = (OsuInputManager)drawableRuleset.KeyBindingInputManager;
            replayFrames = new OsuAutoGenerator(drawableRuleset.Beatmap).Generate().Frames.Cast<OsuReplayFrame>().ToList();
        }
    }
}
