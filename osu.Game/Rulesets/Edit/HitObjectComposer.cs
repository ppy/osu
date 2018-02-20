// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Logging;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
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
        private readonly ScalableContainer[] layerContainers = new ScalableContainer[2];

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
            }
            catch (Exception e)
            {
                Logger.Error(e, "Could not load beatmap sucessfully!");
                return;
            }

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
                                createBottomLayer(),
                                rulesetContainer,
                                createTopLayer()
                            }
                        }
                    },
                },
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.Absolute, 200),
                }
            };

            rulesetContainer.Clock = new InterpolatingFramedClock((IAdjustableClock)osuGame.Beatmap.Value.Track ?? new StopwatchClock());

            toolboxCollection.Items =
                new[] { new RadioButton("Select", () => setCompositionTool(null)) }
                .Concat(
                    CompositionTools.Select(t => new RadioButton(t.Name, () => setCompositionTool(t)))
                )
                .ToList();

            toolboxCollection.Items[0].Select();
        }

        private ScalableContainer createBottomLayer()
        {
            layerContainers[0] = CreateLayerContainer();
            layerContainers[0].Child = new Container
            {
                Name = "Border",
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                BorderColour = Color4.White,
                BorderThickness = 2,
                Child = new Box { RelativeSizeAxes = Axes.Both, Alpha = 0, AlwaysPresent = true }
            };

            return layerContainers[0];
        }

        private ScalableContainer createTopLayer()
        {
            var overlayLayer = CreateHitObjectOverlayLayer();
            var selectionLayer = new SelectionLayer(rulesetContainer.Playfield);

            selectionLayer.ObjectSelected += overlayLayer.AddOverlay;
            selectionLayer.ObjectDeselected += overlayLayer.RemoveOverlay;

            layerContainers[1] = CreateLayerContainer();
            layerContainers[1].Children = new Drawable[]
            {
                overlayLayer,
                selectionLayer,
            };

            return layerContainers[1];
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
        protected virtual ScalableContainer CreateLayerContainer() => new ScalableContainer();

        /// <summary>
        /// Creates the <see cref="HitObjectOverlayLayer"/> which overlays selected <see cref="DrawableHitObject"/>s.
        /// </summary>
        protected virtual HitObjectOverlayLayer CreateHitObjectOverlayLayer() => new HitObjectOverlayLayer();
    }
}
