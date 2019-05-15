// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
        /// This will only be invoked with top-level <see cref="DrawableHitObject"/>s. Access <see cref="DrawableHitObject.NestedHitObjects"/> if adjusting nested objects is necessary.
        /// </summary>
        /// <param name="drawables">The list of <see cref="DrawableHitObject"/>s to apply to.</param>
        void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables);
    }
}
