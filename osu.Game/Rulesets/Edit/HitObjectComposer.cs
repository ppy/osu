// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
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

        protected HitObjectComposer(Ruleset ruleset)
        {
            this.ruleset = ruleset;
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase osuGame)
        {
            try
            {
                rulesetContainer = CreateRulesetContainer(ruleset, osuGame.Beatmap.Value);

                // TODO: should probably be done at a RulesetContainer level to share logic with Player.
                rulesetContainer.Clock = new InterpolatingFramedClock((IAdjustableClock)osuGame.Beatmap.Value.Track ?? new StopwatchClock());
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
