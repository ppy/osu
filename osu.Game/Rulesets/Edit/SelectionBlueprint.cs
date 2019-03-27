// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Input.Events;
using osu.Framework.Input.States;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.Edit
{
    /// <summary>
    /// A blueprint placed above a <see cref="DrawableHitObject"/> adding editing functionality.
    /// </summary>
    public abstract class SelectionBlueprint : CompositeDrawable, IStateful<SelectionState>
    {
        /// <summary>
        /// Invoked when this <see cref="SelectionBlueprint"/> has been selected.
        /// </summary>
        public event Action<SelectionBlueprint> Selected;

        /// <summary>
        /// Invoked when this <see cref="SelectionBlueprint"/> has been deselected.
        /// </summary>
        public event Action<SelectionBlueprint> Deselected;

        /// <summary>
        /// Invoked when this <see cref="SelectionBlueprint"/> has requested selection.
        /// Will fire even if already selected. Does not actually perform selection.
        /// </summary>
        public event Action<SelectionBlueprint, InputState> SelectionRequested;

        /// <summary>
        /// Invoked when this <see cref="SelectionBlueprint"/> has requested drag.
        /// </summary>
        public event Action<SelectionBlueprint, DragEvent> DragRequested;

        /// <summary>
        /// The <see cref="DrawableHitObject"/> which this <see cref="SelectionBlueprint"/> applies to.
        /// </summary>
        public readonly DrawableHitObject HitObject;

        protected override bool ShouldBeAlive => HitObject.IsAlive && HitObject.IsPresent || State == SelectionState.Selected;
        public override bool HandlePositionalInput => ShouldBeAlive;
        public override bool RemoveWhenNotAlive => false;

        protected SelectionBlueprint(DrawableHitObject hitObject)
        {
            HitObject = hitObject;

            RelativeSizeAxes = Axes.Both;

            AlwaysPresent = true;
            Alpha = 0;
        }

        private SelectionState state;

        public event Action<SelectionState> StateChanged;

        public SelectionState State
        {
            get => state;
            set
            {
                if (state == value) return;

                state = value;
                switch (state)
                {
                    case SelectionState.Selected:
                        Show();
                        Selected?.Invoke(this);
                        break;
                    case SelectionState.NotSelected:
                        Hide();
                        Deselected?.Invoke(this);
                        break;
                }
            }
        }

        /// <summary>
        /// Selects this <see cref="SelectionBlueprint"/>, causing it to become visible.
        /// </summary>
        public void Select() => State = SelectionState.Selected;

        /// <summary>
        /// Deselects this <see cref="SelectionBlueprint"/>, causing it to become invisible.
        /// </summary>
        public void Deselect() => State = SelectionState.NotSelected;

        public bool IsSelected => State == SelectionState.Selected;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => HitObject.ReceivePositionalInputAt(screenSpacePos);

        private bool selectionRequested;

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            selectionRequested = false;

            if (State == SelectionState.NotSelected)
            {
                SelectionRequested?.Invoke(this, e.CurrentState);
                selectionRequested = true;
            }

            return IsSelected;
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (State == SelectionState.Selected && !selectionRequested)
            {
                selectionRequested = true;
                SelectionRequested?.Invoke(this, e.CurrentState);
                return true;
            }

            return base.OnClick(e);
        }

        protected override bool OnDragStart(DragStartEvent e) => true;

        protected override bool OnDrag(DragEvent e)
        {
            DragRequested?.Invoke(this, e);
            return true;
        }

        /// <summary>
        /// The screen-space point that causes this <see cref="SelectionBlueprint"/> to be selected.
        /// </summary>
        public virtual Vector2 SelectionPoint => HitObject.ScreenSpaceDrawQuad.Centre;

        /// <summary>
        /// The screen-space quad that outlines this <see cref="SelectionBlueprint"/> for selections.
        /// </summary>
        public virtual Quad SelectionQuad => HitObject.ScreenSpaceDrawQuad;
    }
}
