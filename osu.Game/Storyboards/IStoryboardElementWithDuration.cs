// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Storyboards
{
    /// <summary>
    /// A <see cref="IStoryboardElement"/> that ends at a different time than its start time.
    /// </summary>
    public interface IStoryboardElementWithDuration : IStoryboardElement
    {
        /// <summary>
        /// The time at which the <see cref="IStoryboardElement"/> ends.
        /// This is consumed to extend the length of a storyboard to ensure all visuals are played to completion.
        /// </summary>
        double EndTime { get; }

        /// <summary>
        /// The time this element displays until.
        /// This is used for lifetime purposes, and includes long playing animations which don't necessarily extend
        /// a storyboard's play time.
        /// </summary>
        double EndTimeForDisplay { get; }

        /// <summary>
        /// The duration of the StoryboardElement.
        /// </summary>
        double Duration => EndTime - StartTime;
    }
}
