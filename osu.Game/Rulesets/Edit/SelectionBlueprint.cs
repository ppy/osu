﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
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
        /// <summary>
        /// Invoked when this <see cref="SelectionBlueprint"/> has been selected.
        /// </summary>
        public event Action<SelectionBlueprint> Selected;

        /// <summary>
        /// Invoked when this <see cref="SelectionBlueprint"/> has been deselected.
        /// </summary>
        public event Action<SelectionBlueprint> Deselected;

        /// <summary>
        /// The <see cref="DrawableHitObject"/> which this <see cref="SelectionBlueprint"/> applies to.
        /// </summary>
        public readonly DrawableHitObject DrawableObject;

        protected override bool ShouldBeAlive => (DrawableObject.IsAlive && DrawableObject.IsPresent) || State == SelectionState.Selected;
        public override bool HandlePositionalInput => ShouldBeAlive;
        public override bool RemoveWhenNotAlive => false;

        [Resolved(CanBeNull = true)]
        private HitObjectComposer composer { get; set; }

        protected SelectionBlueprint(DrawableHitObject drawableObject)
        {
            DrawableObject = drawableObject;

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
                if (state == value)
                    return;

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

                StateChanged?.Invoke(state);
            }
        }

        // When not selected, input is only required for the blueprint itself to receive IsHovering
        protected override bool ShouldBeConsideredForInput(Drawable child) => State == SelectionState.Selected;

        /// <summary>
        /// Selects this <see cref="SelectionBlueprint"/>, causing it to become visible.
        /// </summary>
        public void Select() => State = SelectionState.Selected;

        /// <summary>
        /// Deselects this <see cref="SelectionBlueprint"/>, causing it to become invisible.
        /// </summary>
        public void Deselect() => State = SelectionState.NotSelected;

        public bool IsSelected => State == SelectionState.Selected;

        /// <summary>
        /// Updates the <see cref="HitObject"/>, invoking <see cref="HitObject.ApplyDefaults"/> and re-processing the beatmap.
        /// </summary>
        protected void UpdateHitObject() => composer?.UpdateHitObject(DrawableObject.HitObject);

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => DrawableObject.ReceivePositionalInputAt(screenSpacePos);

        /// <summary>
        /// The <see cref="MenuItem"/>s to be displayed in the context menu for this <see cref="SelectionBlueprint"/>.
        /// </summary>
        public virtual MenuItem[] ContextMenuItems => Array.Empty<MenuItem>();

        /// <summary>
        /// The screen-space point that causes this <see cref="SelectionBlueprint"/> to be selected.
        /// </summary>
        public virtual Vector2 SelectionPoint => DrawableObject.ScreenSpaceDrawQuad.Centre;

        /// <summary>
        /// The screen-space quad that outlines this <see cref="SelectionBlueprint"/> for selections.
        /// </summary>
        public virtual Quad SelectionQuad => DrawableObject.ScreenSpaceDrawQuad;
    }
}
