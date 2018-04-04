// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Input;
using osu.Game.Rulesets.Objects.Drawables;
using OpenTK;

namespace osu.Game.Rulesets.Edit
{
    /// <summary>
    /// A mask placed above a <see cref="DrawableHitObject"/> adding editing functionality.
    /// </summary>
    public class HitObjectMask : VisibilityContainer
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
        /// The <see cref="DrawableHitObject"/> which this <see cref="HitObjectMask"/> applies to.
        /// </summary>
        public readonly DrawableHitObject HitObject;

        protected override bool ShouldBeAlive => HitObject.IsAlive && HitObject.IsPresent || State == Visibility.Visible;
        public override bool HandleMouseInput => ShouldBeAlive;
        public override bool RemoveWhenNotAlive => false;

        public HitObjectMask(DrawableHitObject hitObject)
        {
            HitObject = hitObject;

            AlwaysPresent = true;
            State = Visibility.Hidden;
        }

        /// <summary>
        /// Selects this <see cref="HitObjectMask"/>, causing it to become visible.
        /// </summary>
        /// <returns>True if the <see cref="HitObjectMask"/> was selected. False if the <see cref="HitObjectMask"/> was already selected.</returns>
        public bool Select()
        {
            if (State == Visibility.Visible)
                return false;

            Show();
            Selected?.Invoke(this);
            return true;
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            SelectionRequested?.Invoke(this, state);
            return base.OnMouseDown(state, args);
        }

        /// <summary>
        /// Deselects this <see cref="HitObjectMask"/>, causing it to become invisible.
        /// </summary>
        /// <returns>True if the <see cref="HitObjectMask"/> was deselected. False if the <see cref="HitObjectMask"/> was already deselected.</returns>
        public bool Deselect()
        {
            if (State == Visibility.Hidden)
                return false;

            Hide();
            Deselected?.Invoke(this);
            return true;
        }

        protected override void PopIn() => Alpha = 1;
        protected override void PopOut() => Alpha = 0;

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
