// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints
{
    /// <summary>
    /// A piece of a selection or placement blueprint which visualises an <see cref="OsuHitObject"/>.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="OsuHitObject"/> which this <see cref="BlueprintPiece{T}"/> visualises.</typeparam>
    public abstract partial class BlueprintPiece<T> : CompositeDrawable
        where T : OsuHitObject
    {
        /// <summary>
        /// Updates this <see cref="BlueprintPiece{T}"/> using the properties of a <see cref="OsuHitObject"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="OsuHitObject"/> to reference properties from.</param>
        public virtual void UpdateFrom(T hitObject)
        {
            Position = hitObject.StackedPosition;
        }
    }
}
