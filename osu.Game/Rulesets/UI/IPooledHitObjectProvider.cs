// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.UI
{
    internal interface IPooledHitObjectProvider
    {
        /// <summary>
        /// Attempts to retrieve the poolable <see cref="DrawableHitObject"/> representation of a <see cref="HitObject"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> to retrieve the <see cref="DrawableHitObject"/> representation of.</param>
        /// <param name="parent">The parenting <see cref="DrawableHitObject"/>, if any.</param>
        /// <returns>The <see cref="DrawableHitObject"/> representing <see cref="HitObject"/>, or <c>null</c> if no poolable representation exists.</returns>
        [CanBeNull]
        DrawableHitObject GetPooledDrawableRepresentation([NotNull] HitObject hitObject, [CanBeNull] DrawableHitObject parent);
    }
}
