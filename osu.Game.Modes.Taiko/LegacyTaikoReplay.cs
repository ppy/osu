// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.IO;
using osu.Framework.Input;
using osu.Game.Input.Handlers;
using OpenTK.Input;

namespace osu.Game.Modes.Taiko
{
    public class LegacyTaikoReplay : LegacyReplay
    {
        public LegacyTaikoReplay(StreamReader reader)
            : base(reader)
        {
        }

        public override ReplayInputHandler CreateInputHandler() => new LegacyTaikoReplayInputHandler(Frames);

        private class LegacyTaikoReplayInputHandler : LegacyReplayInputHandler
        {
            public LegacyTaikoReplayInputHandler(List<LegacyReplayFrame> replayContent)
                : base(replayContent)
            {
            }

            public override List<InputState> GetPendingStates() => new List<InputState>
            {
                new InputState
                {
                    Keyboard = new ReplayKeyboardState(new List<Key>(new[]
                    {
                        CurrentFrame?.MouseRight1 == true ? Key.F : Key.Unknown,
                        CurrentFrame?.MouseRight2 == true ? Key.J : Key.Unknown,
                        CurrentFrame?.MouseLeft1 == true ? Key.D : Key.Unknown,
                        CurrentFrame?.MouseLeft2 == true ? Key.K : Key.Unknown
                    }))
                }
            };
        }
    }
}
