// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Input;
using System;

namespace osu.Game.Rulesets.Edit.Tools
{
    public interface ICompositionTool
    {
        string Name { get; }

        Func<InputState, MouseDownEventArgs, bool> OnMouseDown { get; }
        Func<InputState, MouseDownEventArgs, bool> OnMouseUp { get; }
    }
}
