// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
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

        // Provides `Playfield`
        private DependencyContainer dependencies;

        [Resolved]
        protected EditorClock EditorClock { get; private set; }

        [Resolved]
        protected EditorBeatmap EditorBeatmap { get; private set; }

        [Resolved]
        protected IBeatSnapProvider BeatSnapProvider { get; private set; }

        protected ComposeBlueprintContainer BlueprintContainer { get; private set; }

        private DrawableEditorRulesetWrapper<TObject> drawableRulesetWrapper;

        protected readonly Container LayerBelowRuleset = new Container { RelativeSizeAxes = Axes.Both };

        private InputManager inputManager;

        private EditorRadioButtonCollection toolboxCollection;

        private FillFlowContainer togglesCollection;

        private IBindable<bool> hasTiming;

        protected HitObjectComposer(Ruleset ruleset)
        {
            Ruleset = ruleset;
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        [BackgroundDependencyLoader]
        private void load()
        {
            Config = Dependencies.Get<IRulesetConfigCache>().GetConfigFor(Ruleset);

            try
            {
                drawableRulesetWrapper = new DrawableEditorRulesetWrapper<TObject>(CreateDrawableRuleset(Ruleset, EditorBeatmap.PlayableBeatmap, new[] { Ruleset.GetAutoplayMod() }))
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

            dependencies.CacheAs(Playfield);

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    Name = "Content",
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
                new LeftToolboxFlow
                {
                    Children = new Drawable[]
                    {
                        new EditorToolboxGroup("toolbox (1-9)")
                        {
                            Child = toolboxCollection = new EditorRadioButtonCollection { RelativeSizeAxes = Axes.X }
                        },
                        new EditorToolboxGroup("toggles (Q~P)")
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

            hasTiming = EditorBeatmap.HasTiming.GetBoundCopy();
            hasTiming.BindValueChanged(timing =>
            {
                // it's important this is performed before the similar code in EditorRadioButton disables the button.
                if (!timing.NewValue)
                    setSelectTool();
            });
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
        protected virtual ComposeBlueprintContainer CreateBlueprintContainer() => new ComposeBlueprintContainer(this);

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

            if (checkLeftToggleFromKey(e.Key, out int leftIndex))
            {
                var item = toolboxCollection.Items.ElementAtOrDefault(leftIndex);

                if (item != null)
                {
                    if (!item.Selected.Disabled)
                        item.Select();
                    return true;
                }
            }

            if (checkRightToggleFromKey(e.Key, out int rightIndex))
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
                    EditorClock.SeekSmoothlyTo(hitObject.StartTime);
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

        public override float GetBeatSnapDistanceAt(HitObject referenceObject)
        {
            return (float)(100 * EditorBeatmap.Difficulty.SliderMultiplier * referenceObject.DifficultyControlPoint.SliderVelocity / BeatSnapProvider.BeatDivisor);
        }

        public override float DurationToDistance(HitObject referenceObject, double duration)
        {
            double beatLength = BeatSnapProvider.GetBeatLengthAtTime(referenceObject.StartTime);
            return (float)(duration / beatLength * GetBeatSnapDistanceAt(referenceObject));
        }

        public override double DistanceToDuration(HitObject referenceObject, float distance)
        {
            double beatLength = BeatSnapProvider.GetBeatLengthAtTime(referenceObject.StartTime);
            return distance / GetBeatSnapDistanceAt(referenceObject) * beatLength;
        }

        public override double GetSnappedDurationFromDistance(HitObject referenceObject, float distance)
            => BeatSnapProvider.SnapTime(referenceObject.StartTime + DistanceToDuration(referenceObject, distance), referenceObject.StartTime) - referenceObject.StartTime;

        public override float GetSnappedDistanceFromDistance(HitObject referenceObject, float distance)
        {
            double startTime = referenceObject.StartTime;

            double actualDuration = startTime + DistanceToDuration(referenceObject, distance);

            double snappedEndTime = BeatSnapProvider.SnapTime(actualDuration, startTime);

            double beatLength = BeatSnapProvider.GetBeatLengthAtTime(startTime);

            // we don't want to exceed the actual duration and snap to a point in the future.
            // as we are snapping to beat length via SnapTime (which will round-to-nearest), check for snapping in the forward direction and reverse it.
            if (snappedEndTime > actualDuration + 1)
                snappedEndTime -= beatLength;

            return DurationToDistance(referenceObject, snappedEndTime - startTime);
        }

        #endregion

        private class LeftToolboxFlow : ExpandingButtonContainer
        {
            public LeftToolboxFlow()
                : base(80, 200)
            {
                RelativeSizeAxes = Axes.Y;
                Padding = new MarginPadding { Right = 10 };

                FillFlow.Spacing = new Vector2(10);
            }
        }
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

        public virtual string ConvertSelectionToString() => string.Empty;

        #region IPositionSnapProvider

        public abstract SnapResult SnapScreenSpacePositionToValidTime(Vector2 screenSpacePosition);

        public virtual SnapResult SnapScreenSpacePositionToValidPosition(Vector2 screenSpacePosition) =>
            new SnapResult(screenSpacePosition, null);

        public abstract float GetBeatSnapDistanceAt(HitObject referenceObject);

        public abstract float DurationToDistance(HitObject referenceObject, double duration);

        public abstract double DistanceToDuration(HitObject referenceObject, float distance);

        public abstract double GetSnappedDurationFromDistance(HitObject referenceObject, float distance);

        public abstract float GetSnappedDistanceFromDistance(HitObject referenceObject, float distance);

        #endregion
    }
}
