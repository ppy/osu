// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;
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

        private readonly IBindable<WorkingBeatmap> beatmap = new Bindable<WorkingBeatmap>();

        protected HitObjectComposer(Ruleset ruleset)
        {
            this.ruleset = ruleset;

            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(IBindableBeatmap beatmap, IFrameBasedClock framedClock)
        {
            this.beatmap.BindTo(beatmap);

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

        protected override void LoadComplete()
        {
            base.LoadComplete();

            rulesetContainer.Playfield.DisplayJudgements.Value = false;
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

        protected virtual RulesetContainer CreateRulesetContainer(Ruleset ruleset, WorkingBeatmap beatmap) => ruleset.CreateRulesetContainerWith(beatmap);

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
