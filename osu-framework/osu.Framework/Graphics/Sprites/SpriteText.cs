//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System.Collections.Generic;
using OpenTK;
using osu.Framework.Graphics.Textures;
using osu.Framework.Cached;
using osu.Framework.Graphics.Containers;
using osu.Framework.Resources;

namespace osu.Framework.Graphics.Sprites
{
    public class SpriteText : FlowContainer
    {
        /// <summary>
        /// The amount by which characters should overlap each other (negative character spacing).
        /// </summary>
        public float SpacingOverlap;

        public override bool IsVisible => base.IsVisible && !string.IsNullOrEmpty(text);

        public string Font { get; private set; }

        private Cached<Vector2> internalSize = new Cached<Vector2>();

        private float spaceWidth;

        private static TextureStore centralStore;

        private TextureStore store;

        public SpriteText(TextureStore store = null)
        {
            this.store = store;
            Font = @"Exo2.0-Regular";
        }

        public override void Load()
        {
            if (store == null)
            {
                if (centralStore == null)
                    centralStore = new TextureStore(new GlyphStore(Game.Resources.Get(@"Fonts.Exo2.0-Regular.otf"), @"Exo2.0-Regular")) { ScaleAdjust = 0.16f };

                store = centralStore;
            }


            base.Load();

            // Cache all sprites that may be used in the future.
            for (char i = '0'; i <= '9'; i++)
                getTexture(i);
            getTexture('.');
            getTexture(',');
            getTexture('%');

            spaceWidth = getSprite('.')?.Width ?? 20;
        }

        private string text;
        public virtual string Text
        {
            get { return text; }
            set
            {
                if (text == value)
                    return;

                text = value;
                internalSize.Invalidate();
            }
        }

        private float? constantWidth;
        public bool TextFixedWidth
        {
            get { return constantWidth.HasValue; }
            set
            {
                if (value)
                    constantWidth = getSprite('5')?.Width + 1 ?? 20;
                else
                    constantWidth = null;
            }
        }

        public override Vector2 Size
        {
            get
            {
                if (constantWidth.HasValue && !HasDefinedSize)
                    // We can determine the size even in the case autosize hasn't been run here, because we override autosize
                    refreshLayout();
                return base.Size;
            }
        }

        protected override void Update()
        {
            base.Update();
            refreshLayout();
        }

        private void refreshLayout()
        {
            internalSize.Refresh(delegate
            {
                Clear();

                if (string.IsNullOrEmpty(text))
                    return Vector2.Zero;

                foreach (char c in text)
                {
                    Drawable s;
                    if (c == ' ')
                        s = new Drawable() { Size = new Vector2(spaceWidth) };
                    else
                        s = getSprite(c);

                    Add(s);
                }

                return Vector2.Zero;
            });
        }

        private Texture getTexture(char c) => store?.Get(getTextureName(c));
        private Sprite getSprite(char c) => new Sprite(getTexture(c));

        private string getTextureName(char c) => $@"{Font}-{getCharName(c)}";
        private bool isSpecialChar(char c) => getCharName(c) != c.ToString();

        private string getCharName(char c)
        {
            switch (c)
            {
                case ' ':
                    return null;
                case ',':
                    return @"comma";
                case '.':
                    return @"dot";
                case '%':
                    return @"percent";
                case '/':
                    return @"slash";
                case '\\':
                    return @"fps";
                case '=':
                    return @"ms";
                case '+':
                    return @"hz";
                default:
                    return c.ToString();
            }
        }
    }
}
