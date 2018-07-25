// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Replays;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Input.StateChanges;

namespace osu.Game.Rulesets.Taiko.Replays
{
    internal class TaikoFramedReplayInputHandler : FramedReplayInputHandler<TaikoReplayFrame>
    {
        public TaikoFramedReplayInputHandler(Replay replay)
            : base(replay)
        {
        }

        protected override bool IsImportant(TaikoReplayFrame frame) => frame.Actions.Any();

        public override List<IInput> GetPendingInputs() => new List<IInput> { new ReplayState<TaikoAction> { PressedActions = CurrentFrame.Actions } };
    }
}
