// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning
{
    /// <summary>
    /// A common interface between implementations of the <see cref="OsuSkinComponents.SpinnerBody"/> component that provide approach circles for the spinner.
    /// </summary>
    public interface IHasSpinnerApproachCircle
    {
        /// <summary>
        /// The spinner approach circle.
        /// </summary>
        Drawable ApproachCircle { get; }
    }
}
