// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Input.StateChanges;
using osu.Framework.Utils;
using osu.Game.Replays;
using osu.Game.Rulesets.Replays;
using osuTK;

namespace osu.Game.Rulesets.Pippidon.Replays
{
    public class PippidonFramedReplayInputHandler : FramedReplayInputHandler<PippidonReplayFrame>
    {
        public PippidonFramedReplayInputHandler(Replay replay)
            : base(replay)
        {
        }

        protected override bool IsImportant(PippidonReplayFrame frame) => true;

        protected Vector2 Position
        {
            get
            {
                var frame = CurrentFrame;

                if (frame == null)
                    return Vector2.Zero;

                return NextFrame != null ? Interpolation.ValueAt(CurrentTime, frame.Position, NextFrame.Position, frame.Time, NextFrame.Time) : frame.Position;
            }
        }

        public override void CollectPendingInputs(List<IInput> inputs)
        {
            inputs.Add(new MousePositionAbsoluteInput
            {
                Position = GamefieldToScreenSpace(Position)
            });
        }
    }
}
