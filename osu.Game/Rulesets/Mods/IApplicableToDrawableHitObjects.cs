// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// An interface for <see cref="Mod"/>s that can be applied to <see cref="DrawableHitObject"/>s.
    /// </summary>
    public interface IApplicableToDrawableHitObjects
    {
        /// <summary>
        /// Applies this <see cref="IApplicableToDrawableHitObjects"/> to a list of <see cref="DrawableHitObject"/>
        /// </summary>
        /// <param name="drawables"></param>
        void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables);
    }
}
