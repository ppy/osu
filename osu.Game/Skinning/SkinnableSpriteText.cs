// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Skinning
{
    public class SkinnableSpriteText : SkinnableDrawable<SpriteText>, IHasText
    {
        public SkinnableSpriteText(string name, Func<string, SpriteText> defaultImplementation, Func<ISkinSource, bool> allowFallback = null, bool restrictSize = true)
            : base(name, defaultImplementation, allowFallback, restrictSize)
        {
        }

        protected override void SkinChanged(ISkinSource skin, bool allowFallback)
        {
            base.SkinChanged(skin, allowFallback);

            if (Drawable is IHasText textDrawable)
                textDrawable.Text = Text;
        }

        private string text;

        public string Text
        {
            get => text;
            set
            {
                if (text == value)
                    return;

                text = value;

                if (Drawable is IHasText textDrawable)
                    textDrawable.Text = value;
            }
        }
    }
}
