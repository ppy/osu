// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// An interface for <see cref="Mod"/>s that can be applied to <see cref="DrawableHitObject"/>s.
    /// </summary>
    public interface IApplicableToDrawableHitObjects : IApplicableMod
    {
        /// <summary>
        /// Applies this <see cref="IApplicableToDrawableHitObjects"/> to a list of <see cref="DrawableHitObject"/>s.
        /// </summary>
        /// <param name="drawables">The list of <see cref="DrawableHitObject"/>s to apply to.</param>
        void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables);
    }
}
