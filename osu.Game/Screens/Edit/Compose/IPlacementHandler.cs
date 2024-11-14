// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Edit.Compose
{
    [Cached]
    public interface IPlacementHandler
    {
        /// <summary>
        /// Notifies that a placement blueprint became visible on the screen.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> representing the placement.</param>
        void ShowPlacement(HitObject hitObject);

        /// <summary>
        /// Notifies that a visible placement blueprint has been hidden.
        /// </summary>
        void HidePlacement();

        /// <summary>
        /// Notifies that a placement has been committed.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> that has been placed.</param>
        void CommitPlacement(HitObject hitObject);

        /// <summary>
        /// Deletes a <see cref="HitObject"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> to delete.</param>
        void Delete(HitObject hitObject);
    }
}
