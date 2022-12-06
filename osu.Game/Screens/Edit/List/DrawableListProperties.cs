// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Localisation;

namespace osu.Game.Screens.Edit.List
{
    public class DrawableListProperties<T>
        where T : Drawable
    {
        private Action<T, int> setItemDepth = IDrawableListItem<T>.DEFAULT_SET_ITEM_DEPTH;

        public Action<T, int> SetItemDepth
        {
            get => setItemDepth;
            set
            {
                setItemDepth = value;
                topLevelItem.UpdateItem();
            }
        }

        private Action onDragAction = () => { };

        public Action OnDragAction
        {
            get => onDragAction;
            set
            {
                onDragAction = value;
                topLevelItem.UpdateItem();
            }
        }

        public Action<Action<IDrawableListItem<T>>> ApplyAll { get; internal set; }

        private Func<T, LocalisableString> getName = IDrawableListItem<T>.GetDefaultText;

        public Func<T, LocalisableString> GetName
        {
            get => getName;
            set
            {
                getName = value;
                topLevelItem.UpdateItem();
            }
        }

        private IDrawableListItem<T> topLevelItem;

        public IDrawableListItem<T> TopLevelItem
        {
            get => topLevelItem;
            internal set
            {
                topLevelItem = value;
                ApplyAll = topLevelItem.ApplyAction;
            }
        }

        internal DrawableListProperties(IDrawableListItem<T> topLevelItem)
        {
            ApplyAll = topLevelItem.ApplyAction;
            this.topLevelItem = topLevelItem;
        }

        internal DrawableListProperties()
        {
            ApplyAll = null!;
            topLevelItem = null!;
        }
    }
}
