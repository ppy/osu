// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Game.Audio;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Screens.Edit.Components.TernaryButtons;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components
{
    /// <summary>
    /// A blueprint container generally displayed as an overlay to a ruleset's playfield.
    /// </summary>
    public class ComposeBlueprintContainer : BlueprintContainer
    {
        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

        private readonly Container<PlacementBlueprint> placementBlueprintContainer;

        private PlacementBlueprint currentPlacement;
        private InputManager inputManager;

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
            TernaryStates = CreateTernaryButtons().ToArray();

            AddInternal(placementBlueprintContainer);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            inputManager = GetContainingInputManager();

            // updates to selected are handled for us by SelectionHandler.
            NewCombo.BindTo(SelectionHandler.SelectionNewComboState);

            // we are responsible for current placement blueprint updated based on state changes.
            NewCombo.ValueChanged += _ => updatePlacementNewCombo();

            // we own SelectionHandler so don't need to worry about making bindable copies (for simplicity)
            foreach (var kvp in SelectionHandler.SelectionSampleStates)
            {
                kvp.Value.BindValueChanged(_ => updatePlacementSamples());
            }
        }

        private void updatePlacementNewCombo()
        {
            if (currentPlacement?.HitObject is IHasComboInformation c)
                c.NewCombo = NewCombo.Value == TernaryState.True;
        }

        private void updatePlacementSamples()
        {
            if (currentPlacement == null) return;

            foreach (var kvp in SelectionHandler.SelectionSampleStates)
                sampleChanged(kvp.Key, kvp.Value.Value);
        }

        private void sampleChanged(string sampleName, TernaryState state)
        {
            if (currentPlacement == null) return;

            var samples = currentPlacement.HitObject.Samples;

            var existingSample = samples.FirstOrDefault(s => s.Name == sampleName);

            switch (state)
            {
                case TernaryState.False:
                    if (existingSample != null)
                        samples.Remove(existingSample);
                    break;

                case TernaryState.True:
                    if (existingSample == null)
                        samples.Add(new HitSampleInfo(sampleName));
                    break;
            }
        }

        public readonly Bindable<TernaryState> NewCombo = new Bindable<TernaryState> { Description = "New Combo" };

        /// <summary>
        /// A collection of states which will be displayed to the user in the toolbox.
        /// </summary>
        public TernaryButton[] TernaryStates { get; private set; }

        /// <summary>
        /// Create all ternary states required to be displayed to the user.
        /// </summary>
        protected virtual IEnumerable<TernaryButton> CreateTernaryButtons()
        {
            //TODO: this should only be enabled (visible?) for rulesets that provide combo-supporting HitObjects.
            yield return new TernaryButton(NewCombo, "New combo", () => new SpriteIcon { Icon = FontAwesome.Regular.DotCircle });

            foreach (var kvp in SelectionHandler.SelectionSampleStates)
                yield return new TernaryButton(kvp.Value, kvp.Key.Replace("hit", string.Empty).Titleize(), () => getIconForSample(kvp.Key));
        }

        private Drawable getIconForSample(string sampleName)
        {
            switch (sampleName)
            {
                case HitSampleInfo.HIT_CLAP:
                    return new SpriteIcon { Icon = FontAwesome.Solid.Hands };

                case HitSampleInfo.HIT_WHISTLE:
                    return new SpriteIcon { Icon = FontAwesome.Solid.Bullhorn };

                case HitSampleInfo.HIT_FINISH:
                    return new SpriteIcon { Icon = FontAwesome.Solid.DrumSteelpan };
            }

            return null;
        }

        #region Placement

        /// <summary>
        /// Refreshes the current placement tool.
        /// </summary>
        private void refreshTool()
        {
            removePlacement();
            createPlacement();
        }

        private void updatePlacementPosition()
        {
            var snapResult = Composer.SnapScreenSpacePositionToValidTime(inputManager.CurrentState.Mouse.Position);

            // if no time was found from positional snapping, we should still quantize to the beat.
            snapResult.Time ??= Beatmap.SnapTime(EditorClock.CurrentTime, null);

            currentPlacement.UpdateTimeAndPosition(snapResult);
        }

        #endregion

        protected override void Update()
        {
            base.Update();

            if (Composer.CursorInPlacementArea)
                createPlacement();
            else if (currentPlacement?.PlacementActive == false)
                removePlacement();

            if (currentPlacement != null)
            {
                updatePlacementPosition();
            }
        }

        protected sealed override SelectionBlueprint CreateBlueprintFor(HitObject hitObject)
        {
            var drawable = Composer.HitObjects.FirstOrDefault(d => d.HitObject == hitObject);

            if (drawable == null)
                return null;

            return CreateBlueprintFor(drawable);
        }

        public virtual OverlaySelectionBlueprint CreateBlueprintFor(DrawableHitObject hitObject) => null;

        protected override void OnBlueprintAdded(HitObject hitObject)
        {
            base.OnBlueprintAdded(hitObject);

            refreshTool();

            // on successful placement, the new combo button should be reset as this is the most common user interaction.
            if (Beatmap.SelectedHitObjects.Count == 0)
                NewCombo.Value = TernaryState.False;
        }

        private void createPlacement()
        {
            if (currentPlacement != null) return;

            var blueprint = CurrentTool?.CreatePlacementBlueprint();

            if (blueprint != null)
            {
                // doing this post-creations as adding the default hit sample should be the case regardless of the ruleset.
                blueprint.HitObject.Samples.Add(new HitSampleInfo(HitSampleInfo.HIT_NORMAL));

                placementBlueprintContainer.Child = currentPlacement = blueprint;

                // Fixes a 1-frame position discrepancy due to the first mouse move event happening in the next frame
                updatePlacementPosition();

                updatePlacementSamples();

                updatePlacementNewCombo();
            }
        }

        private void removePlacement()
        {
            if (currentPlacement == null) return;

            currentPlacement.EndPlacement(false);
            currentPlacement.Expire();
            currentPlacement = null;
        }

        private HitObjectCompositionTool currentTool;

        /// <summary>
        /// The current placement tool.
        /// </summary>
        public HitObjectCompositionTool CurrentTool
        {
            get => currentTool;

            set
            {
                if (currentTool == value)
                    return;

                currentTool = value;

                refreshTool();
            }
        }
    }
}
