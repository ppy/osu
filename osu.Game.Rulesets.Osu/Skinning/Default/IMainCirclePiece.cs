// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Skinning.Default
{
    public interface IMainCirclePiece
    {
        /// <summary>
        /// Begins animating this <see cref="IMainCirclePiece"/>.
        /// </summary>
        /// <param name="state">The <see cref="ArmedState"/> of the related <see cref="DrawableHitCircle"/>.</param>
        void Animate(ArmedState state);
    }
}
