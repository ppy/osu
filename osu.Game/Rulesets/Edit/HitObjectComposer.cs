// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Logging;
using osu.Framework.MathUtils;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Edit.Screens.Compose;
using osu.Game.Screens.Edit.Screens.Compose.Layers;
using osu.Game.Screens.Edit.Screens.Compose.RadioButtons;

namespace osu.Game.Rulesets.Edit
{
    public abstract class HitObjectComposer : CompositeDrawable
    {
        private readonly Ruleset ruleset;

        protected ICompositionTool CurrentTool { get; private set; }

        private RulesetContainer rulesetContainer;
        private readonly List<Container> layerContainers = new List<Container>();

        private readonly Bindable<WorkingBeatmap> beatmap = new Bindable<WorkingBeatmap>();
        private readonly BindableBeatDivisor beatDivisor = new BindableBeatDivisor();

        private IAdjustableClock adjustableClock;

        protected HitObjectComposer(Ruleset ruleset)
        {
            this.ruleset = ruleset;

            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader(true)]
        private void load([NotNull] OsuGameBase osuGame, [NotNull] IAdjustableClock adjustableClock, [NotNull] IFrameBasedClock framedClock, [CanBeNull] BindableBeatDivisor beatDivisor)
        {
            this.adjustableClock = adjustableClock;

            if (beatDivisor != null)
                this.beatDivisor.BindTo(beatDivisor);

            beatmap.BindTo(osuGame.Beatmap);

            try
            {
                rulesetContainer = CreateRulesetContainer(ruleset, beatmap.Value);
                rulesetContainer.Clock = framedClock;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Could not load beatmap sucessfully!");
                return;
            }

            var layerBelowRuleset = new BorderLayer
            {
                RelativeSizeAxes = Axes.Both,
                Child = CreateLayerContainer()
            };

            var layerAboveRuleset = CreateLayerContainer();
            layerAboveRuleset.Child = new HitObjectMaskLayer(rulesetContainer.Playfield, this);

            layerContainers.Add(layerBelowRuleset);
            layerContainers.Add(layerAboveRuleset);

            RadioButtonCollection toolboxCollection;
            InternalChild = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                Content = new[]
                {
                    new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            Name = "Sidebar",
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Right = 10 },
                            Children = new Drawable[]
                            {
                                new ToolboxGroup { Child = toolboxCollection = new RadioButtonCollection { RelativeSizeAxes = Axes.X } }
                            }
                        },
                        new Container
                        {
                            Name = "Content",
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                layerBelowRuleset,
                                rulesetContainer,
                                layerAboveRuleset
                            }
                        }
                    },
                },
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.Absolute, 200),
                }
            };

            toolboxCollection.Items =
                CompositionTools.Select(t => new RadioButton(t.Name, () => setCompositionTool(t)))
                .Prepend(new RadioButton("Select", () => setCompositionTool(null)))
                .ToList();

            toolboxCollection.Items[0].Select();
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            layerContainers.ForEach(l =>
            {
                l.Anchor = rulesetContainer.Playfield.Anchor;
                l.Origin = rulesetContainer.Playfield.Origin;
                l.Position = rulesetContainer.Playfield.Position;
                l.Size = rulesetContainer.Playfield.Size;
            });
        }

        protected override bool OnWheel(InputState state)
        {
            if (state.Mouse.WheelDelta > 0)
                SeekBackward(true);
            else
                SeekForward(true);
            return true;
        }

        /// <summary>
        /// Seeks the current time one beat-snapped beat-length backwards.
        /// </summary>
        /// <param name="snapped">Whether to snap to the closest beat.</param>
        public void SeekBackward(bool snapped = false) => seek(-1, snapped);

        /// <summary>
        /// Seeks the current time one beat-snapped beat-length forwards.
        /// </summary>
        /// <param name="snapped">Whether to snap to the closest beat.</param>
        public void SeekForward(bool snapped = false) => seek(1, snapped);

        private void seek(int direction, bool snapped)
        {
            var cpi = beatmap.Value.Beatmap.ControlPointInfo;

            var timingPoint = cpi.TimingPointAt(adjustableClock.CurrentTime);
            if (direction < 0 && timingPoint.Time == adjustableClock.CurrentTime)
            {
                // When going backwards and we're at the boundary of two timing points, we compute the seek distance with the timing point which we are seeking into
                int activeIndex = cpi.TimingPoints.IndexOf(timingPoint);
                while (activeIndex > 0 && adjustableClock.CurrentTime == timingPoint.Time)
                    timingPoint = cpi.TimingPoints[--activeIndex];
            }

            double seekAmount = timingPoint.BeatLength / beatDivisor;
            double seekTime = adjustableClock.CurrentTime + seekAmount * direction;

            if (!snapped || cpi.TimingPoints.Count == 0)
            {
                adjustableClock.Seek(seekTime);
                return;
            }

            // We will be snapping to beats within timingPoint
            seekTime -= timingPoint.Time;

            // Determine the index from timingPoint of the closest beat to seekTime, accounting for scrolling direction
            int closestBeat;
            if (direction > 0)
                closestBeat = (int)Math.Floor(seekTime / seekAmount);
            else
                closestBeat = (int)Math.Ceiling(seekTime / seekAmount);

            seekTime = timingPoint.Time + closestBeat * seekAmount;

            // Due to the rounding above, we may end up on the current beat. This will effectively cause 0 seeking to happen, but we don't want this.
            // Instead, we'll go to the next beat in the direction when this is the case
            if (Precision.AlmostEquals(adjustableClock.CurrentTime, seekTime))
            {
                closestBeat += direction > 0 ? 1 : -1;
                seekTime = timingPoint.Time + closestBeat * seekAmount;
            }

            if (seekTime < timingPoint.Time && timingPoint != cpi.TimingPoints.First())
                seekTime = timingPoint.Time;

            var nextTimingPoint = cpi.TimingPoints.FirstOrDefault(t => t.Time > timingPoint.Time);
            if (seekTime > nextTimingPoint?.Time)
                seekTime = nextTimingPoint.Time;

            adjustableClock.Seek(seekTime);
        }

        public void SeekTo(double seekTime, bool snapped = false)
        {
            if (!snapped)
            {
                adjustableClock.Seek(seekTime);
                return;
            }

            var timingPoint = beatmap.Value.Beatmap.ControlPointInfo.TimingPointAt(seekTime);
            double beatSnapLength = timingPoint.BeatLength / beatDivisor;

            // We will be snapping to beats within the timing point
            seekTime -= timingPoint.Time;

            // Determine the index from the current timing point of the closest beat to seekTime
            int closestBeat = (int)Math.Round(seekTime / beatSnapLength);
            seekTime = timingPoint.Time + closestBeat * beatSnapLength;

            // Depending on beatSnapLength, we may snap to a beat that is beyond timingPoint's end time, but we want to instead snap to
            // the next timing point's start time
            var nextTimingPoint = beatmap.Value.Beatmap.ControlPointInfo.TimingPoints.FirstOrDefault(t => t.Time > timingPoint.Time);
            if (seekTime > nextTimingPoint?.Time)
                seekTime = nextTimingPoint.Time;

            adjustableClock.Seek(seekTime);
        }

        private void setCompositionTool(ICompositionTool tool) => CurrentTool = tool;

        protected virtual RulesetContainer CreateRulesetContainer(Ruleset ruleset, WorkingBeatmap beatmap) => ruleset.CreateRulesetContainerWith(beatmap, true);

        protected abstract IReadOnlyList<ICompositionTool> CompositionTools { get; }

        /// <summary>
        /// Creates a <see cref="HitObjectMask"/> for a specific <see cref="DrawableHitObject"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="DrawableHitObject"/> to create the overlay for.</param>
        public virtual HitObjectMask CreateMaskFor(DrawableHitObject hitObject) => null;

        /// <summary>
        /// Creates a <see cref="MaskSelection"/> which outlines <see cref="DrawableHitObject"/>s
        /// and handles hitobject pattern adjustments.
        /// </summary>
        public virtual MaskSelection CreateMaskSelection() => new MaskSelection();

        /// <summary>
        /// Creates a <see cref="ScalableContainer"/> which provides a layer above or below the <see cref="Playfield"/>.
        /// </summary>
        protected virtual ScalableContainer CreateLayerContainer() => new ScalableContainer { RelativeSizeAxes = Axes.Both };
    }
}
