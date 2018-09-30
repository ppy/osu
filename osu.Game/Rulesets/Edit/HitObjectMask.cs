// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Input.EventArgs;
using osu.Framework.Input.States;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Objects.Drawables;
using OpenTK;

namespace osu.Game.Rulesets.Edit
{
    /// <summary>
    /// A mask placed above a <see cref="DrawableHitObject"/> adding editing functionality.
    /// </summary>
    public class HitObjectMask : CompositeDrawable, IStateful<SelectionState>
    {
        /// <summary>
        /// Invoked when this <see cref="HitObjectMask"/> has been selected.
        /// </summary>
        public event Action<HitObjectMask> Selected;

        /// <summary>
        /// Invoked when this <see cref="HitObjectMask"/> has been deselected.
        /// </summary>
        public event Action<HitObjectMask> Deselected;

        /// <summary>
        /// Invoked when this <see cref="HitObjectMask"/> has requested selection.
        /// Will fire even if already selected. Does not actually perform selection.
        /// </summary>
        public event Action<HitObjectMask, InputState> SelectionRequested;

        /// <summary>
        /// Invoked when this <see cref="HitObjectMask"/> has requested drag.
        /// </summary>
        public event Action<HitObjectMask, InputState> DragRequested;

        /// <summary>
        /// The <see cref="DrawableHitObject"/> which this <see cref="HitObjectMask"/> applies to.
        /// </summary>
        public readonly DrawableHitObject HitObject;

        protected override bool ShouldBeAlive => HitObject.IsAlive && HitObject.IsPresent || State == SelectionState.Selected;
        public override bool HandlePositionalInput => ShouldBeAlive;
        public override bool RemoveWhenNotAlive => false;

        public HitObjectMask(DrawableHitObject hitObject)
        {
            HitObject = hitObject;

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
        /// Selects this <see cref="HitObjectMask"/>, causing it to become visible.
        /// </summary>
        public void Select() => State = SelectionState.Selected;

        /// <summary>
        /// Deselects this <see cref="HitObjectMask"/>, causing it to become invisible.
        /// </summary>
        public void Deselect() => State = SelectionState.NotSelected;

        public bool IsSelected => State == SelectionState.Selected;

        private bool selectionRequested;

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            selectionRequested = false;

            if (State == SelectionState.NotSelected)
            {
                SelectionRequested?.Invoke(this, state);
                selectionRequested = true;
            }

            return IsSelected;
        }

        protected override bool OnClick(InputState state)
        {
            if (State == SelectionState.Selected && !selectionRequested)
            {
                selectionRequested = true;
                SelectionRequested?.Invoke(this, state);
                return true;
            }

            return base.OnClick(state);
        }

        protected override bool OnDragStart(InputState state) => true;

        protected override bool OnDrag(InputState state)
        {
            DragRequested?.Invoke(this, state);
            return true;
        }

        /// <summary>
        /// The screen-space point that causes this <see cref="HitObjectMask"/> to be selected.
        /// </summary>
        public virtual Vector2 SelectionPoint => ScreenSpaceDrawQuad.Centre;

        /// <summary>
        /// The screen-space quad that outlines this <see cref="HitObjectMask"/> for selections.
        /// </summary>
        public virtual Quad SelectionQuad => ScreenSpaceDrawQuad;
    }
}
