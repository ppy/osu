// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.States;
using osu.Game.Audio;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components
{
    /// <summary>
    /// A component which outlines <see cref="DrawableHitObject"/>s and handles movement of selections.
    /// </summary>
    public class SelectionHandler : CompositeDrawable, IKeyBindingHandler<PlatformAction>, IHasContextMenu
    {
        public const float BORDER_RADIUS = 2;

        public IEnumerable<SelectionBlueprint> SelectedBlueprints => selectedBlueprints;
        private readonly List<SelectionBlueprint> selectedBlueprints;

        public IEnumerable<HitObject> SelectedHitObjects => selectedBlueprints.Select(b => b.HitObject);

        private Drawable outline;

        [Resolved(CanBeNull = true)]
        protected EditorBeatmap EditorBeatmap { get; private set; }

        [Resolved(CanBeNull = true)]
        private IEditorChangeHandler changeHandler { get; set; }

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
        /// Handles the selected <see cref="DrawableHitObject"/>s being moved.
        /// </summary>
        /// <remarks>
        /// Just returning true is enough to allow <see cref="HitObject.StartTime"/> updates to take place.
        /// Custom implementation is only required if other attributes are to be considered, like changing columns.
        /// </remarks>
        /// <param name="moveEvent">The move event.</param>
        /// <returns>
        /// Whether any <see cref="DrawableHitObject"/>s could be moved.
        /// Returning true will also propagate StartTime changes provided by the closest <see cref="IPositionSnapProvider.SnapScreenSpacePositionToValidTime"/>.
        /// </returns>
        public virtual bool HandleMovement(MoveSelectionEvent moveEvent) => true;

        public bool OnPressed(PlatformAction action)
        {
            switch (action.ActionMethod)
            {
                case PlatformActionMethod.Delete:
                    deleteSelected();
                    return true;
            }

            return false;
        }

        public void OnReleased(PlatformAction action)
        {
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
        internal void HandleSelected(SelectionBlueprint blueprint)
        {
            selectedBlueprints.Add(blueprint);
            EditorBeatmap.SelectedHitObjects.Add(blueprint.HitObject);

            UpdateVisibility();
        }

        /// <summary>
        /// Handle a blueprint becoming deselected.
        /// </summary>
        /// <param name="blueprint">The blueprint.</param>
        internal void HandleDeselected(SelectionBlueprint blueprint)
        {
            selectedBlueprints.Remove(blueprint);
            EditorBeatmap.SelectedHitObjects.Remove(blueprint.HitObject);

            // We don't want to update visibility if > 0, since we may be deselecting blueprints during drag-selection
            if (selectedBlueprints.Count == 0)
                UpdateVisibility();
        }

        /// <summary>
        /// Handle a blueprint requesting selection.
        /// </summary>
        /// <param name="blueprint">The blueprint.</param>
        /// <param name="state">The input state at the point of selection.</param>
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
        }

        private void deleteSelected()
        {
            changeHandler?.BeginChange();

            foreach (var h in selectedBlueprints.ToList())
                EditorBeatmap?.Remove(h.HitObject);

            changeHandler?.EndChange();
        }

        #endregion

        #region Outline Display

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

        #endregion

        #region Sample Changes

        /// <summary>
        /// Adds a hit sample to all selected <see cref="HitObject"/>s.
        /// </summary>
        /// <param name="sampleName">The name of the hit sample.</param>
        public void AddHitSample(string sampleName)
        {
            changeHandler?.BeginChange();

            foreach (var h in SelectedHitObjects)
            {
                // Make sure there isn't already an existing sample
                if (h.Samples.Any(s => s.Name == sampleName))
                    continue;

                h.Samples.Add(new HitSampleInfo { Name = sampleName });
            }

            changeHandler?.EndChange();
        }

        /// <summary>
        /// Removes a hit sample from all selected <see cref="HitObject"/>s.
        /// </summary>
        /// <param name="sampleName">The name of the hit sample.</param>
        public void RemoveHitSample(string sampleName)
        {
            changeHandler?.BeginChange();

            foreach (var h in SelectedHitObjects)
                h.SamplesBindable.RemoveAll(s => s.Name == sampleName);

            changeHandler?.EndChange();
        }

        #endregion

        #region Context Menu

        public MenuItem[] ContextMenuItems
        {
            get
            {
                if (!selectedBlueprints.Any(b => b.IsHovered))
                    return Array.Empty<MenuItem>();

                var items = new List<MenuItem>();

                items.AddRange(GetContextMenuItemsForSelection(selectedBlueprints));

                if (selectedBlueprints.Count == 1)
                    items.AddRange(selectedBlueprints[0].ContextMenuItems);

                items.AddRange(new[]
                {
                    new OsuMenuItem("Sound")
                    {
                        Items = new[]
                        {
                            createHitSampleMenuItem("Whistle", HitSampleInfo.HIT_WHISTLE),
                            createHitSampleMenuItem("Clap", HitSampleInfo.HIT_CLAP),
                            createHitSampleMenuItem("Finish", HitSampleInfo.HIT_FINISH)
                        }
                    },
                    new OsuMenuItem("Delete", MenuItemType.Destructive, deleteSelected),
                });

                return items.ToArray();
            }
        }

        /// <summary>
        /// Provide context menu items relevant to current selection. Calling base is not required.
        /// </summary>
        /// <param name="selection">The current selection.</param>
        /// <returns>The relevant menu items.</returns>
        protected virtual IEnumerable<MenuItem> GetContextMenuItemsForSelection(IEnumerable<SelectionBlueprint> selection)
            => Enumerable.Empty<MenuItem>();

        private MenuItem createHitSampleMenuItem(string name, string sampleName)
        {
            return new TernaryStateMenuItem(name, MenuItemType.Standard, setHitSampleState)
            {
                State = { Value = getHitSampleState() }
            };

            void setHitSampleState(TernaryState state)
            {
                switch (state)
                {
                    case TernaryState.False:
                        RemoveHitSample(sampleName);
                        break;

                    case TernaryState.True:
                        AddHitSample(sampleName);
                        break;
                }
            }

            TernaryState getHitSampleState()
            {
                int countExisting = SelectedHitObjects.Count(h => h.Samples.Any(s => s.Name == sampleName));

                if (countExisting == 0)
                    return TernaryState.False;

                if (countExisting < SelectedHitObjects.Count())
                    return TernaryState.Indeterminate;

                return TernaryState.True;
            }
        }

        #endregion
    }
}
