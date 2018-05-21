// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Input;
using osu.Framework.MathUtils;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Rulesets.Catch.Replays
{
    public class CatchFramedReplayInputHandler : FramedReplayInputHandler<CatchReplayFrame>
    {
        public CatchFramedReplayInputHandler(Replay replay)
            : base(replay)
        {
        }

        protected override bool IsImportant(CatchReplayFrame frame) => frame.Position > 0;

        protected float? Position
        {
            get
            {
                if (!HasFrames)
                    return null;

                return Interpolation.ValueAt(CurrentTime, CurrentFrame.Position, NextFrame.Position, CurrentFrame.Time, NextFrame.Time);
            }
        }

        public override List<InputState> GetPendingStates()
        {
            if (!Position.HasValue) return new List<InputState>();

            var actions = new List<CatchAction>();

            if (CurrentFrame.Dashing)
                actions.Add(CatchAction.Dash);

            if (Position.Value > CurrentFrame.Position)
                actions.Add(CatchAction.MoveRight);
            else if (Position.Value < CurrentFrame.Position)
                actions.Add(CatchAction.MoveLeft);

            return new List<InputState>
            {
                new CatchReplayState
                {
                    PressedActions = actions,
                    CatcherX = Position.Value
                },
            };
        }

        public class CatchReplayState : ReplayState<CatchAction>
        {
            public float? CatcherX { get; set; }
        }
    }
}
