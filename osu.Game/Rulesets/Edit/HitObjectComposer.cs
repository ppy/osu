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
using osu.Game.Screens.Edit.Components.TernaryButtons;
using osu.Game.Screens.Edit.Compose;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Edit
{
    /// <summary>
    /// Top level container for editor compose mode.
    /// Responsible for providing snapping and generally gluing components together.
    /// </summary>
    /// <typeparam name="TObject">The base type of supported objects.</typeparam>
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

        private DrawableEditRulesetWrapper<TObject> drawableRulesetWrapper;

        protected readonly Container LayerBelowRuleset = new Container { RelativeSizeAxes = Axes.Both };

        private InputManager inputManager;

        private RadioButtonCollection toolboxCollection;

        private FillFlowContainer togglesCollection;

        protected HitObjectComposer(Ruleset ruleset)
        {
            Ruleset = ruleset;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Config = Dependencies.Get<RulesetConfigCache>().GetConfigFor(Ruleset);

            try
            {
                drawableRulesetWrapper = new DrawableEditRulesetWrapper<TObject>(CreateDrawableRuleset(Ruleset, EditorBeatmap.PlayableBeatmap, new[] { Ruleset.GetAutoplayMod() }))
                {
                    Clock = EditorClock,
                    ProcessCustomClock = false
                };
            }
            catch (Exception e)
            {
                Logger.Error(e, "Could not load beatmap successfully!");
                return;
            }

            const float toolbar_width = 200;

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    Name = "Content",
                    Padding = new MarginPadding { Left = toolbar_width },
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        // layers below playfield
                        drawableRulesetWrapper.CreatePlayfieldAdjustmentContainer().WithChild(LayerBelowRuleset),
                        drawableRulesetWrapper,
                        // layers above playfield
                        drawableRulesetWrapper.CreatePlayfieldAdjustmentContainer()
                                              .WithChild(BlueprintContainer = CreateBlueprintContainer())
                    }
                },
                new FillFlowContainer
                {
                    Name = "Sidebar",
                    RelativeSizeAxes = Axes.Y,
                    Width = toolbar_width,
                    Padding = new MarginPadding { Right = 10 },
                    Spacing = new Vector2(10),
                    Children = new Drawable[]
                    {
                        new ToolboxGroup("toolbox (1-9)")
                        {
                            Child = toolboxCollection = new RadioButtonCollection { RelativeSizeAxes = Axes.X }
                        },
                        new ToolboxGroup("toggles (Q~P)")
                        {
                            Child = togglesCollection = new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(0, 5),
                            },
                        }
                    }
                },
            };

            toolboxCollection.Items = CompositionTools
                                      .Prepend(new SelectTool())
                                      .Select(t => new RadioButton(t.Name, () => toolSelected(t), t.CreateIcon))
                                      .ToList();

            TernaryStates = CreateTernaryButtons().ToArray();
            togglesCollection.AddRange(TernaryStates.Select(b => new DrawableTernaryButton(b)));

            setSelectTool();

            EditorBeatmap.SelectedHitObjects.CollectionChanged += selectionChanged;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            inputManager = GetContainingInputManager();
        }

        public override Playfield Playfield => drawableRulesetWrapper.Playfield;

        public override IEnumerable<DrawableHitObject> HitObjects => drawableRulesetWrapper.Playfield.AllHitObjects;

        public override bool CursorInPlacementArea => drawableRulesetWrapper.Playfield.ReceivePositionalInputAt(inputManager.CurrentState.Mouse.Position);

        /// <summary>
        /// Defines all available composition tools, listed on the left side of the editor screen as button controls.
        /// This should usually define one tool for each <see cref="HitObject"/> type used in the target ruleset.
        /// </summary>
        /// <remarks>
        /// A "select" tool is automatically added as the first tool.
        /// </remarks>
        protected abstract IReadOnlyList<HitObjectCompositionTool> CompositionTools { get; }

        /// <summary>
        /// A collection of states which will be displayed to the user in the toolbox.
        /// </summary>
        public TernaryButton[] TernaryStates { get; private set; }

        /// <summary>
        /// Create all ternary states required to be displayed to the user.
        /// </summary>
        protected virtual IEnumerable<TernaryButton> CreateTernaryButtons() => BlueprintContainer.TernaryStates;

        /// <summary>
        /// Construct a relevant blueprint container. This will manage hitobject selection/placement input handling and display logic.
        /// </summary>
        protected virtual ComposeBlueprintContainer CreateBlueprintContainer()
            => new ComposeBlueprintContainer(this);

        /// <summary>
        /// Construct a drawable ruleset for the provided ruleset.
        /// </summary>
        /// <remarks>
        /// Can be overridden to add editor-specific logical changes to a <see cref="Ruleset"/>'s standard <see cref="DrawableRuleset{TObject}"/>.
        /// For example, hit animations or judgement logic may be changed to give a better editor user experience.
        /// </remarks>
        /// <param name="ruleset">The ruleset used to construct its drawable counterpart.</param>
        /// <param name="beatmap">The loaded beatmap.</param>
        /// <param name="mods">The mods to be applied.</param>
        /// <returns>An editor-relevant <see cref="DrawableRuleset{TObject}"/>.</returns>
        protected virtual DrawableRuleset<TObject> CreateDrawableRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods = null)
            => (DrawableRuleset<TObject>)ruleset.CreateDrawableRulesetWith(beatmap, mods);

        #region Tool selection logic

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.ControlPressed || e.AltPressed || e.SuperPressed)
                return false;

            if (checkLeftToggleFromKey(e.Key, out var leftIndex))
            {
                var item = toolboxCollection.Items.ElementAtOrDefault(leftIndex);

                if (item != null)
                {
                    item.Select();
                    return true;
                }
            }

            if (checkRightToggleFromKey(e.Key, out var rightIndex))
            {
                var item = togglesCollection.ElementAtOrDefault(rightIndex);

                if (item is DrawableTernaryButton button)
                {
                    button.Button.Toggle();
                    return true;
                }
            }

            return base.OnKeyDown(e);
        }

        private bool checkLeftToggleFromKey(Key key, out int index)
        {
            if (key < Key.Number1 || key > Key.Number9)
            {
                index = -1;
                return false;
            }

            index = key - Key.Number1;
            return true;
        }

        private bool checkRightToggleFromKey(Key key, out int index)
        {
            switch (key)
            {
                case Key.Q:
                    index = 0;
                    break;

                case Key.W:
                    index = 1;
                    break;

                case Key.E:
                    index = 2;
                    break;

                case Key.R:
                    index = 3;
                    break;

                case Key.T:
                    index = 4;
                    break;

                case Key.Y:
                    index = 5;
                    break;

                case Key.U:
                    index = 6;
                    break;

                case Key.I:
                    index = 7;
                    break;

                case Key.O:
                    index = 8;
                    break;

                case Key.P:
                    index = 9;
                    break;

                default:
                    index = -1;
                    break;
            }

            return index >= 0;
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

        #endregion

        #region IPlacementHandler

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

        #endregion

        #region IPositionSnapProvider

        /// <summary>
        /// Retrieve the relevant <see cref="Playfield"/> at a specified screen-space position.
        /// In cases where a ruleset doesn't require custom logic (due to nested playfields, for example)
        /// this will return the ruleset's main playfield.
        /// </summary>
        /// <param name="screenSpacePosition">The screen-space position to query.</param>
        /// <returns>The most relevant <see cref="Playfield"/>.</returns>
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
            double actualDuration = referenceTime + DistanceToDuration(referenceTime, distance);

            double snappedEndTime = BeatSnapProvider.SnapTime(actualDuration, referenceTime);

            double beatLength = BeatSnapProvider.GetBeatLengthAtTime(referenceTime);

            // we don't want to exceed the actual duration and snap to a point in the future.
            // as we are snapping to beat length via SnapTime (which will round-to-nearest), check for snapping in the forward direction and reverse it.
            if (snappedEndTime > actualDuration + 1)
                snappedEndTime -= beatLength;

            return DurationToDistance(referenceTime, snappedEndTime - referenceTime);
        }

        #endregion
    }

    /// <summary>
    /// A non-generic definition of a HitObject composer class.
    /// Generally used to access certain methods without requiring a generic type for <see cref="HitObjectComposer{T}" />.
    /// </summary>
    [Cached(typeof(HitObjectComposer))]
    [Cached(typeof(IPositionSnapProvider))]
    public abstract class HitObjectComposer : CompositeDrawable, IPositionSnapProvider
    {
        protected HitObjectComposer()
        {
            RelativeSizeAxes = Axes.Both;
        }

        /// <summary>
        /// The target ruleset's playfield.
        /// </summary>
        public abstract Playfield Playfield { get; }

        /// <summary>
        /// All <see cref="DrawableHitObject"/>s in currently loaded beatmap.
        /// </summary>
        public abstract IEnumerable<DrawableHitObject> HitObjects { get; }

        /// <summary>
        /// Whether the user's cursor is currently in an area of the <see cref="HitObjectComposer"/> that is valid for placement.
        /// </summary>
        public abstract bool CursorInPlacementArea { get; }

        #region IPositionSnapProvider

        public abstract SnapResult SnapScreenSpacePositionToValidTime(Vector2 screenSpacePosition);

        public virtual SnapResult SnapScreenSpacePositionToValidPosition(Vector2 screenSpacePosition) =>
            new SnapResult(screenSpacePosition, null);

        public abstract float GetBeatSnapDistanceAt(double referenceTime);

        public abstract float DurationToDistance(double referenceTime, double duration);

        public abstract double DistanceToDuration(double referenceTime, float distance);

        public abstract double GetSnappedDurationFromDistance(double referenceTime, float distance);

        public abstract float GetSnappedDistanceFromDistance(double referenceTime, float distance);

        #endregion
    }
}
