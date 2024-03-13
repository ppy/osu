// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Layout;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit.Compose.Components
{
    /// <summary>
    /// A grid which takes user input and returns a quantized ("snapped") position and time.
    /// </summary>
    public abstract partial class DistanceSnapGrid : CompositeDrawable
    {
        /// <summary>
        /// The spacing between each tick of the beat snapping grid.
        /// </summary>
        protected float DistanceBetweenTicks { get; private set; }

        protected IBindable<double> DistanceSpacingMultiplier { get; private set; }

        /// <summary>
        /// The maximum number of distance snapping intervals allowed.
        /// </summary>
        protected int MaxIntervals { get; private set; }

        /// <summary>
        /// The position which the grid should start.
        /// The first beat snapping tick is located at <see cref="StartPosition"/> + <see cref="DistanceBetweenTicks"/> away from this point.
        /// </summary>
        protected readonly Vector2 StartPosition;

        /// <summary>
        /// The snapping time at <see cref="StartPosition"/>.
        /// </summary>
        protected readonly double StartTime;

        protected readonly double? LatestEndTime;

        [Resolved]
        protected OsuColour Colours { get; private set; }

        [Resolved]
        protected IDistanceSnapProvider SnapProvider { get; private set; }

        [Resolved]
        protected EditorBeatmap Beatmap { get; private set; }

        [Resolved]
        private BindableBeatDivisor beatDivisor { get; set; }

        /// <summary>
        /// When enabled, distance snap should only snap to the current time (as per the editor clock).
        /// This is to emulate stable behaviour.
        /// </summary>
        protected Bindable<bool> LimitedDistanceSnap { get; private set; }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            LimitedDistanceSnap = config.GetBindable<bool>(OsuSetting.EditorLimitedDistanceSnap);
        }

        private readonly LayoutValue gridCache = new LayoutValue(Invalidation.RequiredParentSizeToFit);

        protected readonly HitObject ReferenceObject;

        /// <summary>
        /// Creates a new <see cref="DistanceSnapGrid"/>.
        /// </summary>
        /// <param name="referenceObject">A reference object to gather relevant difficulty values from.</param>
        /// <param name="startPosition">The position at which the grid should start. The first tick is located one distance spacing length away from this point.</param>
        /// <param name="startTime">The snapping time at <see cref="StartPosition"/>.</param>
        /// <param name="endTime">The time at which the snapping grid should end. If null, the grid will continue until the bounds of the screen are exceeded.</param>
        protected DistanceSnapGrid(HitObject referenceObject, Vector2 startPosition, double startTime, double? endTime = null)
        {
            ReferenceObject = referenceObject;
            LatestEndTime = endTime;

            StartPosition = startPosition;
            StartTime = startTime;

            RelativeSizeAxes = Axes.Both;

            AddLayout(gridCache);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            beatDivisor.BindValueChanged(_ => updateSpacing());

            DistanceSpacingMultiplier = SnapProvider.DistanceSpacingMultiplier.GetBoundCopy();
            DistanceSpacingMultiplier.BindValueChanged(_ => updateSpacing(), true);
        }

        private void updateSpacing()
        {
            float distanceSpacingMultiplier = (float)DistanceSpacingMultiplier.Value;
            float beatSnapDistance = SnapProvider.GetBeatSnapDistanceAt(ReferenceObject, false);

            DistanceBetweenTicks = beatSnapDistance * distanceSpacingMultiplier;

            if (LatestEndTime == null)
                MaxIntervals = int.MaxValue;
            else
                MaxIntervals = (int)((LatestEndTime.Value - StartTime) / SnapProvider.DistanceToDuration(ReferenceObject, beatSnapDistance));

            gridCache.Invalidate();
        }

        protected override void Update()
        {
            base.Update();

            if (!gridCache.IsValid)
            {
                ClearInternal();
                CreateContent();
                gridCache.Validate();
            }
        }

        /// <summary>
        /// Creates the content which visualises the grid ticks.
        /// </summary>
        protected abstract void CreateContent();

        /// <summary>
        /// Snaps a position to this grid.
        /// </summary>
        /// <param name="position">The original position in coordinate space local to this <see cref="DistanceSnapGrid"/>.</param>
        /// <returns>A tuple containing the snapped position in coordinate space local to this <see cref="DistanceSnapGrid"/> and the respective time value.</returns>
        public abstract (Vector2 position, double time) GetSnappedPosition(Vector2 position);

        /// <summary>
        /// Retrieves the applicable colour for a beat index.
        /// </summary>
        /// <param name="placementIndex">The 0-based beat index from the point of placement.</param>
        /// <returns>The applicable colour.</returns>
        protected Color4 GetColourForIndexFromPlacement(int placementIndex)
        {
            var timingPoint = Beatmap.ControlPointInfo.TimingPointAt(StartTime);
            double beatLength = timingPoint.BeatLength / beatDivisor.Value;
            int beatIndex = (int)Math.Round((StartTime - timingPoint.Time) / beatLength);

            var colour = BindableBeatDivisor.GetColourFor(BindableBeatDivisor.GetDivisorForBeatIndex(beatIndex + placementIndex + 1, beatDivisor.Value), Colours);

            int repeatIndex = placementIndex / beatDivisor.Value;
            return colour.Opacity(0.5f / (repeatIndex + 1));
        }
    }
}
