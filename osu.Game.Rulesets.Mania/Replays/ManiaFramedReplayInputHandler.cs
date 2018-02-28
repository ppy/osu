// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Input;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Rulesets.Mania.Replays
{
    internal class ManiaFramedReplayInputHandler : FramedReplayInputHandler<ManiaReplayFrame>
    {
        public ManiaFramedReplayInputHandler(Replay replay)
            : base(replay)
        {
        }

        protected override bool IsImportant(ManiaReplayFrame frame) => frame.Actions.Any();

        public override List<InputState> GetPendingStates() => new List<InputState> { new ReplayState<ManiaAction> { PressedActions = CurrentFrame.Actions } };
    }
}
