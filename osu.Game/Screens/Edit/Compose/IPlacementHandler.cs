// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Edit.Compose
{
    public interface IPlacementHandler
    {
        /// <summary>
        /// Notifies that a placement has begun.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> being placed.</param>
        void BeginPlacement(HitObject hitObject);

        /// <summary>
        /// Notifies that a placement has finished.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> that has been placed.</param>
        /// <param name="commit">Whether the object should be committed.</param>
        void EndPlacement(HitObject hitObject, bool commit);

        /// <summary>
        /// Deletes a <see cref="HitObject"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> to delete.</param>
        void Delete(HitObject hitObject);
    }
}
