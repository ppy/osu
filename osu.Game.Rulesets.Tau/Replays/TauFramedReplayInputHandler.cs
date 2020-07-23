// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Input.StateChanges;
using osu.Framework.Utils;
using osu.Game.Replays;
using osu.Game.Rulesets.Replays;
using osuTK;

namespace osu.Game.Rulesets.Tau.Replays
{
    public class TauFramedReplayInputHandler : FramedReplayInputHandler<TauReplayFrame>
    {
        public TauFramedReplayInputHandler(Replay replay)
            : base(replay)
        {
        }

        protected override bool IsImportant(TauReplayFrame frame) => frame.Actions.Any();

        protected Vector2? Position
        {
            get
            {
                var frame = CurrentFrame;

                if (frame == null)
                    return null;

                Debug.Assert(CurrentTime != null);

                return NextFrame != null ? Interpolation.ValueAt(CurrentTime.Value, frame.Position, NextFrame.Position, frame.Time, NextFrame.Time) : frame.Position;
            }
        }

        public override void CollectPendingInputs(List<IInput> inputs)
        {
            inputs.Add(new MousePositionAbsoluteInput { Position = GamefieldToScreenSpace(Position ?? Vector2.Zero) });
            inputs.Add(new ReplayState<TauAction> { PressedActions = CurrentFrame?.Actions ?? new List<TauAction>() });
        }
    }
}
