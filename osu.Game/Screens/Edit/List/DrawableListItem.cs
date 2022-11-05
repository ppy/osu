// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Edit.List
{
    public class DrawableListItem : CompositeDrawable, IDrawableListItem
    {
        private readonly OsuSpriteText text = new OsuSpriteText();
        protected readonly WeakReference<Drawable> DrawableReference;

        internal DrawableListItem(Drawable d, LocalisableString name)
        {
            DrawableReference = new WeakReference<Drawable>(d);
            text.Text = name;

            InternalChild = text;
        }

        public DrawableListItem(Drawable d)
            : this(d, (d.GetType().DeclaringType ?? d.GetType()).Name)
        {
        }

        public Drawable GetDrawableListItem()
        {
            return this;
        }
    }
}
