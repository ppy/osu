// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components
{
    /// <summary>
    /// A grid which takes user input and returns a quantized ("snapped") position and time.
    /// </summary>
    public abstract class DistanceSnapGrid : CompositeDrawable
    {
        /// <summary>
        /// The velocity of the beatmap at the point of placement in pixels per millisecond.
        /// </summary>
        protected double Velocity { get; private set; }

        /// <summary>
        /// The spacing between each tick of the beat snapping grid.
        /// </summary>
        protected float DistanceSpacing { get; private set; }

        /// <summary>
        /// The position which the grid is centred on.
        /// The first beat snapping tick is located at <see cref="CentrePosition"/> + <see cref="DistanceSpacing"/> in the desired direction.
        /// </summary>
        protected readonly Vector2 CentrePosition;

        [Resolved]
        protected OsuColour Colours { get; private set; }

        [Resolved]
        private IEditorBeatmap beatmap { get; set; }

        [Resolved]
        private BindableBeatDivisor beatDivisor { get; set; }

        private readonly Cached gridCache = new Cached();
        private readonly HitObject hitObject;

        private double startTime;
        private double beatLength;

        protected DistanceSnapGrid(HitObject hitObject, Vector2 centrePosition)
        {
            this.hitObject = hitObject;
            this.CentrePosition = centrePosition;

            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            startTime = (hitObject as IHasEndTime)?.EndTime ?? hitObject.StartTime;
            beatLength = beatmap.ControlPointInfo.TimingPointAt(startTime).BeatLength;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            beatDivisor.BindValueChanged(_ => updateSpacing(), true);
        }

        private void updateSpacing()
        {
            Velocity = GetVelocity(startTime, beatmap.ControlPointInfo, beatmap.BeatmapInfo.BaseDifficulty);
            DistanceSpacing = (float)(beatLength / beatDivisor.Value * Velocity);
            gridCache.Invalidate();
        }

        public override bool Invalidate(Invalidation invalidation = Invalidation.All, Drawable source = null, bool shallPropagate = true)
        {
            if ((invalidation & Invalidation.RequiredParentSizeToFit) > 0)
                gridCache.Invalidate();

            return base.Invalidate(invalidation, source, shallPropagate);
        }

        protected override void Update()
        {
            base.Update();

            if (!gridCache.IsValid)
            {
                ClearInternal();
                CreateContent(CentrePosition);
                gridCache.Validate();
            }
        }

        /// <summary>
        /// Creates the content which visualises the grid ticks.
        /// </summary>
        protected abstract void CreateContent(Vector2 centrePosition);

        /// <summary>
        /// Retrieves the velocity of gameplay at a point in time in pixels per millisecond.
        /// </summary>
        /// <param name="time">The time to retrieve the velocity at.</param>
        /// <param name="controlPointInfo">The beatmap's <see cref="ControlPointInfo"/> at the point in time.</param>
        /// <param name="difficulty">The beatmap's <see cref="BeatmapDifficulty"/> at the point in time.</param>
        /// <returns>The velocity.</returns>
        protected abstract float GetVelocity(double time, ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty);

        /// <summary>
        /// Snaps a position to this grid.
        /// </summary>
        /// <param name="position">The original position in coordinate space local to this <see cref="DistanceSnapGrid"/>.</param>
        /// <returns>The snapped position in coordinate space local to this <see cref="DistanceSnapGrid"/>.</returns>
        public abstract Vector2 GetSnapPosition(Vector2 position);

        /// <summary>
        /// Retrieves the time at a snapped position.
        /// </summary>
        /// <param name="position">The snapped position in coordinate space local to this <see cref="DistanceSnapGrid"/>.</param>
        /// <returns>The time at the snapped position.</returns>
        public double GetSnapTime(Vector2 position) => startTime + (position - CentrePosition).Length / Velocity;

        /// <summary>
        /// Retrieves the applicable colour for a beat index.
        /// </summary>
        /// <param name="index">The 0-based beat index.</param>
        /// <returns>The applicable colour.</returns>
        protected ColourInfo GetColourForBeatIndex(int index)
        {
            int beat = (index + 1) % beatDivisor.Value;
            ColourInfo colour = Colours.Gray5;

            for (int i = 0; i < BindableBeatDivisor.VALID_DIVISORS.Length; i++)
            {
                int divisor = BindableBeatDivisor.VALID_DIVISORS[i];

                if ((beat * divisor) % beatDivisor.Value == 0)
                {
                    colour = BindableBeatDivisor.GetColourFor(divisor, Colours);
                    break;
                }
            }

            int repeatIndex = index / beatDivisor.Value;
            return colour.MultiplyAlpha(0.5f / (repeatIndex + 1));
        }
    }
}
