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

        protected HitObjectComposer(Ruleset ruleset)
        {
            this.ruleset = ruleset;

            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase osuGame)
        {
            RulesetContainer rulesetContainer;
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
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            BorderColour = Color4.White,
                            BorderThickness = 2,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Alpha = 0,
                                    AlwaysPresent = true,
                                },
                                rulesetContainer,
                                new SelectionLayer(rulesetContainer.Playfield)
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

        private void setCompositionTool(ICompositionTool tool) => CurrentTool = tool;

        protected virtual RulesetContainer CreateRulesetContainer(Ruleset ruleset, WorkingBeatmap beatmap) => ruleset.CreateRulesetContainerWith(beatmap, true);

        protected abstract IReadOnlyList<ICompositionTool> CompositionTools { get; }
    }
}
