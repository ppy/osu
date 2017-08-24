﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Replays;
using System.Collections.Generic;
using osu.Framework.Input;

namespace osu.Game.Rulesets.Taiko.Replays
{
    internal class TaikoFramedReplayInputHandler : FramedReplayInputHandler
    {
        public TaikoFramedReplayInputHandler(Replay replay)
            : base(replay)
        {
        }

        public override List<InputState> GetPendingStates()
        {
            var actions = new List<TaikoAction>();

            if (CurrentFrame?.MouseRight1 == true)
                actions.Add(TaikoAction.LeftCentre);
            if (CurrentFrame?.MouseRight2 == true)
                actions.Add(TaikoAction.RightCentre);
            if (CurrentFrame?.MouseLeft1 == true)
                actions.Add(TaikoAction.LeftRim);
            if (CurrentFrame?.MouseLeft2 == true)
                actions.Add(TaikoAction.RightRim);

            return new List<InputState> { new ReplayState<TaikoAction> { PressedActions = actions } };
        }
    }
}
