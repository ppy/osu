// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
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

namespace osu.Game.Screens.Edit.Compose.Components
{
    /// <summary>
    /// A blueprint container generally displayed as an overlay to a ruleset's playfield.
    /// </summary>
    public abstract partial class ComposeBlueprintContainer : EditorBlueprintContainer
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

        protected override IEnumerable<SelectionBlueprint<HitObject>> ApplySelectionOrder(IEnumerable<SelectionBlueprint<HitObject>> blueprints) =>
            base.ApplySelectionOrder(blueprints)
                .OrderBy(b => Math.Min(Math.Abs(EditorClock.CurrentTime - b.Item.GetEndTime()), Math.Abs(EditorClock.CurrentTime - b.Item.StartTime)));

        protected ComposeBlueprintContainer(HitObjectComposer composer)
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
            SampleBankTernaryStates = createSampleBankTernaryButtons().ToArray();

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

            SelectionHandler.AutoSelectionBankEnabled.BindValueChanged(_ => updateAutoBankTernaryButtonTooltip(), true);
            SelectionHandler.SelectionAdditionBanksEnabled.BindValueChanged(_ => updateAdditionBankTernaryButtonTooltips(), true);
        }

        protected override void TransferBlueprintFor(HitObject hitObject, DrawableHitObject drawableObject)
        {
            base.TransferBlueprintFor(hitObject, drawableObject);

            var blueprint = (HitObjectSelectionBlueprint)GetBlueprintFor(hitObject);
            blueprint.DrawableObject = drawableObject;
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
        public Drawable[] MainTernaryStates { get; private set; }

        public SampleBankTernaryButton[] SampleBankTernaryStates { get; private set; }

        /// <summary>
        /// Create all ternary states required to be displayed to the user.
        /// </summary>
        protected virtual IEnumerable<Drawable> CreateTernaryButtons()
        {
            //TODO: this should only be enabled (visible?) for rulesets that provide combo-supporting HitObjects.
            yield return new NewComboTernaryButton { Current = NewCombo };

            foreach (var kvp in SelectionHandler.SelectionSampleStates)
            {
                yield return new DrawableTernaryButton
                {
                    Current = kvp.Value,
                    Description = kvp.Key.Replace(@"hit", string.Empty).Titleize(),
                    CreateIcon = () => GetIconForSample(kvp.Key),
                };
            }
        }

        private IEnumerable<SampleBankTernaryButton> createSampleBankTernaryButtons()
        {
            foreach (string bankName in HitSampleInfo.ALL_BANKS.Prepend(EditorSelectionHandler.HIT_BANK_AUTO))
            {
                yield return new SampleBankTernaryButton(bankName)
                {
                    NormalState = { Current = SelectionHandler.SelectionBankStates[bankName], },
                    AdditionsState = { Current = SelectionHandler.SelectionAdditionBankStates[bankName], },
                    CreateIcon = () => getIconForBank(bankName)
                };
            }
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

            var autoBankButton = SampleBankTernaryStates.Single(t => t.BankName == EditorSelectionHandler.HIT_BANK_AUTO);
            autoBankButton.NormalButton.Enabled.Value = enabled;
            autoBankButton.NormalButton.TooltipText = !enabled ? "Auto normal bank can only be used during hit object placement" : string.Empty;
        }

        private void updateAdditionBankTernaryButtonTooltips()
        {
            bool enabled = SelectionHandler.SelectionAdditionBanksEnabled.Value;

            foreach (var ternaryButton in SampleBankTernaryStates)
            {
                ternaryButton.AdditionsButton.Enabled.Value = enabled;
                ternaryButton.AdditionsButton.TooltipText = !enabled ? "Add an addition sample first to be able to set a bank" : string.Empty;
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
            CurrentPlacement.UpdateTimeAndPosition(InputManager.CurrentState.Mouse.Position, Beatmap.SnapTime(EditorClock.CurrentTime, null));
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

                SelectionHandler.RightClickAlwaysQuickDeletes = currentTool is not SelectTool;

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
