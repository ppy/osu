// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Rulesets.Edit;
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
        /// The spacing between each tick of the beat snapping grid.
        /// </summary>
        protected float DistanceSpacing { get; private set; }

        /// <summary>
        /// The snapping time at <see cref="CentrePosition"/>.
        /// </summary>
        protected double StartTime { get; private set; }

        /// <summary>
        /// The position which the grid is centred on.
        /// The first beat snapping tick is located at <see cref="CentrePosition"/> + <see cref="DistanceSpacing"/> in the desired direction.
        /// </summary>
        protected readonly Vector2 CentrePosition;

        [Resolved]
        protected OsuColour Colours { get; private set; }

        [Resolved]
        protected IDistanceSnapProvider SnapProvider { get; private set; }

        [Resolved]
        private IEditorBeatmap beatmap { get; set; }

        [Resolved]
        private BindableBeatDivisor beatDivisor { get; set; }

        private readonly Cached gridCache = new Cached();
        private readonly HitObject hitObject;

        protected DistanceSnapGrid(HitObject hitObject, Vector2 centrePosition)
        {
            this.hitObject = hitObject;

            CentrePosition = centrePosition;
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            StartTime = (hitObject as IHasEndTime)?.EndTime ?? hitObject.StartTime;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            beatDivisor.BindValueChanged(_ => updateSpacing(), true);
        }

        private void updateSpacing()
        {
            DistanceSpacing = SnapProvider.GetBeatSnapDistanceAt(StartTime);
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
        /// Snaps a position to this grid.
        /// </summary>
        /// <param name="position">The original position in coordinate space local to this <see cref="DistanceSnapGrid"/>.</param>
        /// <returns>A tuple containing the snapped position in coordinate space local to this <see cref="DistanceSnapGrid"/> and the respective time value.</returns>
        public abstract (Vector2 position, double time) GetSnappedPosition(Vector2 position);

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
