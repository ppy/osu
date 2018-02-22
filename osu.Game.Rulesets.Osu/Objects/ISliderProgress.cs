// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
