// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Edit
{
    /// <summary>
    /// A blueprint which governs the creation of a new <see cref="HitObject"/> to actualisation.
    /// </summary>
    public abstract partial class HitObjectPlacementBlueprint : PlacementBlueprint
    {
        /// <summary>
        /// Whether the sample bank should be taken from the previous hit object.
        /// </summary>
        public bool AutomaticBankAssignment { get; set; }

        /// <summary>
        /// Whether the sample addition bank should be taken from the previous hit objects.
        /// </summary>
        public bool AutomaticAdditionBankAssignment { get; set; }

        /// <summary>
        /// The <see cref="HitObject"/> that is being placed.
        /// </summary>
        public readonly HitObject HitObject;

        [Resolved]
        protected EditorClock EditorClock { get; private set; } = null!;

        [Resolved]
        private EditorBeatmap beatmap { get; set; } = null!;

        private Bindable<double> startTimeBindable = null!;

        protected override bool IsValidForPlacement => HitObject.StartTime >= beatmap.ControlPointInfo.TimingPoints.FirstOrDefault()?.Time;

        [Resolved]
        private IPlacementHandler placementHandler { get; set; } = null!;

        private PlacementStateManager placementStateManager = null!;

        /// <summary>
        /// Acceptable leniency to account for rounding errors and minor unsnaps that we generally
        /// don't consider a problem, but still need to account for in certain operations.
        /// </summary>
        private const double placement_replace_start_time_leniency_ms = 2;

        protected HitObjectPlacementBlueprint(HitObject hitObject)
        {
            HitObject = hitObject;

            // adding the default hit sample should be the case regardless of the ruleset.
            HitObject.Samples.Add(new HitSampleInfo(HitSampleInfo.HIT_NORMAL));
        }

        /// <summary>
        /// Whether an existing <see cref="Objects.HitObject"/> should be removed because <see cref="HitObject"/> is being placed on top of it.
        /// </summary>
        /// <remarks>
        /// By default, it matches when start times are within ±<see cref="placement_replace_start_time_leniency_ms"/> ms of each other.
        /// </remarks>
        public virtual bool ReplacesExistingObject(HitObject existing)
            => Precision.AlmostEquals(existing.StartTime, HitObject.StartTime, placement_replace_start_time_leniency_ms);

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(placementStateManager = new PlacementStateManager(HitObject));

            startTimeBindable = HitObject.StartTimeBindable.GetBoundCopy();
            startTimeBindable.BindValueChanged(_ => ApplyDefaultsToHitObject(), true);
        }

        private bool placementBegun;

        protected override void BeginPlacement(bool commitStart = false)
        {
            base.BeginPlacement(commitStart);

            if (State.Value == Visibility.Visible)
                placementHandler.ShowPlacement(HitObject);

            placementBegun = true;
        }

        public override void EndPlacement(bool commit)
        {
            base.EndPlacement(commit);

            if (IsValidForPlacement && commit)
                placementHandler.CommitPlacement(HitObject);
            else
                placementHandler.HidePlacement();
        }

        protected override void Update()
        {
            base.Update();

            Colour = IsValidForPlacement ? Colour4.White : Colour4.Red;
        }

        /// <summary>
        /// Updates the time and position of this <see cref="PlacementBlueprint"/>.
        /// </summary>
        public override SnapResult UpdateTimeAndPosition(Vector2 screenSpacePosition, double time)
        {
            if (PlacementActive == PlacementState.Waiting)
                HitObject.StartTime = time;

            placementStateManager.CopyStateFromPreviousObject();

            return new SnapResult(screenSpacePosition, time);
        }

        /// <summary>
        /// Invokes <see cref="Objects.HitObject.ApplyDefaults(ControlPointInfo,IBeatmapDifficultyInfo,CancellationToken)"/>,
        /// refreshing <see cref="Objects.HitObject.NestedHitObjects"/> and parameters for the <see cref="HitObject"/>.
        /// </summary>
        protected void ApplyDefaultsToHitObject() => HitObject.ApplyDefaults(beatmap.ControlPointInfo, beatmap.Difficulty);

        protected override void PopIn()
        {
            base.PopIn();

            if (placementBegun)
                placementHandler.ShowPlacement(HitObject);
        }

        protected override void PopOut()
        {
            base.PopOut();
            placementHandler.HidePlacement();
        }
    }
}
