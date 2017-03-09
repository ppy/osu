// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.IO;
using osu.Framework.Input;
using osu.Game.Input.Handlers;
using OpenTK.Input;

namespace osu.Game.Modes.Taiko.Replays
{
    public class LegacyTaikoReplay : LegacyReplay
    {
        public LegacyTaikoReplay()
        {
        }

        public LegacyTaikoReplay(StreamReader reader)
            : base(reader)
        {
        }

        public override ReplayInputHandler GetInputHandler() => new LegacyTaikoInputHandler(Frames);

        private class LegacyTaikoInputHandler : LegacyReplayInputHandler
        {
            public LegacyTaikoInputHandler(List<LegacyReplayFrame> replayContent)
                : base(replayContent)
            {
            }

            public override List<InputState> GetPendingStates()
            {
                return new List<InputState>
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
}
