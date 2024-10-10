// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;
using osu.Game.Storyboards.Drawables;

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
        /// The name of the <see cref="Drawable"/> property affected by this storyboard command.
        /// Used to apply initial property values based on the list of commands given in <see cref="StoryboardSprite"/>.
        /// </summary>
        string PropertyName { get; }

        /// <summary>
        /// Sets the value of the corresponding property in <see cref="Drawable"/> to the start value of this command.
        /// </summary>
        /// <remarks>
        /// Parameter commands (e.g. <see cref="StoryboardFlipHCommand"/> / <see cref="StoryboardFlipVCommand"/> / <see cref="StoryboardBlendingParametersCommand"/>) only apply the start value if they have zero duration, i.e. take "permanent" effect regardless of time.
        /// </remarks>
        /// <param name="d">The target drawable.</param>
        void ApplyInitialValue<TDrawable>(TDrawable d)
            where TDrawable : Drawable, IFlippable, IVectorScalable;

        /// <summary>
        /// Applies the transforms described by this storyboard command to the target drawable.
        /// </summary>
        /// <param name="d">The target drawable.</param>
        /// <returns>The sequence of transforms applied to the target drawable.</returns>
        TransformSequence<TDrawable> ApplyTransforms<TDrawable>(TDrawable d)
            where TDrawable : Drawable, IFlippable, IVectorScalable;
    }
}
