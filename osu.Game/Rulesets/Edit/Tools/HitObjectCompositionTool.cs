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

        public Func<InputState, MouseDownEventArgs, bool> OnMouseDown;
        public Func<InputState, MouseDownEventArgs, bool> OnMouseUp;
        public Func<InputState, MouseDownEventArgs, bool> OnDragStart;
        public Func<InputState, MouseDownEventArgs, bool> OnDragRequested;
        public Func<InputState, MouseDownEventArgs, bool> OnDragEnd;

        public HitObjectCompositionTool()
        {
        }

        public HitObjectCompositionTool(string name)
        {
            Name = name;
        }
    }
}
