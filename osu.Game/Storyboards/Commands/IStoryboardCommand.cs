// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;

namespace osu.Game.Storyboards.Commands
{
    public interface IStoryboardCommand
    {
        /// <summary>
        /// The start time of the storyboard command.
        /// </summary>
        double StartTime { get; }

        /// <summary>
        /// The end time of the storyboard command.
        /// </summary>
        double EndTime { get; }

        /// <summary>
        /// Applies the transforms described by this storyboard command to the target drawable.
        /// </summary>
        /// <param name="d">The target drawable.</param>
        /// <returns>The sequence of transforms applied to the target drawable.</returns>
        TransformSequence<Drawable> ApplyTransform(Drawable d);
    }
}
