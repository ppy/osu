// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Input.Handlers;

namespace osu.Game.Modes.Replays
{
    public abstract class FramedReplay : Replay
    {
        protected List<ReplayFrame> Frames = new List<ReplayFrame>();

        public override ReplayInputHandler CreateInputHandler() => new FramedReplayInputHandler(Frames);
    }
}