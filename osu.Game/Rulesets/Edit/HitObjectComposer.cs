// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Graphics;
using osu.Framework.Allocation;
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

            ScalableContainer createLayerContainerWithContent(Drawable content)
            {
                var container = CreateLayerContainer();
                container.Child = content;
                layerContainers.Add(container);
                return container;
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
                                createLayerContainerWithContent(new Container
                                {
                                    Name = "Border",
                                    RelativeSizeAxes = Axes.Both,
                                    Masking = true,
                                    BorderColour = Color4.White,
                                    BorderThickness = 2,
                                    Child = new Box { RelativeSizeAxes = Axes.Both, Alpha = 0, AlwaysPresent = true }
                                }),
                                rulesetContainer,
                                createLayerContainerWithContent(new SelectionLayer(rulesetContainer.Playfield))
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
    }
}
