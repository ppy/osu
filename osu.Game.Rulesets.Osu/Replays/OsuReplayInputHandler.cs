// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Input.StateChanges;
using osu.Framework.MathUtils;
using osu.Game.Replays;
using osu.Game.Rulesets.Replays;
using osuTK;

namespace osu.Game.Rulesets.Osu.Replays
{
    public class OsuReplayInputHandler : FramedReplayInputHandler<OsuReplayFrame>
    {
        public OsuReplayInputHandler(Replay replay)
            : base(replay)
        {
        }

        protected override bool IsImportant(OsuReplayFrame frame) => frame.Actions.Any();

        protected Vector2? Position
        {
            get
            {
                if (!HasFrames)
                    return null;

                return Interpolation.ValueAt(CurrentTime, CurrentFrame.Position, NextFrame.Position, CurrentFrame.Time, NextFrame.Time);
            }
        }

        public override List<IInput> GetPendingInputs()
        {
            return new List<IInput>
            {
                new MousePositionAbsoluteInput
                {
                    Position = GamefieldToScreenSpace(Position ?? Vector2.Zero)
                },
                new ReplayState<OsuAction>
                {
                    PressedActions = CurrentFrame.Actions
                }
            };
        }
    }
}
