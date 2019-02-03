// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Input.States;
using osu.Game.Graphics;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK;
using osuTK.Input;

namespace osu.Game.Screens.Edit.Compose.Components
{
    /// <summary>
    /// A component which outlines <see cref="DrawableHitObject"/>s and handles movement of selections.
    /// </summary>
    public class SelectionHandler : CompositeDrawable
    {
        public const float BORDER_RADIUS = 2;

        protected IEnumerable<SelectionBlueprint> SelectedBlueprints => selectedBlueprints;
        private readonly List<SelectionBlueprint> selectedBlueprints;

        protected IEnumerable<HitObject> SelectedHitObjects => selectedBlueprints.Select(b => b.HitObject.HitObject);

        private Drawable outline;

        [Resolved]
        private IPlacementHandler placementHandler { get; set; }

        public SelectionHandler()
        {
            selectedBlueprints = new List<SelectionBlueprint>();

            RelativeSizeAxes = Axes.Both;
            AlwaysPresent = true;
            Alpha = 0;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            InternalChild = outline = new Container
            {
                Masking = true,
                BorderThickness = BORDER_RADIUS,
                BorderColour = colours.Yellow,
                Child = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    AlwaysPresent = true,
                    Alpha = 0
                }
            };
        }

        #region User Input Handling

        /// <summary>
        /// Handles the selected <see cref="DrawableHitObject"/>s being dragged.
        /// </summary>
        /// <param name="blueprint">The <see cref="SelectionBlueprint"/> that received the drag event.</param>
        /// <param name="dragEvent">The drag event.</param>
        public virtual void HandleDrag(SelectionBlueprint blueprint, DragEvent dragEvent)
        {
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.Repeat)
                return base.OnKeyDown(e);

            switch (e.Key)
            {
                case Key.Delete:
                    foreach (var h in selectedBlueprints.ToList())
                        placementHandler.Delete(h.HitObject.HitObject);
                    return true;
            }

            return base.OnKeyDown(e);
        }

        #endregion

        #region Selection Handling

        /// <summary>
        /// Bind an action to deselect all selected blueprints.
        /// </summary>
        internal Action DeselectAll { private get; set; }

        /// <summary>
        /// Handle a blueprint becoming selected.
        /// </summary>
        /// <param name="blueprint">The blueprint.</param>
        internal void HandleSelected(SelectionBlueprint blueprint) => selectedBlueprints.Add(blueprint);

        /// <summary>
        /// Handle a blueprint becoming deselected.
        /// </summary>
        /// <param name="blueprint">The blueprint.</param>
        internal void HandleDeselected(SelectionBlueprint blueprint)
        {
            selectedBlueprints.Remove(blueprint);

            // We don't want to update visibility if > 0, since we may be deselecting blueprints during drag-selection
            if (selectedBlueprints.Count == 0)
                UpdateVisibility();
        }

        /// <summary>
        /// Handle a blueprint requesting selection.
        /// </summary>
        /// <param name="blueprint">The blueprint.</param>
        internal void HandleSelectionRequested(SelectionBlueprint blueprint, InputState state)
        {
            if (state.Keyboard.ControlPressed)
            {
                if (blueprint.IsSelected)
                    blueprint.Deselect();
                else
                    blueprint.Select();
            }
            else
            {
                if (blueprint.IsSelected)
                    return;

                DeselectAll?.Invoke();
                blueprint.Select();
            }

            UpdateVisibility();
        }

        #endregion

        /// <summary>
        /// Updates whether this <see cref="SelectionHandler"/> is visible.
        /// </summary>
        internal void UpdateVisibility()
        {
            if (selectedBlueprints.Count > 0)
                Show();
            else
                Hide();
        }

        protected override void Update()
        {
            base.Update();

            if (selectedBlueprints.Count == 0)
                return;

            // Move the rectangle to cover the hitobjects
            var topLeft = new Vector2(float.MaxValue, float.MaxValue);
            var bottomRight = new Vector2(float.MinValue, float.MinValue);

            bool hasSelection = false;

            foreach (var blueprint in selectedBlueprints)
            {
                topLeft = Vector2.ComponentMin(topLeft, ToLocalSpace(blueprint.SelectionQuad.TopLeft));
                bottomRight = Vector2.ComponentMax(bottomRight, ToLocalSpace(blueprint.SelectionQuad.BottomRight));
            }

            topLeft -= new Vector2(5);
            bottomRight += new Vector2(5);

            outline.Size = bottomRight - topLeft;
            outline.Position = topLeft;
        }
    }
}
