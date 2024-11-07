// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets.Objects;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Edit
{
    /// <summary>
    /// A blueprint which governs the placement of something.
    /// </summary>
    public abstract partial class PlacementBlueprint : CompositeDrawable, IKeyBindingHandler<GlobalAction>
    {
        /// <summary>
        /// Whether the <see cref="HitObject"/> is currently mid-placement, but has not necessarily finished being placed.
        /// </summary>
        public PlacementState PlacementActive { get; private set; }

        /// <summary>
        /// Whether this blueprint is currently in a state that can be committed.
        /// </summary>
        /// <remarks>
        /// Override this with any preconditions that should be double-checked on committing.
        /// If <c>false</c> is returned and a commit is attempted, the blueprint will be destroyed instead.
        /// </remarks>
        protected virtual bool IsValidForPlacement => true;

        protected PlacementBlueprint()
        {
            RelativeSizeAxes = Axes.Both;

            // This is required to allow the blueprint's position to be updated via OnMouseMove/Handle
            // on the same frame it is made visible via a PlacementState change.
            AlwaysPresent = true;
        }

        /// <summary>
        /// Signals that the placement has started.
        /// </summary>
        /// <param name="commitStart">Whether this call is committing a value and continuing with further adjustments.</param>
        protected virtual void BeginPlacement(bool commitStart = false)
        {
            if (commitStart)
                PlacementActive = PlacementState.Active;
        }

        /// <summary>
        /// Signals that the placement of <see cref="HitObject"/> has finished.
        /// This will destroy this <see cref="PlacementBlueprint"/>, and commit the changes.
        /// </summary>
        /// <param name="commit">Whether the changes should be committed. Note that a commit may fail if <see cref="IsValidForPlacement"/> is <c>false</c>.</param>
        public virtual void EndPlacement(bool commit)
        {
            switch (PlacementActive)
            {
                case PlacementState.Finished:
                    return;

                case PlacementState.Waiting:
                    // ensure placement was started before ending to make state handling simpler.
                    BeginPlacement();
                    break;
            }

            PlacementActive = PlacementState.Finished;
        }

        /// <summary>
        /// Determines which objects to snap to for the snap result in <see cref="UpdateTimeAndPosition"/>.
        /// </summary>
        public virtual SnapType SnapType => SnapType.All;

        /// <summary>
        /// Updates the time and position of this <see cref="PlacementBlueprint"/> based on the provided snap information.
        /// </summary>
        /// <param name="result">The snap result information.</param>
        public virtual void UpdateTimeAndPosition(SnapResult result)
        {
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (PlacementActive == PlacementState.Waiting)
                return false;

            switch (e.Action)
            {
                case GlobalAction.Back:
                    EndPlacement(false);
                    return true;

                default:
                    return false;
            }
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

        protected override bool Handle(UIEvent e)
        {
            base.Handle(e);

            switch (e)
            {
                case ScrollEvent:
                    return false;

                case DoubleClickEvent:
                    return false;

                case MouseButtonEvent mouse:
                    // placement blueprints should generally block mouse from reaching underlying components (ie. performing clicks on interface buttons).
                    return mouse.Button == MouseButton.Left || PlacementActive == PlacementState.Active;

                default:
                    return false;
            }
        }

        public enum PlacementState
        {
            Waiting,
            Active,
            Finished
        }
    }
}
