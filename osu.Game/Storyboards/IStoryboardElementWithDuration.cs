// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

namespace osu.Game.Storyboards
{
    /// <summary>
    /// A <see cref="IStoryboardElement"/> that ends at a different time than its start time.
    /// </summary>
    public interface IStoryboardElementWithDuration : IStoryboardElement
    {
        /// <summary>
        /// The time at which the <see cref="IStoryboardElement"/> ends.
        /// </summary>
        double EndTime { get; }

        /// <summary>
        /// The duration of the StoryboardElement.
        /// </summary>
        double Duration => EndTime - StartTime;
    }
}
