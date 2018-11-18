// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
        void EndPlacement(HitObject hitObject);

        /// <summary>
        /// Deletes a <see cref="HitObject"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> to delete.</param>
        void Delete(HitObject hitObject);
    }
}
