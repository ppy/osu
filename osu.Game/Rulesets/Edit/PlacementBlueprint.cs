// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Edit.Compose;
using osuTK;

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
        public bool PlacementActive { get; private set; }

        /// <summary>
        /// The <see cref="HitObject"/> that is being placed.
        /// </summary>
        protected readonly HitObject HitObject;

        protected IClock EditorClock { get; private set; }

        private readonly IBindable<WorkingBeatmap> beatmap = new Bindable<WorkingBeatmap>();

        [Resolved]
        private IPlacementHandler placementHandler { get; set; }

        protected PlacementBlueprint(HitObject hitObject)
        {
            HitObject = hitObject;

            RelativeSizeAxes = Axes.Both;

            // This is required to allow the blueprint's position to be updated via OnMouseMove/Handle
            // on the same frame it is made visible via a PlacementState change.
            AlwaysPresent = true;
        }

        [BackgroundDependencyLoader]
        private void load(IBindable<WorkingBeatmap> beatmap, IAdjustableClock clock)
        {
            this.beatmap.BindTo(beatmap);

            EditorClock = clock;

            ApplyDefaultsToHitObject();
        }

        /// <summary>
        /// Signals that the placement of <see cref="HitObject"/> has started.
        /// </summary>
        /// <param name="startTime">The start time of <see cref="HitObject"/> at the placement point. If null, the current clock time is used.</param>
        /// <param name="commitStart">Whether this call is committing a value for HitObject.StartTime and continuing with further adjustments.</param>
        protected void BeginPlacement(double? startTime = null, bool commitStart = false)
        {
            HitObject.StartTime = startTime ?? EditorClock.CurrentTime;
            placementHandler.BeginPlacement(HitObject);
            PlacementActive |= commitStart;
        }

        /// <summary>
        /// Signals that the placement of <see cref="HitObject"/> has finished.
        /// This will destroy this <see cref="PlacementBlueprint"/>, and add the HitObject.StartTime to the <see cref="Beatmap"/>.
        /// </summary>
        /// <param name="commit">Whether the object should be committed.</param>
        public void EndPlacement(bool commit)
        {
            if (!PlacementActive)
                BeginPlacement();
            placementHandler.EndPlacement(HitObject, commit);
            PlacementActive = false;
        }

        /// <summary>
        /// Updates the position of this <see cref="PlacementBlueprint"/> to a new screen-space position.
        /// </summary>
        /// <param name="screenSpacePosition">The screen-space position.</param>
        public abstract void UpdatePosition(Vector2 screenSpacePosition);

        /// <summary>
        /// Invokes <see cref="Objects.HitObject.ApplyDefaults(ControlPointInfo,BeatmapDifficulty)"/>,
        /// refreshing <see cref="Objects.HitObject.NestedHitObjects"/> and parameters for the <see cref="HitObject"/>.
        /// </summary>
        protected void ApplyDefaultsToHitObject() => HitObject.ApplyDefaults(beatmap.Value.Beatmap.ControlPointInfo, beatmap.Value.Beatmap.BeatmapInfo.BaseDifficulty);

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

                case MouseButtonEvent _:
                    return true;

                default:
                    return false;
            }
        }
    }
}
