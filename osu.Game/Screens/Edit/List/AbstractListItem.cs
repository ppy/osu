// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Screens.Edit.List
{
    public abstract partial class AbstractListItem<T> : CompositeDrawable, IRearrangableDrawableListItem<T>
        where T : Drawable
    {
        public DrawableListProperties<T> Properties { get; internal set; }

        DrawableListProperties<T> IDrawableListItem<T>.Properties
        {
            get => Properties;
            set => Properties = value;
        }

        protected AbstractListItem(DrawableListRepresetedItem<T> item, DrawableListProperties<T> properties)
        {
            Model = item;
            StateChanged = t =>
            {
                switch (t)
                {
                    case SelectionState.Selected:
                        Selected.Invoke();
                        break;

                    case SelectionState.NotSelected:
                        Deselected.Invoke();
                        break;
                }
            };
            Properties = properties;
        }

        public T RepresentedItem => Model.RepresentedItem;

        public abstract void UpdateItem();
        public abstract void Select();
        public abstract void Deselect();
        public abstract void ApplyAction(Action<IDrawableListItem<T>> action);
        void IDrawableListItem<T>.SelectInternal() => SelectInternal();
        void IDrawableListItem<T>.DeselectInternal() => DeselectInternal();
        public abstract void SelectInternal(bool invokeChildMethods = true);
        public abstract void DeselectInternal(bool invokeChildMethods = true);
        public abstract SelectionState State { get; set; }

        protected void InvokeStateChanged(SelectionState state) => StateChanged.Invoke(state);

        public event Action<SelectionState> StateChanged;
        public virtual event Action Selected = () => { };
        public virtual event Action Deselected = () => { };

        #region RearrangableListItem reimplementation

        public readonly DrawableListRepresetedItem<T> Model;

        /// <summary>
        /// Invoked on drag start, if an arrangement should be started.
        /// </summary>
        internal Action<AbstractListItem<T>, DragStartEvent> StartArrangement = (_, _) => { };

        /// <summary>
        /// Invoked on drag, if this item is being arranged.
        /// </summary>
        internal Action<AbstractListItem<T>, DragEvent> Arrange = (_, _) => { };

        /// <summary>
        /// Invoked on drag end, if this item is being arranged.
        /// </summary>
        internal Action<AbstractListItem<T>, DragEndEvent> EndArrangement = (_, _) => { };

        /// <summary>
        /// Whether the item is able to be dragged at the given screen-space position.
        /// </summary>
        protected virtual bool IsDraggableAt(Vector2 screenSpacePos) => true;

        protected override bool OnDragStart(DragStartEvent e)
        {
            if (IsDraggableAt(e.ScreenSpaceMouseDownPosition))
            {
                StartArrangement.Invoke(this, e);
                return true;
            }

            return false;
        }

        protected override void OnDrag(DragEvent e) => Arrange.Invoke(this, e);

        protected override void OnDragEnd(DragEndEvent e) => EndArrangement.Invoke(this, e);

        #endregion
    }
}
