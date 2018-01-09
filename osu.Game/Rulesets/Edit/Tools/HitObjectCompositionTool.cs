// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Edit.Tools
{
    public class HitObjectCompositionTool<T> : ICompositionTool
        where T : HitObject
    {
        public string Name => typeof(T).Name;
    }
}
