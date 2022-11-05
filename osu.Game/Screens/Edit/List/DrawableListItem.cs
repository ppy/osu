// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Edit.List
{
    public class DrawableListItem : CompositeDrawable, IDrawableListItem
    {
        private readonly OsuSpriteText text = new OsuSpriteText();
        private readonly Box box;
        protected readonly WeakReference<Drawable> DrawableReference;

        internal DrawableListItem(Drawable d, LocalisableString name)
        {
            DrawableReference = new WeakReference<Drawable>(d);
            text.Text = name;

            InternalChildren = new Drawable[]
            {
                box = new Box
                {
                    Colour = new Colour4(255, 255, 0, 0.25f),
                },
                text
            };
            box.Hide();
        }

        public DrawableListItem(Drawable d)
            : this(d, (d.GetType().DeclaringType ?? d.GetType()).Name)
        {
        }

        public Drawable GetDrawableListItem()
        {
            return this;
        }

        public void Select(bool value)
        {
            if (value)
            {
                box.Show();
                box.Width = text.Width;
                box.Height = text.Height;
            }
            else
            {
                box.Hide();
                box.Width = text.Width;
                box.Height = text.Height;
            }
        }

        protected bool Equals(DrawableListItem other)
        {
            //todo: is this correct?
            return text.Text.Equals(other.text.Text) && GetHashCode() == other.GetHashCode();
        }

        public bool Equals(IDrawableListItem other)
        {
            if (other is DrawableListItem item) return Equals(item);

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(text, box, DrawableReference);
        }
    }
}
