// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Edit
{
    /// <summary>
    /// A blueprint which governs the creation of a new <see cref="HitObject"/> to actualisation.
    /// </summary>
    public abstract partial class PlacementBlueprint : CompositeDrawable, IKeyBindingHandler<GlobalAction>
    {
        /// <summary>
        /// Whether the <see cref="HitObject"/> is currently mid-placement, but has not necessarily finished being placed.
        /// </summary>
        public PlacementState PlacementActive { get; private set; }

        /// <summary>
        /// Whether the sample bank should be taken from the previous hit object.
        /// </summary>
        public bool AutomaticBankAssignment { get; set; }

        /// <summary>
        /// The <see cref="HitObject"/> that is being placed.
        /// </summary>
        public readonly HitObject HitObject;

        [Resolved]
        protected EditorClock EditorClock { get; private set; } = null!;

        [Resolved]
        private EditorBeatmap beatmap { get; set; } = null!;

        private Bindable<double> startTimeBindable = null!;

        private HitObject? getPreviousHitObject() => beatmap.HitObjects.TakeWhile(h => h.StartTime <= startTimeBindable.Value).LastOrDefault();

        [Resolved]
        private IPlacementHandler placementHandler { get; set; } = null!;

        /// <summary>
        /// Whether this blueprint is currently in a state that can be committed.
        /// </summary>
        /// <remarks>
        /// Override this with any preconditions that should be double-checked on committing.
        /// If <c>false</c> is returned and a commit is attempted, the blueprint will be destroyed instead.
        /// </remarks>
        protected virtual bool IsValidForPlacement => true;

        protected PlacementBlueprint(HitObject hitObject)
        {
            HitObject = hitObject;

            // adding the default hit sample should be the case regardless of the ruleset.
            HitObject.Samples.Add(new HitSampleInfo(HitSampleInfo.HIT_NORMAL));

            RelativeSizeAxes = Axes.Both;

            // This is required to allow the blueprint's position to be updated via OnMouseMove/Handle
            // on the same frame it is made visible via a PlacementState change.
            AlwaysPresent = true;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            startTimeBindable = HitObject.StartTimeBindable.GetBoundCopy();
            startTimeBindable.BindValueChanged(_ => ApplyDefaultsToHitObject(), true);
        }

        /// <summary>
        /// Signals that the placement of <see cref="HitObject"/> has started.
        /// </summary>
        /// <param name="commitStart">Whether this call is committing a value for HitObject.StartTime and continuing with further adjustments.</param>
        protected void BeginPlacement(bool commitStart = false)
        {
            placementHandler.BeginPlacement(HitObject);
            if (commitStart)
                PlacementActive = PlacementState.Active;
        }

        /// <summary>
        /// Signals that the placement of <see cref="HitObject"/> has finished.
        /// This will destroy this <see cref="PlacementBlueprint"/>, and add the HitObject.StartTime to the <see cref="Beatmap"/>.
        /// </summary>
        /// <param name="commit">Whether the object should be committed. Note that a commit may fail if <see cref="IsValidForPlacement"/> is <c>false</c>.</param>
        public void EndPlacement(bool commit)
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

            placementHandler.EndPlacement(HitObject, IsValidForPlacement && commit);
            PlacementActive = PlacementState.Finished;
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (PlacementActive == PlacementState.Waiting)
                return false;

            switch (e.Action)
            {
                case GlobalAction.Select:
                    EndPlacement(true);
                    return true;

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

        /// <summary>
        /// Updates the time and position of this <see cref="PlacementBlueprint"/> based on the provided snap information.
        /// </summary>
        /// <param name="result">The snap result information.</param>
        public virtual void UpdateTimeAndPosition(SnapResult result)
        {
            if (PlacementActive == PlacementState.Waiting)
            {
                HitObject.StartTime = result.Time ?? EditorClock.CurrentTime;

                if (HitObject is IHasComboInformation comboInformation)
                    comboInformation.UpdateComboInformation(getPreviousHitObject() as IHasComboInformation);
            }

            if (AutomaticBankAssignment)
            {
                // Take the hitnormal sample of the last hit object
                var lastHitNormal = getPreviousHitObject()?.Samples?.FirstOrDefault(o => o.Name == HitSampleInfo.HIT_NORMAL);
                if (lastHitNormal != null)
                    HitObject.Samples[0] = lastHitNormal;
            }
        }

        /// <summary>
        /// Invokes <see cref="Objects.HitObject.ApplyDefaults(ControlPointInfo,IBeatmapDifficultyInfo,CancellationToken)"/>,
        /// refreshing <see cref="Objects.HitObject.NestedHitObjects"/> and parameters for the <see cref="HitObject"/>.
        /// </summary>
        protected void ApplyDefaultsToHitObject() => HitObject.ApplyDefaults(beatmap.ControlPointInfo, beatmap.Difficulty);

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
                    // for now, the one exception we want to allow is when using a non-main mouse button when shift is pressed, which is used to trigger object deletion
                    // while in placement mode.
                    return mouse.Button == MouseButton.Left || !mouse.ShiftPressed;

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
