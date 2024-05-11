// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Rulesets.Edit
{
    /// <summary>
    /// A blueprint placed above a displaying item adding editing functionality.
    /// </summary>
    public abstract partial class SelectionBlueprint<T> : CompositeDrawable, IStateful<SelectionState>
    {
        public readonly T Item;

        /// <summary>
        /// Invoked when this <see cref="SelectionBlueprint{T}"/> has been selected.
        /// </summary>
        public event Action<SelectionBlueprint<T>> Selected;

        /// <summary>
        /// Invoked when this <see cref="SelectionBlueprint{T}"/> has been deselected.
        /// </summary>
        public event Action<SelectionBlueprint<T>> Deselected;

        public override bool HandlePositionalInput => IsSelectable;
        public override bool RemoveWhenNotAlive => false;

        protected SelectionBlueprint(T item)
        {
            Item = item;

            RelativeSizeAxes = Axes.Both;
            AlwaysPresent = true;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateState();
        }

        private SelectionState state;

        [CanBeNull]
        public event Action<SelectionState> StateChanged;

        public SelectionState State
        {
            get => state;
            set
            {
                if (state == value)
                    return;

                state = value;

                if (IsLoaded)
                    updateState();

                StateChanged?.Invoke(state);
            }
        }

        private void updateState()
        {
            switch (state)
            {
                case SelectionState.Selected:
                    OnSelected();
                    Selected?.Invoke(this);
                    break;

                case SelectionState.NotSelected:
                    OnDeselected();
                    Deselected?.Invoke(this);
                    break;
            }
        }

        protected virtual void OnDeselected()
        {
            // selection blueprints are AlwaysPresent while the related item is visible
            // set the body piece's alpha directly to avoid arbitrarily rendering frame buffers etc. of children.
            foreach (var d in InternalChildren)
                d.Hide();
        }

        protected virtual void OnSelected()
        {
            foreach (var d in InternalChildren)
                d.Show();
        }

        // When not selected, input is only required for the blueprint itself to receive IsHovering
        protected override bool ShouldBeConsideredForInput(Drawable child) => State == SelectionState.Selected;

        /// <summary>
        /// Selects this <see cref="SelectionBlueprint{T}"/>, causing it to become visible.
        /// </summary>
        public void Select() => State = SelectionState.Selected;

        /// <summary>
        /// Deselects this <see cref="HitObjectSelectionBlueprint"/>, causing it to become invisible.
        /// </summary>
        public void Deselect() => State = SelectionState.NotSelected;

        /// <summary>
        /// Toggles the selection state of this <see cref="HitObjectSelectionBlueprint"/>.
        /// </summary>
        public void ToggleSelection() => State = IsSelected ? SelectionState.NotSelected : SelectionState.Selected;

        public bool IsSelected => State == SelectionState.Selected;

        /// <summary>
        /// The <see cref="MenuItem"/>s to be displayed in the context menu for this <see cref="HitObjectSelectionBlueprint"/>.
        /// </summary>
        public virtual MenuItem[] ContextMenuItems => Array.Empty<MenuItem>();

        /// <summary>
        /// Whether the <see cref="SelectionBlueprint{T}"/> can be currently selected via a click or a drag box.
        /// </summary>
        public virtual bool IsSelectable => ShouldBeAlive && IsPresent;

        /// <summary>
        /// The screen-space main point that causes this <see cref="HitObjectSelectionBlueprint"/> to be selected via a drag.
        /// </summary>
        public virtual Vector2 ScreenSpaceSelectionPoint => ScreenSpaceDrawQuad.Centre;

        /// <summary>
        /// Any points that should be used for snapping purposes in addition to <see cref="ScreenSpaceSelectionPoint"/>. Exposed via <see cref="ScreenSpaceSnapPoints"/>.
        /// </summary>
        protected virtual Vector2[] ScreenSpaceAdditionalNodes => Array.Empty<Vector2>();

        /// <summary>
        /// The screen-space collection of base points on this <see cref="HitObjectSelectionBlueprint"/> that other objects can be snapped to.
        /// The first element of this collection is <see cref="ScreenSpaceSelectionPoint"/>
        /// </summary>
        public Vector2[] ScreenSpaceSnapPoints => ScreenSpaceAdditionalNodes.Prepend(ScreenSpaceSelectionPoint).ToArray();

        /// <summary>
        /// The screen-space quad that outlines this <see cref="HitObjectSelectionBlueprint"/> for selections.
        /// </summary>
        public virtual Quad SelectionQuad => ScreenSpaceDrawQuad;

        /// <summary>
        /// Handle to perform a partial deletion when the user requests a quick delete (Shift+Right Click).
        /// </summary>
        /// <returns>True if the deletion operation was handled by this blueprint. Returning false will delete the full blueprint.</returns>
        public virtual bool HandleQuickDeletion() => false;
    }
}
