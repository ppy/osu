// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Transforms;

namespace osu.Game.Skinning
{
    /// <summary>
    /// This interface is intended to be implemented by skinnable components
    /// which are to take part in a larger coordinated animation sequence.
    /// An example of such a sequence is e.g. the reveal on entry to results screen.
    /// </summary>
    public interface IAnimatableSkinnable
    {
        /// <summary>
        /// The group number to which this skinnable belongs to.
        /// </summary>
        /// <example>
        /// The intent behind this property is to allow sequencing of animatables.
        /// For instance, given four <see cref="IAnimatableSkinnable"/>s with groups 0, 1, 1, and 2 respectively,
        /// the full transition will play out as follows:
        /// <list type="bullet">
        /// <item>The skinnable from group 0 will animate first.</item>
        /// <item>
        /// Once the skinnable from group 0 indicates animation completion,
        /// both skinnables from group 1 will animate.
        /// </item>
        /// <item>
        /// Once the last skinnable from group 1 indicates animation completion,
        /// the skinnable from group 2 will animate.
        /// </item>
        /// </list>
        /// </example>
        int GroupNumber { get; }

        /// <summary>
        /// Sets up this <see cref="IAnimatableSkinnable"/> to animate.
        /// </summary>
        /// <remarks>
        /// The implementation of this method should do the following:
        /// <list type="bullet">
        /// <item>If an appearance adjustment is needed to start animating, it should be applied <b>instantly</b>, disregarding <paramref name="startTime"/>.</item>
        /// <item>
        /// Using <see cref="CompositeDrawable.BeginAbsoluteSequence"/> at <paramref name="startTime"/>,
        /// this skinnable should queue up its animation transforms appropriately.
        /// </item>
        /// <item>
        /// Finally, this method should return a time instant at which this skinnable's animation is considered to be completed
        /// and the next one can proceed (most often, this is <see cref="Transformable.LatestTransformEndTime"/>).
        /// </item>
        /// </list>
        /// It can also be advisable to call <see cref="FinishAnimating"/> at the start of the implementation of this method,
        /// to ensure no cross-pollution of initial transform state from previous yet-to-complete transforms.
        /// </remarks>
        /// <param name="startTime">The time instant at which the relevant animation sequence to this drawable is to start playing.</param>
        double StartAnimating(double startTime);

        /// <summary>
        /// Instantly finishes any animations started via <see cref="StartAnimating"/>.
        /// </summary>
        void FinishAnimating();
    }
}
