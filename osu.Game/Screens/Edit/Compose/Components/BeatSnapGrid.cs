// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components
{
    public abstract class BeatSnapGrid : CompositeDrawable
    {
        protected double Velocity { get; private set; }

        protected float DistanceSpacing { get; private set; }

        [Resolved]
        private IFrameBasedClock framedClock { get; set; }

        [Resolved]
        private IEditorBeatmap beatmap { get; set; }

        [Resolved]
        private BindableBeatDivisor beatDivisor { get; set; }

        [Resolved]
        private OsuColour colours { get; set; }

        private readonly Cached gridCache = new Cached();
        private readonly HitObject hitObject;
        private readonly Vector2 startPosition;

        private double startTime;
        private double beatLength;

        protected BeatSnapGrid(HitObject hitObject, Vector2 startPosition)
        {
            this.hitObject = hitObject;
            this.startPosition = startPosition;

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
                CreateGrid(startPosition);
                gridCache.Validate();
            }
        }

        /// <summary>
        /// Draws the grid.
        /// </summary>
        protected abstract void CreateGrid(Vector2 startPosition);

        /// <summary>
        /// Retrieves the velocity of gameplay at a time.
        /// </summary>
        /// <param name="time">The time to retrieve the velocity at.</param>
        /// <param name="controlPointInfo">The beatmap's <see cref="ControlPointInfo"/> at the requested time.</param>
        /// <param name="difficulty">The beatmap's <see cref="BeatmapDifficulty"/> at the requested time.</param>
        /// <returns>The velocity in pixels per millisecond.</returns>
        protected abstract float GetVelocity(double time, ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty);

        /// <summary>
        /// Snaps a position to this grid.
        /// </summary>
        /// <param name="screenSpacePosition">The original screen-space position.</param>
        /// <returns>The snapped position.</returns>
        public abstract Vector2 GetSnapPosition(Vector2 screenSpacePosition);

        /// <summary>
        /// Retrieves the time at a snapped position.
        /// </summary>
        /// <param name="snappedPosition">The snapped position.</param>
        /// <returns>The time at the snapped position.</returns>
        public double GetSnapTime(Vector2 snappedPosition) => startTime + (ToLocalSpace(snappedPosition) - startPosition).Length / Velocity;

        /// <summary>
        /// Retrieves the applicable colour for a beat index.
        /// </summary>
        /// <param name="index">The 0-based beat index.</param>
        /// <returns>The applicable colour.</returns>
        protected ColourInfo GetColourForBeatIndex(int index)
        {
            int divIndex = beatDivisor.Value - (index % beatDivisor.Value) - 1;

            ColourInfo colour = colours.Gray5;

            {
                for (int i = 0; i < BindableBeatDivisor.VALID_DIVISORS.Length; i++)
                {
                    int divisor = BindableBeatDivisor.VALID_DIVISORS[i];

                    if ((divIndex * divisor) % beatDivisor.Value == 0)
                    {
                        colour = BindableBeatDivisor.GetColourFor(divisor, colours);
                        break;
                    }
                }
            }

            int repeatIndex = index / beatDivisor.Value;
            return colour.MultiplyAlpha(0.5f / (repeatIndex + 1));
        }
    }
}
