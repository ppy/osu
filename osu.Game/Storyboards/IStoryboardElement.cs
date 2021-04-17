// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;

namespace osu.Game.Storyboards
{
    public interface IStoryboardElement
    {
        string Path { get; }
        bool IsDrawable { get; }

        double StartTime { get; }

        Drawable CreateDrawable();
    }

    public static class StoryboardElementExtensions
    {
        /// <summary>
        /// Returns the end time of this object.
        /// </summary>
        /// <remarks>
        /// This returns the <see cref="IStoryboardElementHasDuration.EndTime"/> where available, falling back to <see cref="IStoryboardElement.StartTime"/> otherwise.
        /// </remarks>
        /// <param name="storyboardElement">The object.</param>
        /// <returns>The end time of this object.</returns>
        public static double GetEndTime(this IStoryboardElement storyboardElement) => (storyboardElement as IStoryboardElementHasDuration)?.EndTime ?? storyboardElement.StartTime;
    }
}
