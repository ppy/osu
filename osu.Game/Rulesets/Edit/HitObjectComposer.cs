// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Logging;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Configuration;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Components.RadioButtons;
using osu.Game.Screens.Edit.Compose;
using osu.Game.Screens.Edit.Compose.Components;

namespace osu.Game.Rulesets.Edit
{
    [Cached(Type = typeof(IPlacementHandler))]
    public abstract class HitObjectComposer<TObject> : HitObjectComposer, IPlacementHandler
        where TObject : HitObject
    {
        protected IRulesetConfigManager Config { get; private set; }

        protected readonly Ruleset Ruleset;

        private IWorkingBeatmap workingBeatmap;
        private Beatmap<TObject> playableBeatmap;
        private EditorBeatmap<TObject> editorBeatmap;
        private IBeatmapProcessor beatmapProcessor;

        private DrawableEditRulesetWrapper<TObject> drawableRulesetWrapper;
        private BlueprintContainer blueprintContainer;
        private readonly List<Container> layerContainers = new List<Container>();

        private InputManager inputManager;

        protected HitObjectComposer(Ruleset ruleset)
        {
            Ruleset = ruleset;
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(IFrameBasedClock framedClock)
        {
            try
            {
                drawableRulesetWrapper = new DrawableEditRulesetWrapper<TObject>(CreateDrawableRuleset(Ruleset, workingBeatmap, Array.Empty<Mod>()))
                {
                    Clock = framedClock
                };
            }
            catch (Exception e)
            {
                Logger.Error(e, "Could not load beatmap sucessfully!");
                return;
            }

            var layerBelowRuleset = drawableRulesetWrapper.CreatePlayfieldAdjustmentContainer();
            layerBelowRuleset.Child = new EditorPlayfieldBorder { RelativeSizeAxes = Axes.Both };

            var layerAboveRuleset = drawableRulesetWrapper.CreatePlayfieldAdjustmentContainer();
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
                                drawableRulesetWrapper,
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

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var parentWorkingBeatmap = parent.Get<IBindable<WorkingBeatmap>>().Value;

            playableBeatmap = (Beatmap<TObject>)parentWorkingBeatmap.GetPlayableBeatmap(Ruleset.RulesetInfo, Array.Empty<Mod>());
            workingBeatmap = new EditorWorkingBeatmap<TObject>(playableBeatmap, parentWorkingBeatmap);

            beatmapProcessor = Ruleset.CreateBeatmapProcessor(playableBeatmap);

            editorBeatmap = new EditorBeatmap<TObject>(playableBeatmap);
            editorBeatmap.HitObjectAdded += addHitObject;
            editorBeatmap.HitObjectRemoved += removeHitObject;

            var dependencies = new DependencyContainer(parent);
            dependencies.CacheAs<IEditorBeatmap>(editorBeatmap);
            dependencies.CacheAs<IEditorBeatmap<TObject>>(editorBeatmap);

            Config = dependencies.Get<RulesetConfigCache>().GetConfigFor(Ruleset);

            return base.CreateChildDependencies(dependencies);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            inputManager = GetContainingInputManager();
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            layerContainers.ForEach(l =>
            {
                l.Anchor = drawableRulesetWrapper.Playfield.Anchor;
                l.Origin = drawableRulesetWrapper.Playfield.Origin;
                l.Position = drawableRulesetWrapper.Playfield.Position;
                l.Size = drawableRulesetWrapper.Playfield.Size;
            });
        }

        private void addHitObject(HitObject hitObject)
        {
            beatmapProcessor?.PreProcess();
            hitObject.ApplyDefaults(playableBeatmap.ControlPointInfo, playableBeatmap.BeatmapInfo.BaseDifficulty);
            beatmapProcessor?.PostProcess();
        }

        private void removeHitObject(HitObject hitObject)
        {
            beatmapProcessor?.PreProcess();
            beatmapProcessor?.PostProcess();
        }

        public override IEnumerable<DrawableHitObject> HitObjects => drawableRulesetWrapper.Playfield.AllHitObjects;
        public override bool CursorInPlacementArea => drawableRulesetWrapper.Playfield.ReceivePositionalInputAt(inputManager.CurrentState.Mouse.Position);

        protected abstract IReadOnlyList<HitObjectCompositionTool> CompositionTools { get; }

        protected abstract DrawableRuleset<TObject> CreateDrawableRuleset(Ruleset ruleset, IWorkingBeatmap beatmap, IReadOnlyList<Mod> mods);

        public void BeginPlacement(HitObject hitObject)
        {
        }

        public void EndPlacement(HitObject hitObject) => editorBeatmap.Add(hitObject);

        public void Delete(HitObject hitObject) => editorBeatmap.Remove(hitObject);

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (editorBeatmap != null)
            {
                editorBeatmap.HitObjectAdded -= addHitObject;
                editorBeatmap.HitObjectRemoved -= removeHitObject;
            }
        }
    }

    [Cached(typeof(HitObjectComposer))]
    public abstract class HitObjectComposer : CompositeDrawable
    {
        internal HitObjectComposer()
        {
            RelativeSizeAxes = Axes.Both;
        }

        /// <summary>
        /// All the <see cref="DrawableHitObject"/>s.
        /// </summary>
        public abstract IEnumerable<DrawableHitObject> HitObjects { get; }

        /// <summary>
        /// Whether the user's cursor is currently in an area of the <see cref="HitObjectComposer"/> that is valid for placement.
        /// </summary>
        public abstract bool CursorInPlacementArea { get; }

        /// <summary>
        /// Creates a <see cref="SelectionBlueprint"/> for a specific <see cref="DrawableHitObject"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="DrawableHitObject"/> to create the overlay for.</param>
        public virtual SelectionBlueprint CreateBlueprintFor(DrawableHitObject hitObject) => null;

        /// <summary>
        /// Creates a <see cref="SelectionHandler"/> which outlines <see cref="DrawableHitObject"/>s and handles movement of selections.
        /// </summary>
        public virtual SelectionHandler CreateSelectionHandler() => new SelectionHandler();
    }
}
