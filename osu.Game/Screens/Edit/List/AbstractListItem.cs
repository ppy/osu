// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Edit.List
{
    public abstract partial class AbstractListItem<T> : RearrangeableListItem<DrawableListRepresetedItem<T>>, IRearrangableDrawableListItem<T>
        where T : Drawable
    {
        private Action<T, int> setItemDepth = IDrawableListItem<T>.DEFAULT_SET_ITEM_DEPTH;
        private Action onDragAction = () => { };
        private Func<T, LocalisableString> getName = IDrawableListItem<T>.GetDefaultText;

        protected AbstractListItem(DrawableListRepresetedItem<T> item)
            : base(item)
        {
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
        }

        public T? RepresentedItem => Model.RepresentedItem;
        public abstract Action<Action<IDrawableListItem<T>>> ApplyAll { get; set; }

        public Func<T, LocalisableString> GetName
        {
            get => getName;
            set
            {
                getName = value;
                OnSetGetName(ref value);
            }
        }

        protected virtual void OnSetGetName(ref Func<T, LocalisableString> value) { }

        public Action<T, int> SetItemDepth
        {
            get => setItemDepth;
            set
            {
                setItemDepth = value;
                OnSetItemDepth(ref value);
            }
        }

        protected virtual void OnSetItemDepth(ref Action<T, int> value) { }

        public Action OnDragAction
        {
            get => onDragAction;
            set
            {
                onDragAction = value;
                OnSetDragAction(ref value);
            }
        }

        protected virtual void OnSetDragAction(ref Action value) { }

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
    }
}
