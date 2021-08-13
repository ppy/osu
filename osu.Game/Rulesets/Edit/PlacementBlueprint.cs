// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Edit
{
    /// <summary>
    /// A blueprint which governs the creation of a new <see cref="HitObject"/> to actualisation.
    /// </summary>
    public abstract class PlacementBlueprint : CompositeDrawable
    {
        /// <summary>
        /// Whether the <see cref="HitObject"/> is currently mid-placement, but has not necessarily finished being placed.
        /// </summary>
        public PlacementState PlacementActive { get; private set; }

        /// <summary>
        /// The <see cref="HitObject"/> that is being placed.
        /// </summary>
        public readonly HitObject HitObject;

        [Resolved(canBeNull: true)]
        protected EditorClock EditorClock { get; private set; }

        [Resolved]
        private EditorBeatmap beatmap { get; set; }

        private Bindable<double> startTimeBindable;

        [Resolved]
        private IPlacementHandler placementHandler { get; set; }

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
        /// <param name="commit">Whether the object should be committed.</param>
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

            placementHandler.EndPlacement(HitObject, commit);
            PlacementActive = PlacementState.Finished;
        }

        /// <summary>
        /// Updates the time and position of this <see cref="PlacementBlueprint"/> based on the provided snap information.
        /// </summary>
        /// <param name="result">The snap result information.</param>
        public virtual void UpdateTimeAndPosition(SnapResult result)
        {
            if (PlacementActive == PlacementState.Waiting)
                HitObject.StartTime = result.Time ?? EditorClock?.CurrentTime ?? Time.Current;
        }

        /// <summary>
        /// Invokes <see cref="Objects.HitObject.ApplyDefaults(ControlPointInfo,BeatmapDifficulty, CancellationToken)"/>,
        /// refreshing <see cref="Objects.HitObject.NestedHitObjects"/> and parameters for the <see cref="HitObject"/>.
        /// </summary>
        protected void ApplyDefaultsToHitObject() => HitObject.ApplyDefaults(beatmap.ControlPointInfo, beatmap.BeatmapInfo.BaseDifficulty);

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => Parent?.ReceivePositionalInputAt(screenSpacePos) ?? false;

        protected override bool Handle(UIEvent e)
        {
            base.Handle(e);

            switch (e)
            {
                case ScrollEvent _:
                    return false;

                case DoubleClickEvent _:
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
