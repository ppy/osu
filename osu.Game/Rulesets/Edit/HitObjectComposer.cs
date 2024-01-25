// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
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
    public abstract partial class HitObjectComposer<TObject> : HitObjectComposer, IPlacementHandler
        where TObject : HitObject
    {
        /// <summary>
        /// Whether the playfield should be centered horizontally. Should be disabled for playfields which span the full horizontal width.
        /// </summary>
        protected virtual bool ApplyHorizontalCentering => true;

        protected IRulesetConfigManager Config { get; private set; }

        // Provides `Playfield`
        private DependencyContainer dependencies;

        [Resolved]
        protected EditorClock EditorClock { get; private set; }

        [Resolved]
        protected EditorBeatmap EditorBeatmap { get; private set; }

        [Resolved]
        protected IBeatSnapProvider BeatSnapProvider { get; private set; }

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; }

        protected ComposeBlueprintContainer BlueprintContainer { get; private set; }

        protected ExpandingToolboxContainer LeftToolbox { get; private set; }

        protected ExpandingToolboxContainer RightToolbox { get; private set; }

        private DrawableEditorRulesetWrapper<TObject> drawableRulesetWrapper;

        protected readonly Container LayerBelowRuleset = new Container { RelativeSizeAxes = Axes.Both };

        protected InputManager InputManager { get; private set; }

        private EditorRadioButtonCollection toolboxCollection;

        private FillFlowContainer togglesCollection;

        private FillFlowContainer sampleBankTogglesCollection;

        private IBindable<bool> hasTiming;
        private Bindable<bool> autoSeekOnPlacement;

        protected DrawableRuleset<TObject> DrawableRuleset { get; private set; }

        protected HitObjectComposer(Ruleset ruleset)
            : base(ruleset)
        {
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            autoSeekOnPlacement = config.GetBindable<bool>(OsuSetting.EditorAutoSeekOnPlacement);

            Config = Dependencies.Get<IRulesetConfigCache>().GetConfigFor(Ruleset);

            try
            {
                DrawableRuleset = CreateDrawableRuleset(Ruleset, EditorBeatmap.PlayableBeatmap, new[] { Ruleset.GetAutoplayMod() });
                drawableRulesetWrapper = new DrawableEditorRulesetWrapper<TObject>(DrawableRuleset)
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

            if (DrawableRuleset is IDrawableScrollingRuleset scrollingRuleset)
                dependencies.CacheAs(scrollingRuleset.ScrollingInfo);

            dependencies.CacheAs(Playfield);

            InternalChildren = new[]
            {
                PlayfieldContentContainer = new Container
                {
                    Name = "Playfield content",
                    RelativeSizeAxes = Axes.Y,
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
                new Container
                {
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = colourProvider.Background5,
                            RelativeSizeAxes = Axes.Both,
                        },
                        LeftToolbox = new ExpandingToolboxContainer(TOOLBOX_CONTRACTED_SIZE_LEFT, 200)
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
                                },
                                new EditorToolboxGroup("bank (Shift-Q~R)")
                                {
                                    Child = sampleBankTogglesCollection = new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Vertical,
                                        Spacing = new Vector2(0, 5),
                                    },
                                }
                            }
                        },
                    }
                },
                new Container
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = colourProvider.Background5,
                            RelativeSizeAxes = Axes.Both,
                        },
                        RightToolbox = new ExpandingToolboxContainer(TOOLBOX_CONTRACTED_SIZE_RIGHT, 250)
                        {
                            Child = new EditorToolboxGroup("inspector")
                            {
                                Child = new HitObjectInspector()
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

            sampleBankTogglesCollection.AddRange(BlueprintContainer.SampleBankTernaryStates.Select(b => new DrawableTernaryButton(b)));

            setSelectTool();

            EditorBeatmap.SelectedHitObjects.CollectionChanged += selectionChanged;
        }

        /// <summary>
        /// Houses all content relevant to the playfield.
        /// </summary>
        /// <remarks>
        /// Generally implementations should not be adding to this directly.
        /// Use <see cref="LayerBelowRuleset"/> or <see cref="BlueprintContainer"/> instead.
        /// </remarks>
        protected Container PlayfieldContentContainer { get; private set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            InputManager = GetContainingInputManager();

            hasTiming = EditorBeatmap.HasTiming.GetBoundCopy();
            hasTiming.BindValueChanged(timing =>
            {
                // it's important this is performed before the similar code in EditorRadioButton disables the button.
                if (!timing.NewValue)
                    setSelectTool();
            });

            EditorBeatmap.HasTiming.BindValueChanged(hasTiming =>
            {
                foreach (var item in toolboxCollection.Items)
                {
                    item.Selected.Disabled = !hasTiming.NewValue;
                }
            }, true);
        }

        protected override void Update()
        {
            base.Update();

            if (ApplyHorizontalCentering)
            {
                PlayfieldContentContainer.Anchor = Anchor.Centre;
                PlayfieldContentContainer.Origin = Anchor.Centre;

                // Ensure that the playfield is always centered but also doesn't get cut off by toolboxes.
                PlayfieldContentContainer.Width = Math.Max(1024, DrawWidth) - TOOLBOX_CONTRACTED_SIZE_RIGHT * 2;
                PlayfieldContentContainer.X = 0;
            }
            else
            {
                PlayfieldContentContainer.Anchor = Anchor.CentreLeft;
                PlayfieldContentContainer.Origin = Anchor.CentreLeft;

                PlayfieldContentContainer.Width = Math.Max(1024, DrawWidth) - (TOOLBOX_CONTRACTED_SIZE_LEFT + TOOLBOX_CONTRACTED_SIZE_RIGHT);
                PlayfieldContentContainer.X = TOOLBOX_CONTRACTED_SIZE_LEFT;
            }
        }

        public override Playfield Playfield => drawableRulesetWrapper.Playfield;

        public override IEnumerable<DrawableHitObject> HitObjects => drawableRulesetWrapper.Playfield.AllHitObjects;

        public override bool CursorInPlacementArea => drawableRulesetWrapper.Playfield.ReceivePositionalInputAt(InputManager.CurrentState.Mouse.Position);

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
        protected virtual IEnumerable<TernaryButton> CreateTernaryButtons() => BlueprintContainer.MainTernaryStates;

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
        protected virtual DrawableRuleset<TObject> CreateDrawableRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods)
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
                var item = e.ShiftPressed
                    ? sampleBankTogglesCollection.ElementAtOrDefault(rightIndex)
                    : togglesCollection.ElementAtOrDefault(rightIndex);

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

                if (autoSeekOnPlacement.Value && EditorClock.CurrentTime < hitObject.StartTime)
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

        public override SnapResult FindSnappedPositionAndTime(Vector2 screenSpacePosition, SnapType snapType = SnapType.All)
        {
            var playfield = PlayfieldAtScreenSpacePosition(screenSpacePosition);
            double? targetTime = null;

            if (snapType.HasFlagFast(SnapType.GlobalGrids))
            {
                if (playfield is ScrollingPlayfield scrollingPlayfield)
                {
                    targetTime = scrollingPlayfield.TimeAtScreenSpacePosition(screenSpacePosition);

                    // apply beat snapping
                    targetTime = BeatSnapProvider.SnapTime(targetTime.Value);

                    // convert back to screen space
                    screenSpacePosition = scrollingPlayfield.ScreenSpacePositionAtTime(targetTime.Value);
                }
            }

            return new SnapResult(screenSpacePosition, targetTime, playfield);
        }

        #endregion
    }

    /// <summary>
    /// A non-generic definition of a HitObject composer class.
    /// Generally used to access certain methods without requiring a generic type for <see cref="HitObjectComposer{T}" />.
    /// </summary>
    [Cached]
    public abstract partial class HitObjectComposer : CompositeDrawable, IPositionSnapProvider
    {
        public const float TOOLBOX_CONTRACTED_SIZE_LEFT = 60;
        public const float TOOLBOX_CONTRACTED_SIZE_RIGHT = 120;

        public readonly Ruleset Ruleset;

        protected HitObjectComposer(Ruleset ruleset)
        {
            Ruleset = ruleset;
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

        /// <summary>
        /// Returns a string representing the current selection.
        /// The inverse method to <see cref="SelectFromTimestamp"/>.
        /// </summary>
        public virtual string ConvertSelectionToString() => string.Empty;

        /// <summary>
        /// Selects objects based on the supplied <paramref name="timestamp"/> and <paramref name="objectDescription"/>.
        /// The inverse method to <see cref="ConvertSelectionToString"/>.
        /// </summary>
        /// <param name="timestamp">The time instant to seek to, in milliseconds.</param>
        /// <param name="objectDescription">The ruleset-specific description of objects to select at the given timestamp.</param>
        public virtual void SelectFromTimestamp(double timestamp, string objectDescription) { }

        #region IPositionSnapProvider

        public abstract SnapResult FindSnappedPositionAndTime(Vector2 screenSpacePosition, SnapType snapType = SnapType.All);

        #endregion
    }
}
