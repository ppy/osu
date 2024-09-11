// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Framework.Input.StateChanges;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModCipher : ModCipher, IUpdatableByPlayfield, IApplicableToDrawableRuleset<OsuHitObject>
    {
        public override LocalisableString Description => "Cipher for Osu";
        public override Type[] IncompatibleMods => [];

        // public override ModReplayData CreateReplayData(IBeatmap beatmap, IReadOnlyList<Mod> mods)
        //     => new ModReplayData(new OsuAutoGenerator(beatmap, mods).Generate(), new ModCreatedUser { Username = "Autoplay" });

        private int currentFrame = -1;
        private OsuInputManager inputManager = null!;
        // OsuModAutopilot
        public void Update(Playfield playfield)
        {
            if (replayFrames == null) return;

            double time = playfield.Clock.CurrentTime;

            if (currentFrame < 0 || Math.Abs(replayFrames[currentFrame + 1].Time - time) <= Math.Abs(replayFrames[currentFrame].Time - time))
            {
                currentFrame++;
                float newPositionX = inputManager.CurrentState.Mouse.Position.X + 0.01f;
                Vector2 newPosition = inputManager.CurrentState.Mouse.Position;
                newPosition.X = newPositionX;

                // new MousePositionAbsoluteInput { Position = playfield.ToScreenSpace(newPosition) }.Apply(inputManager.CurrentState, inputManager);

                // new MousePositionAbsoluteInput { Position = playfield.ToScreenSpace(inputManager.CurrentState.Mouse.Position) }.Apply(new Vector2 {X = newPositionX; yy}, inputManager);
                new MousePositionAbsoluteInput { Position = playfield.ToScreenSpace(newPosition) }.Apply(inputManager.CurrentState, inputManager);
            }

            // TODO: Implement the functionality to automatically spin spinners
        }



        private List<OsuReplayFrame> replayFrames = null!;

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            // replayFrames are only used for timing, nothing else

            // Grab the input manager to disable the user's cursor, and for future use
            inputManager = ((DrawableOsuRuleset)drawableRuleset).KeyBindingInputManager;
            inputManager.AllowUserCursorMovement = true;

            // Generate the replay frames the cursor should follow
            replayFrames = new OsuAutoGenerator(drawableRuleset.Beatmap, drawableRuleset.Mods).Generate().Frames.Cast<OsuReplayFrame>().ToList();
        }
    }
}
