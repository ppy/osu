// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Input.Handlers;

namespace osu.Game.Modes
{
    public abstract class Replay
    {
        public virtual ReplayInputHandler GetInputHandler() => null;
    }
}