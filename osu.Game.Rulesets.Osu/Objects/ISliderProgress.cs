// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Osu.Objects
{
    public interface ISliderProgress
    {
        /// <summary>
        /// Updates the progress of this <see cref="ISliderProgress"/> element along the slider.
        /// </summary>
        /// <param name="completionProgress">Amount of the slider completed.</param>
        void UpdateProgress(double completionProgress);
    }
}
