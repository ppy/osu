// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Input;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Rulesets.Catch.Replays
{
    public class CatchFramedReplayInputHandler : FramedReplayInputHandler
    {
        public CatchFramedReplayInputHandler(Replay replay)
            : base(replay)
        {
        }

        public override List<InputState> GetPendingStates()
        {
            if (!Position.HasValue) return new List<InputState>();

            var action = new List<CatchAction>();

            if (CurrentFrame.ButtonState == ReplayButtonState.Left1)
                action.Add(CatchAction.Dash);

            if (Position.Value.X > CurrentFrame.Position.X)
                action.Add(CatchAction.MoveRight);
            else if (Position.Value.X < CurrentFrame.Position.X)
                action.Add(CatchAction.MoveLeft);

            return new List<InputState>
            {
                new CatchReplayState
                {
                    PressedActions = action,
                    CatcherX = Position.Value.X
                },
            };
        }

        public class CatchReplayState : ReplayState<CatchAction>
        {
            public float? CatcherX { get; set; }
        }
    }
}
