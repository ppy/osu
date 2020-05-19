// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.Edit
{
    /// <summary>
    /// A blueprint placed above a <see cref="DrawableHitObject"/> adding editing functionality.
    /// </summary>
    public abstract class SelectionBlueprint : CompositeDrawable, IStateful<SelectionState>
    {
        public readonly HitObject HitObject;

        /// <summary>
        /// Invoked when this <see cref="SelectionBlueprint"/> has been selected.
        /// </summary>
        public event Action<SelectionBlueprint> Selected;

        /// <summary>
        /// Invoked when this <see cref="SelectionBlueprint"/> has been deselected.
        /// </summary>
        public event Action<SelectionBlueprint> Deselected;

        public override bool HandlePositionalInput => ShouldBeAlive;
        public override bool RemoveWhenNotAlive => false;

        [Resolved(CanBeNull = true)]
        private HitObjectComposer composer { get; set; }

        protected SelectionBlueprint(HitObject hitObject)
        {
            HitObject = hitObject;

            RelativeSizeAxes = Axes.Both;
            AlwaysPresent = true;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateState();
        }

        private SelectionState state;

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

        protected virtual void OnDeselected() => Hide();

        protected virtual void OnSelected() => Show();

        // When not selected, input is only required for the blueprint itself to receive IsHovering
        protected override bool ShouldBeConsideredForInput(Drawable child) => State == SelectionState.Selected;

        /// <summary>
        /// Selects this <see cref="OverlaySelectionBlueprint"/>, causing it to become visible.
        /// </summary>
        public void Select() => State = SelectionState.Selected;

        /// <summary>
        /// Deselects this <see cref="OverlaySelectionBlueprint"/>, causing it to become invisible.
        /// </summary>
        public void Deselect() => State = SelectionState.NotSelected;

        public bool IsSelected => State == SelectionState.Selected;

        /// <summary>
        /// The <see cref="MenuItem"/>s to be displayed in the context menu for this <see cref="OverlaySelectionBlueprint"/>.
        /// </summary>
        public virtual MenuItem[] ContextMenuItems => Array.Empty<MenuItem>();

        /// <summary>
        /// The screen-space point that causes this <see cref="OverlaySelectionBlueprint"/> to be selected.
        /// </summary>
        public virtual Vector2 SelectionPoint => ScreenSpaceDrawQuad.Centre;

        /// <summary>
        /// The screen-space quad that outlines this <see cref="OverlaySelectionBlueprint"/> for selections.
        /// </summary>
        public virtual Quad SelectionQuad => ScreenSpaceDrawQuad;

        public virtual Vector2 GetInstantDelta(Vector2 screenSpacePosition) => Parent.ToLocalSpace(screenSpacePosition) - Position;
    }
}
