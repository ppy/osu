// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using Humanizer;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Audio;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Edit.Components.TernaryButtons;
using osuTK;
using osuTK.Input;

namespace osu.Game.Screens.Edit.Compose.Components
{
    /// <summary>
    /// A blueprint container generally displayed as an overlay to a ruleset's playfield.
    /// </summary>
    public partial class ComposeBlueprintContainer : EditorBlueprintContainer
    {
        private readonly Container<PlacementBlueprint> placementBlueprintContainer;

        protected new EditorSelectionHandler SelectionHandler => (EditorSelectionHandler)base.SelectionHandler;

        public PlacementBlueprint CurrentPlacement { get; private set; }

        public HitObjectPlacementBlueprint CurrentHitObjectPlacement => CurrentPlacement as HitObjectPlacementBlueprint;

        [Resolved(canBeNull: true)]
        private EditorScreenWithTimeline editorScreen { get; set; }

        /// <remarks>
        /// Positional input must be received outside the container's bounds,
        /// in order to handle composer blueprints which are partially offscreen.
        /// </remarks>
        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => editorScreen?.MainContent.ReceivePositionalInputAt(screenSpacePos) ?? base.ReceivePositionalInputAt(screenSpacePos);

        public ComposeBlueprintContainer(HitObjectComposer composer)
            : base(composer)
        {
            placementBlueprintContainer = new Container<PlacementBlueprint>
            {
                RelativeSizeAxes = Axes.Both
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            MainTernaryStates = CreateTernaryButtons().ToArray();
            SampleBankTernaryStates = createSampleBankTernaryButtons(SelectionHandler.SelectionBankStates).ToArray();
            SampleAdditionBankTernaryStates = createSampleBankTernaryButtons(SelectionHandler.SelectionAdditionBankStates).ToArray();

            SelectionHandler.AutoSelectionBankEnabled.BindValueChanged(_ => updateAutoBankTernaryButtonTooltip(), true);
            SelectionHandler.SelectionAdditionBanksEnabled.BindValueChanged(_ => updateAdditionBankTernaryButtonTooltips(), true);

            AddInternal(new DrawableRulesetDependenciesProvidingContainer(Composer.Ruleset)
            {
                Child = placementBlueprintContainer
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Beatmap.HitObjectAdded += hitObjectAdded;

            // updates to selected are handled for us by SelectionHandler.
            NewCombo.BindTo(SelectionHandler.SelectionNewComboState);

            // we are responsible for current placement blueprint updated based on state changes.
            NewCombo.ValueChanged += _ => updatePlacementNewCombo();

            // we own SelectionHandler so don't need to worry about making bindable copies (for simplicity)
            foreach (var kvp in SelectionHandler.SelectionSampleStates)
                kvp.Value.BindValueChanged(_ => updatePlacementSamples());

            foreach (var kvp in SelectionHandler.SelectionBankStates)
                kvp.Value.BindValueChanged(_ => updatePlacementSamples());

            foreach (var kvp in SelectionHandler.SelectionAdditionBankStates)
                kvp.Value.BindValueChanged(_ => updatePlacementSamples());
        }

        protected override void TransferBlueprintFor(HitObject hitObject, DrawableHitObject drawableObject)
        {
            base.TransferBlueprintFor(hitObject, drawableObject);

            var blueprint = (HitObjectSelectionBlueprint)GetBlueprintFor(hitObject);
            blueprint.DrawableObject = drawableObject;
        }

        private bool nudgeMovementActive;

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.ControlPressed)
            {
                switch (e.Key)
                {
                    case Key.Left:
                        nudgeSelection(new Vector2(-1, 0));
                        return true;

                    case Key.Right:
                        nudgeSelection(new Vector2(1, 0));
                        return true;

                    case Key.Up:
                        nudgeSelection(new Vector2(0, -1));
                        return true;

                    case Key.Down:
                        nudgeSelection(new Vector2(0, 1));
                        return true;
                }
            }

            return false;
        }

        protected override void OnKeyUp(KeyUpEvent e)
        {
            base.OnKeyUp(e);

            if (nudgeMovementActive && !e.ControlPressed)
            {
                Beatmap.EndChange();
                nudgeMovementActive = false;
            }
        }

        /// <summary>
        /// Move the current selection spatially by the specified delta, in gamefield coordinates (ie. the same coordinates as the blueprints).
        /// </summary>
        /// <param name="delta"></param>
        private void nudgeSelection(Vector2 delta)
        {
            if (!nudgeMovementActive)
            {
                nudgeMovementActive = true;
                Beatmap.BeginChange();
            }

            var firstBlueprint = SelectionHandler.SelectedBlueprints.FirstOrDefault();

            if (firstBlueprint == null)
                return;

            // convert to game space coordinates
            delta = firstBlueprint.ToScreenSpace(delta) - firstBlueprint.ToScreenSpace(Vector2.Zero);

            SelectionHandler.HandleMovement(new MoveSelectionEvent<HitObject>(firstBlueprint, delta));
        }

        private void updatePlacementNewCombo()
        {
            if (CurrentHitObjectPlacement?.HitObject is IHasComboInformation c)
                c.NewCombo = NewCombo.Value == TernaryState.True;
        }

        private void updatePlacementSamples()
        {
            if (CurrentHitObjectPlacement == null) return;

            foreach (var kvp in SelectionHandler.SelectionSampleStates)
                sampleChanged(kvp.Key, kvp.Value.Value);

            foreach (var kvp in SelectionHandler.SelectionBankStates)
                bankChanged(kvp.Key, kvp.Value.Value);

            foreach (var kvp in SelectionHandler.SelectionAdditionBankStates)
                additionBankChanged(kvp.Key, kvp.Value.Value);
        }

        private void sampleChanged(string sampleName, TernaryState state)
        {
            if (CurrentHitObjectPlacement == null) return;

            var samples = CurrentHitObjectPlacement.HitObject.Samples;

            var existingSample = samples.FirstOrDefault(s => s.Name == sampleName);

            switch (state)
            {
                case TernaryState.False:
                    if (existingSample != null)
                        samples.Remove(existingSample);
                    break;

                case TernaryState.True:
                    if (existingSample == null)
                        samples.Add(CurrentHitObjectPlacement.HitObject.CreateHitSampleInfo(sampleName));
                    break;
            }
        }

        private void bankChanged(string bankName, TernaryState state)
        {
            if (CurrentHitObjectPlacement == null) return;

            if (bankName == EditorSelectionHandler.HIT_BANK_AUTO)
                CurrentHitObjectPlacement.AutomaticBankAssignment = state == TernaryState.True;
            else if (state == TernaryState.True)
                CurrentHitObjectPlacement.HitObject.Samples = CurrentHitObjectPlacement.HitObject.Samples.Select(s => s.Name == HitSampleInfo.HIT_NORMAL ? s.With(newBank: bankName) : s).ToList();
        }

        private void additionBankChanged(string bankName, TernaryState state)
        {
            if (CurrentHitObjectPlacement == null) return;

            if (bankName == EditorSelectionHandler.HIT_BANK_AUTO)
                CurrentHitObjectPlacement.AutomaticAdditionBankAssignment = state == TernaryState.True;
            else if (state == TernaryState.True)
                CurrentHitObjectPlacement.HitObject.Samples = CurrentHitObjectPlacement.HitObject.Samples.Select(s => s.Name != HitSampleInfo.HIT_NORMAL ? s.With(newBank: bankName) : s).ToList();
        }

        public readonly Bindable<TernaryState> NewCombo = new Bindable<TernaryState> { Description = "New Combo" };

        /// <summary>
        /// A collection of states which will be displayed to the user in the toolbox.
        /// </summary>
        public TernaryButton[] MainTernaryStates { get; private set; }

        public TernaryButton[] SampleBankTernaryStates { get; private set; }

        public TernaryButton[] SampleAdditionBankTernaryStates { get; private set; }

        /// <summary>
        /// Create all ternary states required to be displayed to the user.
        /// </summary>
        protected virtual IEnumerable<TernaryButton> CreateTernaryButtons()
        {
            //TODO: this should only be enabled (visible?) for rulesets that provide combo-supporting HitObjects.
            yield return new TernaryButton(NewCombo, "New combo", () => new SpriteIcon { Icon = OsuIcon.EditorNewComboA });

            foreach (var kvp in SelectionHandler.SelectionSampleStates)
                yield return new TernaryButton(kvp.Value, kvp.Key.Replace("hit", string.Empty).Titleize(), () => GetIconForSample(kvp.Key));
        }

        private IEnumerable<TernaryButton> createSampleBankTernaryButtons(Dictionary<string, Bindable<TernaryState>> sampleBankStates)
        {
            foreach (var kvp in sampleBankStates)
                yield return new TernaryButton(kvp.Value, kvp.Key.Titleize(), () => getIconForBank(kvp.Key));
        }

        private Drawable getIconForBank(string sampleName)
        {
            return new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Y = -1,
                Font = OsuFont.Default.With(weight: FontWeight.Bold, size: 20),
                Text = $"{char.ToUpperInvariant(sampleName.First())}"
            };
        }

        public static Drawable GetIconForSample(string sampleName)
        {
            switch (sampleName)
            {
                case HitSampleInfo.HIT_CLAP:
                    return new SpriteIcon { Icon = FontAwesome.Solid.Hands };

                case HitSampleInfo.HIT_WHISTLE:
                    return new SpriteIcon { Icon = OsuIcon.EditorWhistle };

                case HitSampleInfo.HIT_FINISH:
                    return new SpriteIcon { Icon = OsuIcon.EditorFinish };
            }

            return null;
        }

        private void updateAutoBankTernaryButtonTooltip()
        {
            bool enabled = SelectionHandler.AutoSelectionBankEnabled.Value;

            var autoBankButton = SampleBankTernaryStates.Single(t => t.Bindable == SelectionHandler.SelectionBankStates[EditorSelectionHandler.HIT_BANK_AUTO]);
            autoBankButton.Enabled.Value = enabled;
            autoBankButton.Tooltip = !enabled ? "Auto normal bank can only be used during hit object placement" : string.Empty;
        }

        private void updateAdditionBankTernaryButtonTooltips()
        {
            bool enabled = SelectionHandler.SelectionAdditionBanksEnabled.Value;

            foreach (var ternaryButton in SampleAdditionBankTernaryStates)
            {
                ternaryButton.Enabled.Value = enabled;
                ternaryButton.Tooltip = !enabled ? "Add an addition sample first to be able to set a bank" : string.Empty;
            }
        }

        #region Placement

        /// <summary>
        /// Refreshes the current placement tool.
        /// </summary>
        private void refreshPlacement()
        {
            CurrentPlacement?.EndPlacement(false);
            CurrentPlacement?.Expire();
            CurrentPlacement = null;

            ensurePlacementCreated();
        }

        private void updatePlacementTimeAndPosition()
        {
            var snapResult = Composer.FindSnappedPositionAndTime(InputManager.CurrentState.Mouse.Position, CurrentPlacement.SnapType);

            // if no time was found from positional snapping, we should still quantize to the beat.
            snapResult.Time ??= Beatmap.SnapTime(EditorClock.CurrentTime, null);

            CurrentPlacement.UpdateTimeAndPosition(snapResult);
        }

        #endregion

        protected override void Update()
        {
            base.Update();

            if (CurrentPlacement != null)
            {
                switch (CurrentPlacement.PlacementActive)
                {
                    case PlacementBlueprint.PlacementState.Waiting:
                        if (!Composer.CursorInPlacementArea)
                            CurrentPlacement.Hide();
                        else
                            CurrentPlacement.Show();

                        break;

                    case PlacementBlueprint.PlacementState.Active:
                        CurrentPlacement.Show();
                        break;

                    case PlacementBlueprint.PlacementState.Finished:
                        refreshPlacement();
                        break;
                }

                // updates the placement with the latest editor clock time.
                updatePlacementTimeAndPosition();
            }
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            // updates the placement with the latest mouse position.
            if (CurrentPlacement != null)
                updatePlacementTimeAndPosition();

            return base.OnMouseMove(e);
        }

        protected sealed override SelectionBlueprint<HitObject> CreateBlueprintFor(HitObject item)
        {
            var drawable = Composer.HitObjects.FirstOrDefault(d => d.HitObject == item);

            if (drawable == null)
                return null;

            return CreateHitObjectBlueprintFor(item)?.With(b => b.DrawableObject = drawable);
        }

        [CanBeNull]
        public virtual HitObjectSelectionBlueprint CreateHitObjectBlueprintFor(HitObject hitObject) => null;

        private void hitObjectAdded(HitObject obj)
        {
            // refresh the tool to handle the case of placement completing.
            refreshPlacement();

            // on successful placement, the new combo button should be reset as this is the most common user interaction.
            if (Beatmap.SelectedHitObjects.Count == 0)
                NewCombo.Value = TernaryState.False;
        }

        private void ensurePlacementCreated()
        {
            if (CurrentPlacement != null) return;

            var blueprint = CurrentTool?.CreatePlacementBlueprint();

            if (blueprint != null)
            {
                placementBlueprintContainer.Child = CurrentPlacement = blueprint;

                // Fixes a 1-frame position discrepancy due to the first mouse move event happening in the next frame
                updatePlacementTimeAndPosition();

                updatePlacementSamples();

                updatePlacementNewCombo();
            }
        }

        public void CommitIfPlacementActive()
        {
            CurrentPlacement?.EndPlacement(CurrentPlacement.PlacementActive == PlacementBlueprint.PlacementState.Active);
            refreshPlacement();
        }

        private CompositionTool currentTool;

        /// <summary>
        /// The current placement tool.
        /// </summary>
        public CompositionTool CurrentTool
        {
            get => currentTool;

            set
            {
                if (currentTool == value)
                    return;

                currentTool = value;

                // As per stable editor, when changing tools, we should forcefully commit any pending placement.
                CommitIfPlacementActive();
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (Beatmap.IsNotNull())
                Beatmap.HitObjectAdded -= hitObjectAdded;
        }
    }
}
