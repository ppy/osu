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
        protected LegacyTaikoReplay()
        {
        }

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
}
