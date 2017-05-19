// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Mania.Objects.Types;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Mania.Objects
{
    public abstract class ManiaHitObject : HitObject, IHasColumn
    {
        public int Column { get; set; }

        /// <summary>
        /// The number of other <see cref="ManiaHitObject"/> that start at
        /// the same time as this hit object.
        /// </summary>
        public int Siblings { get; set; }
    }
}
