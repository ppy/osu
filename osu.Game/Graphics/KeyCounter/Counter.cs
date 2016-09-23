using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Graphics.KeyCounter
{
    /// <summary>
    /// A drawable counter which counts upon a false -> true isLit change.
    /// Additionally isCounting can be set to false to disable the counter.
    /// </summary>  
    internal class Count : Drawable
    {
        private int countingValue;

        private Texture keyGlow;
        private Texture keyHit;
        private Texture keyUp;
        private Sprite sCounter, sGlow;
        private SpriteText sTextCount;
        private SpriteText sTextKey;

        protected string name;

        internal bool isCounting;

        private bool isLit = false;
        internal bool IsLit
        {
            get { return isLit; }

            set
            {
                if (!isCounting) return;

                if (value)
                {
                    sCounter.Texture = keyHit;
                    sTextKey.Colour = new Color4(77, 160, 186, 255);
                    sTextCount.Colour = new Color4(77, 160, 186, 255);

                    sGlow.FadeIn(50);
                    isLit = true;

                    countingValue++;
                    sTextCount.Text = countingValue.ToString();
                }
                else
                {
                    sCounter.Texture = keyUp;
                    sTextKey.Colour = Color4.White;
                    sTextCount.Colour = Color4.White;

                    sGlow.FadeOut(150);
                    isLit = false;
                }
            }
        }

        internal void Reset()
        {
            countingValue = 0;
            sTextCount.Text = countingValue.ToString();
        }

        public override bool Contains(Vector2 screenSpacePos) => true;

        public override void Load()
        {
            base.Load();

            Size = new Vector2(30, 110);

            isCounting = true;

            keyGlow = Game.Textures.Get(@"key-glow");
            keyHit = Game.Textures.Get(@"key-hit");
            keyUp = Game.Textures.Get(@"key-up");

            sGlow = new Sprite
            {
                Texture = keyGlow,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Alpha = 0.0f
            };

            sCounter = new Sprite
            {
                Texture = keyUp,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };

            sTextKey = new SpriteText
            {
                Text = name,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Position = new Vector2(0, 20)
            };

            sTextCount = new SpriteText//(string.Empty, @"scoreentry", 2.5f, Clocks.Game, new Vector2(0, 10), 0.0f, Color4.Transparent)
            {
                Text = string.Empty,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Position = new Vector2(0, -10)
            };

            Reset();

            Add(sGlow);
            Add(sCounter);
            Add(sTextCount);
            Add(sTextKey);
        }
    }
}