// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Configuration;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
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
        protected EditorClock EditorClock { get; private set; }

        [Resolved]
        protected EditorBeatmap EditorBeatmap { get; private set; }

        [Resolved]
        protected IBeatSnapProvider BeatSnapProvider { get; private set; }

        protected ComposeBlueprintContainer BlueprintContainer { get; private set; }

        public override Playfield Playfield => drawableRulesetWrapper.Playfield;

        private DrawableEditRulesetWrapper<TObject> drawableRulesetWrapper;

        protected readonly Container LayerBelowRuleset = new Container { RelativeSizeAxes = Axes.Both };

        private InputManager inputManager;

        private RadioButtonCollection toolboxCollection;

        protected HitObjectComposer(Ruleset ruleset)
        {
            Ruleset = ruleset;
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Config = Dependencies.Get<RulesetConfigCache>().GetConfigFor(Ruleset);

            try
            {
                drawableRulesetWrapper = new DrawableEditRulesetWrapper<TObject>(CreateDrawableRuleset(Ruleset, EditorBeatmap.PlayableBeatmap))
                {
                    Clock = EditorClock,
                    ProcessCustomClock = false
                };
            }
            catch (Exception e)
            {
                Logger.Error(e, "Could not load beatmap sucessfully!");
                return;
            }

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
                            Masking = true,
                            Children = new Drawable[]
                            {
                                // layers below playfield
                                drawableRulesetWrapper.CreatePlayfieldAdjustmentContainer().WithChildren(new Drawable[]
                                {
                                    LayerBelowRuleset,
                                    new EditorPlayfieldBorder { RelativeSizeAxes = Axes.Both }
                                }),
                                drawableRulesetWrapper,
                                // layers above playfield
                                drawableRulesetWrapper.CreatePlayfieldAdjustmentContainer()
                                                      .WithChild(BlueprintContainer = CreateBlueprintContainer())
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

            EditorBeatmap.SelectedHitObjects.CollectionChanged += selectionChanged;
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

        private void selectionChanged(object sender, NotifyCollectionChangedEventArgs changedArgs)
        {
            if (EditorBeatmap.SelectedHitObjects.Any())
            {
                // ensure in selection mode if a selection is made.
                setSelectTool();
            }
        }

        private void setSelectTool() => toolboxCollection.Items.First().Select();

        private void toolSelected(HitObjectCompositionTool tool)
        {
            BlueprintContainer.CurrentTool = tool;

            if (!(tool is SelectTool))
                EditorBeatmap.SelectedHitObjects.Clear();
        }

        public override IEnumerable<DrawableHitObject> HitObjects => drawableRulesetWrapper.Playfield.AllHitObjects;

        public override bool CursorInPlacementArea => drawableRulesetWrapper.Playfield.ReceivePositionalInputAt(inputManager.CurrentState.Mouse.Position);

        protected abstract IReadOnlyList<HitObjectCompositionTool> CompositionTools { get; }

        protected abstract ComposeBlueprintContainer CreateBlueprintContainer();

        protected abstract DrawableRuleset<TObject> CreateDrawableRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods = null);

        public void BeginPlacement(HitObject hitObject)
        {
            EditorBeatmap.PlacementObject.Value = hitObject;
        }

        public void EndPlacement(HitObject hitObject, bool commit)
        {
            EditorBeatmap.PlacementObject.Value = null;

            if (commit)
            {
                EditorBeatmap.Add(hitObject);

                if (EditorClock.CurrentTime < hitObject.StartTime)
                    EditorClock.SeekTo(hitObject.StartTime);
            }
        }

        public void Delete(HitObject hitObject) => EditorBeatmap.Remove(hitObject);

        protected virtual Playfield PlayfieldAtScreenSpacePosition(Vector2 screenSpacePosition) => drawableRulesetWrapper.Playfield;

        public override SnapResult SnapScreenSpacePositionToValidTime(Vector2 screenSpacePosition)
        {
            var playfield = PlayfieldAtScreenSpacePosition(screenSpacePosition);
            double? targetTime = null;

            if (playfield is ScrollingPlayfield scrollingPlayfield)
            {
                targetTime = scrollingPlayfield.TimeAtScreenSpacePosition(screenSpacePosition);

                // apply beat snapping
                targetTime = BeatSnapProvider.SnapTime(targetTime.Value);

                // convert back to screen space
                screenSpacePosition = scrollingPlayfield.ScreenSpacePositionAtTime(targetTime.Value);
            }

            return new SnapResult(screenSpacePosition, targetTime, playfield);
        }

        public override float GetBeatSnapDistanceAt(double referenceTime)
        {
            DifficultyControlPoint difficultyPoint = EditorBeatmap.ControlPointInfo.DifficultyPointAt(referenceTime);
            return (float)(100 * EditorBeatmap.BeatmapInfo.BaseDifficulty.SliderMultiplier * difficultyPoint.SpeedMultiplier / BeatSnapProvider.BeatDivisor);
        }

        public override float DurationToDistance(double referenceTime, double duration)
        {
            double beatLength = BeatSnapProvider.GetBeatLengthAtTime(referenceTime);
            return (float)(duration / beatLength * GetBeatSnapDistanceAt(referenceTime));
        }

        public override double DistanceToDuration(double referenceTime, float distance)
        {
            double beatLength = BeatSnapProvider.GetBeatLengthAtTime(referenceTime);
            return distance / GetBeatSnapDistanceAt(referenceTime) * beatLength;
        }

        public override double GetSnappedDurationFromDistance(double referenceTime, float distance)
            => BeatSnapProvider.SnapTime(referenceTime + DistanceToDuration(referenceTime, distance), referenceTime) - referenceTime;

        public override float GetSnappedDistanceFromDistance(double referenceTime, float distance)
        {
            var snappedEndTime = BeatSnapProvider.SnapTime(referenceTime + DistanceToDuration(referenceTime, distance), referenceTime);

            return DurationToDistance(referenceTime, snappedEndTime - referenceTime);
        }
    }

    [Cached(typeof(HitObjectComposer))]
    [Cached(typeof(IPositionSnapProvider))]
    public abstract class HitObjectComposer : CompositeDrawable, IPositionSnapProvider
    {
        protected HitObjectComposer()
        {
            RelativeSizeAxes = Axes.Both;
        }

        public abstract Playfield Playfield { get; }

        /// <summary>
        /// All the <see cref="DrawableHitObject"/>s.
        /// </summary>
        public abstract IEnumerable<DrawableHitObject> HitObjects { get; }

        /// <summary>
        /// Whether the user's cursor is currently in an area of the <see cref="HitObjectComposer"/> that is valid for placement.
        /// </summary>
        public abstract bool CursorInPlacementArea { get; }

        public abstract SnapResult SnapScreenSpacePositionToValidTime(Vector2 screenSpacePosition);

        public abstract float GetBeatSnapDistanceAt(double referenceTime);

        public abstract float DurationToDistance(double referenceTime, double duration);

        public abstract double DistanceToDuration(double referenceTime, float distance);

        public abstract double GetSnappedDurationFromDistance(double referenceTime, float distance);

        public abstract float GetSnappedDistanceFromDistance(double referenceTime, float distance);
    }
}
