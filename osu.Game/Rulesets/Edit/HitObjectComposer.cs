// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
using osu.Game.Rulesets.Configuration;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Edit.Components.RadioButtons;
using osu.Game.Screens.Edit.Compose.Components;

namespace osu.Game.Rulesets.Edit
{
    public abstract class HitObjectComposer : CompositeDrawable
    {
        public IEnumerable<DrawableHitObject> HitObjects => RulesetContainer.Playfield.AllHitObjects;

        protected readonly Ruleset Ruleset;

        protected readonly IBindable<WorkingBeatmap> Beatmap = new Bindable<WorkingBeatmap>();

        protected IRulesetConfigManager Config { get; private set; }

        private readonly List<Container> layerContainers = new List<Container>();

        protected EditRulesetContainer RulesetContainer { get; private set; }

        private BlueprintContainer blueprintContainer;

        private InputManager inputManager;

        internal HitObjectComposer(Ruleset ruleset)
        {
            Ruleset = ruleset;

            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(IBindableBeatmap beatmap, IFrameBasedClock framedClock)
        {
            Beatmap.BindTo(beatmap);

            try
            {
                RulesetContainer = CreateRulesetContainer();
                RulesetContainer.Clock = framedClock;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Could not load beatmap sucessfully!");
                return;
            }

            var layerBelowRuleset = CreateLayerContainer();
            layerBelowRuleset.Child = new EditorPlayfieldBorder { RelativeSizeAxes = Axes.Both };

            var layerAboveRuleset = CreateLayerContainer();
            layerAboveRuleset.Child = blueprintContainer = new BlueprintContainer();

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
                                RulesetContainer,
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
                CompositionTools.Select(t => new RadioButton(t.Name, () => blueprintContainer.CurrentTool = t))
                .Prepend(new RadioButton("Select", () => blueprintContainer.CurrentTool = null))
                .ToList();

            toolboxCollection.Items[0].Select();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            inputManager = GetContainingInputManager();
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

            dependencies.CacheAs(this);
            Config = dependencies.Get<RulesetConfigCache>().GetConfigFor(Ruleset);

            return dependencies;
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            layerContainers.ForEach(l =>
            {
                l.Anchor = RulesetContainer.Playfield.Anchor;
                l.Origin = RulesetContainer.Playfield.Origin;
                l.Position = RulesetContainer.Playfield.Position;
                l.Size = RulesetContainer.Playfield.Size;
            });
        }

        /// <summary>
        /// Whether the user's cursor is currently in an area of the <see cref="HitObjectComposer"/> that is valid for placement.
        /// </summary>
        public virtual bool CursorInPlacementArea => RulesetContainer.Playfield.ReceivePositionalInputAt(inputManager.CurrentState.Mouse.Position);

        /// <summary>
        /// Adds a <see cref="HitObject"/> to the <see cref="Beatmaps.Beatmap"/> and visualises it.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> to add.</param>
        public void Add(HitObject hitObject) => blueprintContainer.AddBlueprintFor(RulesetContainer.Add(hitObject));

        public void Remove(HitObject hitObject) => blueprintContainer.RemoveBlueprintFor(RulesetContainer.Remove(hitObject));

        internal abstract EditRulesetContainer CreateRulesetContainer();

        protected abstract IReadOnlyList<HitObjectCompositionTool> CompositionTools { get; }

        /// <summary>
        /// Creates a <see cref="SelectionBlueprint"/> for a specific <see cref="DrawableHitObject"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="DrawableHitObject"/> to create the overlay for.</param>
        public virtual SelectionBlueprint CreateBlueprintFor(DrawableHitObject hitObject) => null;

        /// <summary>
        /// Creates a <see cref="SelectionHandler"/> which outlines <see cref="DrawableHitObject"/>s and handles movement of selections.
        /// </summary>
        public virtual SelectionHandler CreateSelectionHandler() => new SelectionHandler();

        /// <summary>
        /// Creates a <see cref="ScalableContainer"/> which provides a layer above or below the <see cref="Playfield"/>.
        /// </summary>
        protected virtual Container CreateLayerContainer() => new Container { RelativeSizeAxes = Axes.Both };
    }

    public abstract class HitObjectComposer<TObject> : HitObjectComposer
        where TObject : HitObject
    {
        protected HitObjectComposer(Ruleset ruleset)
            : base(ruleset)
        {
        }

        internal override EditRulesetContainer CreateRulesetContainer()
            => new EditRulesetContainer<TObject>(CreateRulesetContainer(Ruleset, Beatmap.Value));

        protected abstract RulesetContainer<TObject> CreateRulesetContainer(Ruleset ruleset, WorkingBeatmap beatmap);
    }
}
