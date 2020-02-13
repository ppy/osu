// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components
{
    /// <summary>
    /// A blueprint container generally displayed as an overlay to a ruleset's playfield.
    /// </summary>
    public class ComposeBlueprintContainer : BlueprintContainer
    {
        [Resolved]
        private HitObjectComposer composer { get; set; }

        private PlacementBlueprint currentPlacement;

        private readonly Container<PlacementBlueprint> placementBlueprintContainer;

        private InputManager inputManager;

        private readonly IEnumerable<DrawableHitObject> drawableHitObjects;

        public ComposeBlueprintContainer(IEnumerable<DrawableHitObject> drawableHitObjects)
        {
            this.drawableHitObjects = drawableHitObjects;

            placementBlueprintContainer = new Container<PlacementBlueprint>
            {
                RelativeSizeAxes = Axes.Both
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(placementBlueprintContainer);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            inputManager = GetContainingInputManager();
        }

        #region Placement

        /// <summary>
        /// Refreshes the current placement tool.
        /// </summary>
        private void refreshTool()
        {
            placementBlueprintContainer.Clear();

            currentPlacement?.EndPlacement(false);
            currentPlacement = null;

            var blueprint = CurrentTool?.CreatePlacementBlueprint();

            if (blueprint != null)
            {
                placementBlueprintContainer.Child = currentPlacement = blueprint;

                // Fixes a 1-frame position discrepancy due to the first mouse move event happening in the next frame
                updatePlacementPosition(inputManager.CurrentState.Mouse.Position);
            }
        }

        private void updatePlacementPosition(Vector2 screenSpacePosition)
        {
            Vector2 snappedGridPosition = composer.GetSnappedPosition(ToLocalSpace(screenSpacePosition), 0).position;
            Vector2 snappedScreenSpacePosition = ToScreenSpace(snappedGridPosition);

            currentPlacement.UpdatePosition(snappedScreenSpacePosition);
        }

        #endregion

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            if (currentPlacement != null)
            {
                updatePlacementPosition(e.ScreenSpaceMousePosition);
                return true;
            }

            return base.OnMouseMove(e);
        }

        protected override void Update()
        {
            base.Update();

            if (currentPlacement != null)
            {
                if (composer.CursorInPlacementArea)
                    currentPlacement.State = PlacementState.Shown;
                else if (currentPlacement?.PlacementBegun == false)
                    currentPlacement.State = PlacementState.Hidden;
            }
        }

        protected sealed override SelectionBlueprint CreateBlueprintFor(HitObject hitObject)
        {
            var drawable = drawableHitObjects.FirstOrDefault(d => d.HitObject == hitObject);
            if (drawable == null)
                return null;

            return CreateBlueprintFor(drawable);
        }

        public virtual OverlaySelectionBlueprint CreateBlueprintFor(DrawableHitObject hitObject) => null;

        protected override void AddBlueprintFor(HitObject hitObject)
        {
            refreshTool();
            base.AddBlueprintFor(hitObject);
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
