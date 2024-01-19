// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components
{
    /// <summary>
    /// Base handler for editor scale operations.
    /// </summary>
    public partial class SelectionScaleHandler : Component
    {
        /// <summary>
        /// Whether the scale can currently be performed.
        /// </summary>
        public Bindable<bool> CanScale { get; private set; } = new BindableBool();

        public Quad? OriginalSurroundingQuad { get; protected set; }

        /// <summary>
        /// Performs a single, instant, atomic scale operation.
        /// </summary>
        /// <remarks>
        /// This method is intended to be used in atomic contexts (such as when pressing a single button).
        /// For continuous operations, see the <see cref="Begin"/>-<see cref="Update"/>-<see cref="Commit"/> flow.
        /// </remarks>
        /// <param name="scale">The scale to apply, as multiplier.</param>
        /// <param name="origin">
        /// The origin point to scale from.
        /// If the default <see langword="null"/> value is supplied, a sane implementation-defined default will be used.
        /// </param>
        public void ScaleSelection(Vector2 scale, Vector2? origin = null)
        {
            Begin();
            Update(scale, origin);
            Commit();
        }

        /// <summary>
        /// Begins a continuous scale operation.
        /// </summary>
        /// <remarks>
        /// This flow is intended to be used when a scale operation is made incrementally (such as when dragging a scale handle or slider).
        /// For instantaneous, atomic operations, use the convenience <see cref="ScaleSelection"/> method.
        /// </remarks>
        public virtual void Begin()
        {
        }

        /// <summary>
        /// Updates a continuous scale operation.
        /// Must be preceded by a <see cref="Begin"/> call.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This flow is intended to be used when a scale operation is made incrementally (such as when dragging a scale handle or slider).
        /// As such, the values of <paramref name="scale"/> and <paramref name="origin"/> supplied should be relative to the state of the objects being scaled
        /// when <see cref="Begin"/> was called, rather than instantaneous deltas.
        /// </para>
        /// <para>
        /// For instantaneous, atomic operations, use the convenience <see cref="ScaleSelection"/> method.
        /// </para>
        /// </remarks>
        /// <param name="scale">The Scale to apply, as multiplier.</param>
        /// <param name="origin">
        /// The origin point to scale from.
        /// If the default <see langword="null"/> value is supplied, a sane implementation-defined default will be used.
        /// </param>
        public virtual void Update(Vector2 scale, Vector2? origin = null)
        {
        }

        /// <summary>
        /// Ends a continuous scale operation.
        /// Must be preceded by a <see cref="Begin"/> call.
        /// </summary>
        /// <remarks>
        /// This flow is intended to be used when a scale operation is made incrementally (such as when dragging a scale handle or slider).
        /// For instantaneous, atomic operations, use the convenience <see cref="ScaleSelection"/> method.
        /// </remarks>
        public virtual void Commit()
        {
        }
    }
}
