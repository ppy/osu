// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Replays;
using System.Collections.Generic;
using osu.Framework.Input;
using OpenTK.Input;

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
            var keys = new List<Key>();

            if (CurrentFrame?.MouseRight1 == true)
                keys.Add(Key.F);
            if (CurrentFrame?.MouseRight2 == true)
                keys.Add(Key.J);
            if (CurrentFrame?.MouseLeft1 == true)
                keys.Add(Key.D);
            if (CurrentFrame?.MouseLeft2 == true)
                keys.Add(Key.K);

            return new List<InputState>
            {
                new InputState { Keyboard = new ReplayKeyboardState(keys) }
            };
        }
    }
}
