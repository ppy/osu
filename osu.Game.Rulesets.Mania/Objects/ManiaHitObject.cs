// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Mania.Objects.Types;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Mania.Objects
{
    public abstract class ManiaHitObject : HitObject, IHasColumn
    {
        public virtual int Column { get; set; }
    }
}
