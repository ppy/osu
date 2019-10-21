// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Logging;
using osu.Framework.Threading;
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
using osuTK;

namespace osu.Game.Rulesets.Edit
{
    [Cached(Type = typeof(IPlacementHandler))]
    public abstract class HitObjectComposer<TObject> : HitObjectComposer, IPlacementHandler
        where TObject : HitObject
    {
        protected IRulesetConfigManager Config { get; private set; }
        protected EditorBeatmap<TObject> EditorBeatmap { get; private set; }
        protected readonly Ruleset Ruleset;

        [Resolved]
        protected IFrameBasedClock EditorClock { get; private set; }

        private IWorkingBeatmap workingBeatmap;
        private Beatmap<TObject> playableBeatmap;
        private IBeatmapProcessor beatmapProcessor;

        private DrawableEditRulesetWrapper<TObject> drawableRulesetWrapper;
        private BlueprintContainer blueprintContainer;
        private Container distanceSnapGridContainer;
        private DistanceSnapGrid distanceSnapGrid;
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
                    Clock = framedClock,
                    ProcessCustomClock = false
                };
            }
            catch (Exception e)
            {
                Logger.Error(e, "Could not load beatmap sucessfully!");
                return;
            }

            var layerBelowRuleset = drawableRulesetWrapper.CreatePlayfieldAdjustmentContainer().WithChildren(new Drawable[]
            {
                distanceSnapGridContainer = new Container { RelativeSizeAxes = Axes.Both },
                new EditorPlayfieldBorder { RelativeSizeAxes = Axes.Both }
            });

            var layerAboveRuleset = drawableRulesetWrapper.CreatePlayfieldAdjustmentContainer().WithChild(blueprintContainer = new BlueprintContainer());

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
                CompositionTools.Select(t => new RadioButton(t.Name, () => selectTool(t)))
                                .Prepend(new RadioButton("Select", () => selectTool(null)))
                                .ToList();

            toolboxCollection.Items[0].Select();

            blueprintContainer.SelectionChanged += selectionChanged;
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var parentWorkingBeatmap = parent.Get<IBindable<WorkingBeatmap>>().Value;

            playableBeatmap = (Beatmap<TObject>)parentWorkingBeatmap.GetPlayableBeatmap(Ruleset.RulesetInfo, Array.Empty<Mod>());
            workingBeatmap = new EditorWorkingBeatmap<TObject>(playableBeatmap, parentWorkingBeatmap);

            beatmapProcessor = Ruleset.CreateBeatmapProcessor(playableBeatmap);

            EditorBeatmap = new EditorBeatmap<TObject>(playableBeatmap);
            EditorBeatmap.HitObjectAdded += addHitObject;
            EditorBeatmap.HitObjectRemoved += removeHitObject;
            EditorBeatmap.StartTimeChanged += updateHitObject;

            var dependencies = new DependencyContainer(parent);
            dependencies.CacheAs<IEditorBeatmap>(EditorBeatmap);
            dependencies.CacheAs<IEditorBeatmap<TObject>>(EditorBeatmap);

            Config = dependencies.Get<RulesetConfigCache>().GetConfigFor(Ruleset);

            return base.CreateChildDependencies(dependencies);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            inputManager = GetContainingInputManager();
        }

        protected override void Update()
        {
            base.Update();

            if (EditorClock.ElapsedFrameTime != 0 && blueprintContainer.CurrentTool != null)
                showGridFor(Enumerable.Empty<HitObject>());
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

        private void selectionChanged(IEnumerable<HitObject> selectedHitObjects)
        {
            var hitObjects = selectedHitObjects.ToArray();

            if (!hitObjects.Any())
                distanceSnapGridContainer.Hide();
            else
                showGridFor(hitObjects);
        }

        private void selectTool(HitObjectCompositionTool tool)
        {
            blueprintContainer.CurrentTool = tool;

            if (tool == null)
                distanceSnapGridContainer.Hide();
            else
                showGridFor(Enumerable.Empty<HitObject>());
        }

        private void showGridFor(IEnumerable<HitObject> selectedHitObjects)
        {
            distanceSnapGridContainer.Clear();
            distanceSnapGrid = CreateDistanceSnapGrid(selectedHitObjects);

            if (distanceSnapGrid != null)
            {
                distanceSnapGridContainer.Child = distanceSnapGrid;
                distanceSnapGridContainer.Show();
            }
        }

        private ScheduledDelegate scheduledUpdate;

        private void addHitObject(HitObject hitObject) => updateHitObject(hitObject);

        private void removeHitObject(HitObject hitObject) => updateHitObject(null);

        private void updateHitObject([CanBeNull] HitObject hitObject)
        {
            scheduledUpdate?.Cancel();
            scheduledUpdate = Schedule(() =>
            {
                beatmapProcessor?.PreProcess();
                hitObject?.ApplyDefaults(playableBeatmap.ControlPointInfo, playableBeatmap.BeatmapInfo.BaseDifficulty);
                beatmapProcessor?.PostProcess();
            });
        }

        public override IEnumerable<DrawableHitObject> HitObjects => drawableRulesetWrapper.Playfield.AllHitObjects;
        public override bool CursorInPlacementArea => drawableRulesetWrapper.Playfield.ReceivePositionalInputAt(inputManager.CurrentState.Mouse.Position);

        protected abstract IReadOnlyList<HitObjectCompositionTool> CompositionTools { get; }

        protected abstract DrawableRuleset<TObject> CreateDrawableRuleset(Ruleset ruleset, IWorkingBeatmap beatmap, IReadOnlyList<Mod> mods);

        public void BeginPlacement(HitObject hitObject)
        {
            if (distanceSnapGrid != null)
                hitObject.StartTime = GetSnappedTime(hitObject.StartTime, distanceSnapGrid.ToLocalSpace(inputManager.CurrentState.Mouse.Position));
        }

        public void EndPlacement(HitObject hitObject)
        {
            EditorBeatmap.Add(hitObject);
            showGridFor(Enumerable.Empty<HitObject>());
        }

        public void Delete(HitObject hitObject) => EditorBeatmap.Remove(hitObject);

        public override Vector2 GetSnappedPosition(Vector2 position) => distanceSnapGrid?.GetSnapPosition(position) ?? position;

        public override double GetSnappedTime(double startTime, Vector2 position) => distanceSnapGrid?.GetSnapTime(position) ?? startTime;

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (EditorBeatmap != null)
            {
                EditorBeatmap.HitObjectAdded -= addHitObject;
                EditorBeatmap.HitObjectRemoved -= removeHitObject;
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

        /// <summary>
        /// Creates the <see cref="DistanceSnapGrid"/> applicable for a <see cref="HitObject"/> selection.
        /// </summary>
        /// <param name="selectedHitObjects">The <see cref="HitObject"/> selection.</param>
        /// <returns>The <see cref="DistanceSnapGrid"/> for <paramref name="selectedHitObjects"/>.</returns>
        [CanBeNull]
        protected virtual DistanceSnapGrid CreateDistanceSnapGrid([NotNull] IEnumerable<HitObject> selectedHitObjects) => null;

        public abstract Vector2 GetSnappedPosition(Vector2 position);

        public abstract double GetSnappedTime(double startTime, Vector2 screenSpacePosition);
    }
}
