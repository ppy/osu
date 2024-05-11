// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components
{
    /// <summary>
    /// Base handler for editor rotation operations.
    /// </summary>
    public partial class SelectionRotationHandler : Component
    {
        /// <summary>
        /// Whether rotation anchored by the selection origin can currently be performed.
        /// </summary>
        public Bindable<bool> CanRotateSelectionOrigin { get; private set; } = new BindableBool();

        /// <summary>
        /// Whether rotation anchored by the center of the playfield can currently be performed.
        /// </summary>
        public Bindable<bool> CanRotatePlayfieldOrigin { get; private set; } = new BindableBool();

        /// <summary>
        /// Performs a single, instant, atomic rotation operation.
        /// </summary>
        /// <remarks>
        /// This method is intended to be used in atomic contexts (such as when pressing a single button).
        /// For continuous operations, see the <see cref="Begin"/>-<see cref="Update"/>-<see cref="Commit"/> flow.
        /// </remarks>
        /// <param name="rotation">Rotation to apply in degrees.</param>
        /// <param name="origin">
        /// The origin point to rotate around.
        /// If the default <see langword="null"/> value is supplied, a sane implementation-defined default will be used.
        /// </param>
        public void Rotate(float rotation, Vector2? origin = null)
        {
            Begin();
            Update(rotation, origin);
            Commit();
        }

        /// <summary>
        /// Begins a continuous rotation operation.
        /// </summary>
        /// <remarks>
        /// This flow is intended to be used when a rotation operation is made incrementally (such as when dragging a rotation handle or slider).
        /// For instantaneous, atomic operations, use the convenience <see cref="Rotate"/> method.
        /// </remarks>
        public virtual void Begin()
        {
        }

        /// <summary>
        /// Updates a continuous rotation operation.
        /// Must be preceded by a <see cref="Begin"/> call.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This flow is intended to be used when a rotation operation is made incrementally (such as when dragging a rotation handle or slider).
        /// As such, the values of <paramref name="rotation"/> and <paramref name="origin"/> supplied should be relative to the state of the objects being rotated
        /// when <see cref="Begin"/> was called, rather than instantaneous deltas.
        /// </para>
        /// <para>
        /// For instantaneous, atomic operations, use the convenience <see cref="Rotate"/> method.
        /// </para>
        /// </remarks>
        /// <param name="rotation">Rotation to apply in degrees.</param>
        /// <param name="origin">
        /// The origin point to rotate around.
        /// If the default <see langword="null"/> value is supplied, a sane implementation-defined default will be used.
        /// </param>
        public virtual void Update(float rotation, Vector2? origin = null)
        {
        }

        /// <summary>
        /// Ends a continuous rotation operation.
        /// Must be preceded by a <see cref="Begin"/> call.
        /// </summary>
        /// <remarks>
        /// This flow is intended to be used when a rotation operation is made incrementally (such as when dragging a rotation handle or slider).
        /// For instantaneous, atomic operations, use the convenience <see cref="Rotate"/> method.
        /// </remarks>
        public virtual void Commit()
        {
        }
    }
}
