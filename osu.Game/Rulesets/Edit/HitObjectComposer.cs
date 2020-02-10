// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.Logging;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
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
using Key = osuTK.Input.Key;

namespace osu.Game.Rulesets.Edit
{
    [Cached(Type = typeof(IPlacementHandler))]
    public abstract class HitObjectComposer<TObject> : HitObjectComposer, IPlacementHandler
        where TObject : HitObject
    {
        protected IRulesetConfigManager Config { get; private set; }

        protected readonly Ruleset Ruleset;

        [Resolved]
        protected IFrameBasedClock EditorClock { get; private set; }

        [Resolved]
        protected EditorBeatmap EditorBeatmap { get; private set; }

        [Resolved]
        private IAdjustableClock adjustableClock { get; set; }

        [Resolved]
        private IBeatSnapProvider beatSnapProvider { get; set; }

        protected ComposeBlueprintContainer BlueprintContainer { get; private set; }

        private DrawableEditRulesetWrapper<TObject> drawableRulesetWrapper;
        private Container distanceSnapGridContainer;
        private DistanceSnapGrid distanceSnapGrid;
        private readonly List<Container> layerContainers = new List<Container>();

        private InputManager inputManager;

        private RadioButtonCollection toolboxCollection;

        protected HitObjectComposer(Ruleset ruleset)
        {
            Ruleset = ruleset;
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(IFrameBasedClock framedClock)
        {
            EditorBeatmap.HitObjectAdded += addHitObject;
            EditorBeatmap.HitObjectRemoved += removeHitObject;
            EditorBeatmap.StartTimeChanged += UpdateHitObject;

            Config = Dependencies.Get<RulesetConfigCache>().GetConfigFor(Ruleset);

            try
            {
                drawableRulesetWrapper = new DrawableEditRulesetWrapper<TObject>(CreateDrawableRuleset(Ruleset, EditorBeatmap.PlayableBeatmap))
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

            var layerAboveRuleset = drawableRulesetWrapper.CreatePlayfieldAdjustmentContainer().WithChild(BlueprintContainer = CreateBlueprintContainer());

            layerContainers.Add(layerBelowRuleset);
            layerContainers.Add(layerAboveRuleset);

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

            toolboxCollection.Items = CompositionTools
                                      .Prepend(new SelectTool())
                                      .Select(t => new RadioButton(t.Name, () => toolSelected(t)))
                                      .ToList();

            setSelectTool();

            BlueprintContainer.SelectionChanged += selectionChanged;
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.Key >= Key.Number1 && e.Key <= Key.Number9)
            {
                var item = toolboxCollection.Items.ElementAtOrDefault(e.Key - Key.Number1);

                if (item != null)
                {
                    item.Select();
                    return true;
                }
            }

            return base.OnKeyDown(e);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            inputManager = GetContainingInputManager();
        }

        private double lastGridUpdateTime;

        protected override void Update()
        {
            base.Update();

            if (EditorClock.CurrentTime != lastGridUpdateTime && !(BlueprintContainer.CurrentTool is SelectTool))
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

            if (hitObjects.Any())
            {
                // ensure in selection mode if a selection is made.
                setSelectTool();

                showGridFor(hitObjects);
            }
            else
                distanceSnapGridContainer.Hide();
        }

        private void setSelectTool() => toolboxCollection.Items.First().Select();

        private void toolSelected(HitObjectCompositionTool tool)
        {
            BlueprintContainer.CurrentTool = tool;

            if (tool is SelectTool)
                distanceSnapGridContainer.Hide();
            else
            {
                EditorBeatmap.SelectedHitObjects.Clear();
                showGridFor(Enumerable.Empty<HitObject>());
            }
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

            lastGridUpdateTime = EditorClock.CurrentTime;
        }

        private void addHitObject(HitObject hitObject) => UpdateHitObject(hitObject);

        private void removeHitObject(HitObject hitObject) => UpdateHitObject(null);

        public override IEnumerable<DrawableHitObject> HitObjects => drawableRulesetWrapper.Playfield.AllHitObjects;
        public override bool CursorInPlacementArea => drawableRulesetWrapper.Playfield.ReceivePositionalInputAt(inputManager.CurrentState.Mouse.Position);

        protected abstract IReadOnlyList<HitObjectCompositionTool> CompositionTools { get; }

        protected abstract ComposeBlueprintContainer CreateBlueprintContainer();

        protected abstract DrawableRuleset<TObject> CreateDrawableRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods = null);

        public void BeginPlacement(HitObject hitObject)
        {
            EditorBeatmap.PlacementObject.Value = hitObject;

            if (distanceSnapGrid != null)
                hitObject.StartTime = GetSnappedPosition(distanceSnapGrid.ToLocalSpace(inputManager.CurrentState.Mouse.Position), hitObject.StartTime).time;
        }

        public void EndPlacement(HitObject hitObject, bool commit)
        {
            EditorBeatmap.PlacementObject.Value = null;

            if (commit)
            {
                EditorBeatmap.Add(hitObject);

                adjustableClock.Seek(hitObject.GetEndTime());
            }

            showGridFor(Enumerable.Empty<HitObject>());
        }

        public void Delete(HitObject hitObject) => EditorBeatmap.Remove(hitObject);

        public override (Vector2 position, double time) GetSnappedPosition(Vector2 position, double time) => distanceSnapGrid?.GetSnappedPosition(position) ?? (position, time);

        public override float GetBeatSnapDistanceAt(double referenceTime)
        {
            DifficultyControlPoint difficultyPoint = EditorBeatmap.ControlPointInfo.DifficultyPointAt(referenceTime);
            return (float)(100 * EditorBeatmap.BeatmapInfo.BaseDifficulty.SliderMultiplier * difficultyPoint.SpeedMultiplier / beatSnapProvider.BeatDivisor);
        }

        public override float DurationToDistance(double referenceTime, double duration)
        {
            double beatLength = beatSnapProvider.GetBeatLengthAtTime(referenceTime);
            return (float)(duration / beatLength * GetBeatSnapDistanceAt(referenceTime));
        }

        public override double DistanceToDuration(double referenceTime, float distance)
        {
            double beatLength = beatSnapProvider.GetBeatLengthAtTime(referenceTime);
            return distance / GetBeatSnapDistanceAt(referenceTime) * beatLength;
        }

        public override double GetSnappedDurationFromDistance(double referenceTime, float distance)
            => beatSnapProvider.SnapTime(referenceTime + DistanceToDuration(referenceTime, distance), referenceTime) - referenceTime;

        public override float GetSnappedDistanceFromDistance(double referenceTime, float distance)
        {
            var snappedEndTime = beatSnapProvider.SnapTime(referenceTime + DistanceToDuration(referenceTime, distance), referenceTime);

            return DurationToDistance(referenceTime, snappedEndTime - referenceTime);
        }

        public override void UpdateHitObject(HitObject hitObject) => EditorBeatmap.UpdateHitObject(hitObject);

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
    [Cached(typeof(IDistanceSnapProvider))]
    public abstract class HitObjectComposer : CompositeDrawable, IDistanceSnapProvider
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
        /// Creates the <see cref="DistanceSnapGrid"/> applicable for a <see cref="HitObject"/> selection.
        /// </summary>
        /// <param name="selectedHitObjects">The <see cref="HitObject"/> selection.</param>
        /// <returns>The <see cref="DistanceSnapGrid"/> for <paramref name="selectedHitObjects"/>. If empty, a grid is returned for the current point in time.</returns>
        [CanBeNull]
        protected virtual DistanceSnapGrid CreateDistanceSnapGrid([NotNull] IEnumerable<HitObject> selectedHitObjects) => null;

        /// <summary>
        /// Updates a <see cref="HitObject"/>, invoking <see cref="HitObject.ApplyDefaults"/> and re-processing the beatmap.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> to update.</param>
        public abstract void UpdateHitObject([CanBeNull] HitObject hitObject);

        public abstract (Vector2 position, double time) GetSnappedPosition(Vector2 position, double time);

        public abstract float GetBeatSnapDistanceAt(double referenceTime);

        public abstract float DurationToDistance(double referenceTime, double duration);

        public abstract double DistanceToDuration(double referenceTime, float distance);

        public abstract double GetSnappedDurationFromDistance(double referenceTime, float distance);

        public abstract float GetSnappedDistanceFromDistance(double referenceTime, float distance);
    }
}
