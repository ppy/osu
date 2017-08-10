// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Input
{
    public interface IHandleActions<in T>
        where T : struct
    {
        bool OnPressed(T action);
        bool OnReleased(T action);
    }
}