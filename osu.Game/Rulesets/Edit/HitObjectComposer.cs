// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Logging;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit.Layers;
using osu.Game.Rulesets.Edit.Layers.Selection;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.UI;
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
        private IAdjustableClock adjustableClock;

        protected HitObjectComposer(Ruleset ruleset)
        {
            this.ruleset = ruleset;
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase osuGame)
        {
            beatmap.BindTo(osuGame.Beatmap);

            try
            {
                rulesetContainer = CreateRulesetContainer(ruleset, beatmap.Value);

                // TODO: should probably be done at a RulesetContainer level to share logic with Player.
                adjustableClock = (IAdjustableClock)beatmap.Value.Track ?? new StopwatchClock();
                rulesetContainer.Clock = new InterpolatingFramedClock(adjustableClock);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Could not load beatmap sucessfully!");
                return;
            }

            HitObjectOverlayLayer hitObjectOverlayLayer = CreateHitObjectOverlayLayer();
            SelectionLayer selectionLayer = new SelectionLayer(rulesetContainer.Playfield);

            var layerBelowRuleset = new BorderLayer
            {
                RelativeSizeAxes = Axes.Both,
                Child = CreateLayerContainer()
            };

            var layerAboveRuleset = CreateLayerContainer();
            layerAboveRuleset.Children = new Drawable[]
            {
                selectionLayer, // Below object overlays for input
                hitObjectOverlayLayer,
                selectionLayer.CreateProxy() // Proxy above object overlays for selections
            };

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

            selectionLayer.ObjectSelected += hitObjectOverlayLayer.AddOverlay;
            selectionLayer.ObjectDeselected += hitObjectOverlayLayer.RemoveOverlay;

            toolboxCollection.Items =
                new[] { new RadioButton("Select", () => setCompositionTool(null)) }
                .Concat(
                    CompositionTools.Select(t => new RadioButton(t.Name, () => setCompositionTool(t)))
                )
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
            var timingPoint = beatmap.Value.Beatmap.ControlPointInfo.TimingPointAt(adjustableClock.CurrentTime);

            const int beat_snap_divisor = 4; // Todo: This should not be a constant
            double seekAmount = timingPoint.BeatLength / beat_snap_divisor;
            int direction = state.Mouse.WheelDelta > 0 ? -1 : 1;

            // The direction is added to prevent rounding issues by enforcing that abs(unsnappedTime - currentTime) > beatLength
            double unsnappedTime = adjustableClock.CurrentTime + seekAmount * direction + direction;

            // Unsnapped time may be between two beats, so we need to snap it to the closest beat
            int closestBeat;
            if (direction > 0)
                closestBeat = (int)Math.Floor(unsnappedTime / seekAmount);
            else
                closestBeat = (int)Math.Ceiling(unsnappedTime / seekAmount);

            double snappedTime = closestBeat * seekAmount;

            adjustableClock.Seek(snappedTime);
            return true;
        }

        private void setCompositionTool(ICompositionTool tool) => CurrentTool = tool;

        protected virtual RulesetContainer CreateRulesetContainer(Ruleset ruleset, WorkingBeatmap beatmap) => ruleset.CreateRulesetContainerWith(beatmap, true);

        protected abstract IReadOnlyList<ICompositionTool> CompositionTools { get; }

        /// <summary>
        /// Creates a <see cref="ScalableContainer"/> which provides a layer above or below the <see cref="Playfield"/>.
        /// </summary>
        protected virtual ScalableContainer CreateLayerContainer() => new ScalableContainer { RelativeSizeAxes = Axes.Both };

        /// <summary>
        /// Creates the <see cref="HitObjectOverlayLayer"/> which overlays selected <see cref="DrawableHitObject"/>s.
        /// </summary>
        protected virtual HitObjectOverlayLayer CreateHitObjectOverlayLayer() => new HitObjectOverlayLayer();
    }
}
