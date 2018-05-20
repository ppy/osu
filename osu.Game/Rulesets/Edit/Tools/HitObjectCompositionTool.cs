// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Input;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Edit.Tools
{
    public class HitObjectCompositionTool<T> : ICompositionTool
        where T : HitObject
    {
        public string Name { get; } = typeof(T).Name;

        public Func<InputState, MouseDownEventArgs, bool> OnMouseDown { get; }
        public Func<InputState, MouseDownEventArgs, bool> OnMouseUp { get; }

        public HitObjectCompositionTool()
        {
        }

        public HitObjectCompositionTool(string name)
        {
            Name = name;
        }

        public HitObjectCompositionTool(string name, Func<InputState, MouseDownEventArgs, bool> onMouseDown, Func<InputState, MouseDownEventArgs, bool> onMouseUp)
        {
            Name = name;
            OnMouseDown = onMouseDown;
            OnMouseUp = onMouseUp;
        }
    }
}
