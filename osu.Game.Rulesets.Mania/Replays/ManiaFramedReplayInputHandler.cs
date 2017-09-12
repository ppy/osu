// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Input;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Rulesets.Mania.Replays
{
    internal class ManiaFramedReplayInputHandler : FramedReplayInputHandler
    {
        public ManiaFramedReplayInputHandler(Replay replay)
            : base(replay)
        {
        }

        public override List<InputState> GetPendingStates()
        {
            var actions = new List<ManiaAction>();

            int activeColumns = (int)(CurrentFrame.MouseX ?? 0);

            int counter = 0;
            while (activeColumns > 0)
            {
                if ((activeColumns & 1) > 0)
                    actions.Add(ManiaAction.Key1 + counter);
                counter++;
                activeColumns >>= 1;
            }

            return new List<InputState> { new ReplayState<ManiaAction> { PressedActions = actions } };
        }
    }
}
