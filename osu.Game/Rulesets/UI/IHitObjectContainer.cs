// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.UI
{
    public interface IHitObjectContainer
    {
        /// <summary>
        /// All currently in-use <see cref="DrawableHitObject"/>s.
        /// </summary>
        IEnumerable<DrawableHitObject> Objects { get; }

        /// <summary>
        /// All currently in-use <see cref="DrawableHitObject"/>s that are alive.
        /// </summary>
        /// <remarks>
        /// If this <see cref="IHitObjectContainer"/> uses pooled objects, this is equivalent to <see cref="Objects"/>.
        /// </remarks>
        IEnumerable<DrawableHitObject> AliveObjects { get; }
    }
}
