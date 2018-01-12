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

        public override List<InputState> GetPendingStates() => new List<InputState>
        {
            new CatchReplayState
            {
                PressedActions = new List<CatchAction> { CatchAction.PositionUpdate },
                CatcherX = ((CatchReplayFrame)CurrentFrame).MouseX
            },
            new CatchReplayState { PressedActions = new List<CatchAction>() },
        };

        public class CatchReplayState : ReplayState<CatchAction>
        {
            public float? CatcherX { get; set; }
        }
    }
}
