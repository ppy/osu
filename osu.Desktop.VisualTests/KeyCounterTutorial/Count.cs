using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Desktop.KeyCounterTutorial
{
    internal abstract class Count : Container
    {
        private int value;
        public int FadeDuration => 200;
        public bool IsCounting { get; set; }
        public bool IsLit { get; set; }
        public string Name { get; set; }

        public int Value
        {
            get { return value; }
            set
            {
                this.value = value;
                ValueSprite.Text = value.ToString();
            }
        }

        public Texture KeyUpTexture { get; set; }
        public Texture KeyDownTexture { get; set; }

        public Sprite KeySprite { get; set; }
        public Sprite KeyGlowSprite { get; set; }

        public AutoSizeContainer TextContainer { get; set; }
        public SpriteText NameSprite { get; set; }
        public SpriteText ValueSprite { get; set; }

        public Color4 KeyUpTextColor => Color4.White;
        public Color4 KeyDownTextColor => Color4.Black;

        public Count(string name)
        {
            Name = name;
        }

        public override void Load()
        {
            base.Load();

            KeyUpTexture = Game.Textures.Get(@"KeyCounter/key-up");
            KeyDownTexture = Game.Textures.Get(@"KeyCounter/key-hit");

            KeySprite = new Sprite
            {
                Texture = KeyUpTexture,
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre
            };
            KeyGlowSprite = new Sprite
            {
                Texture = Game.Textures.Get(@"KeyCounter/key-glow"),
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Alpha = 0
            };

            TextContainer = new AutoSizeContainer()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                SizeMode = InheritMode.XY,
                Colour = Color4.White,
                Children = new Drawable[]
                {
                    new SpriteText
                        {
                            Text = Name,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            PositionMode = InheritMode.XY,
                            Position = new Vector2(0, -0.25f),
                        },
                        ValueSprite = new SpriteText
                        {
                            Text = Value.ToString(),
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            PositionMode = InheritMode.XY,
                            Position = new Vector2(0, 0.25f),
                        }
               }
            };

            Children= new Drawable[]
            {
                KeySprite,
                KeyGlowSprite,
                TextContainer
            };

            Height = KeySprite.Height;
            Width = KeySprite.Width;
        }

        protected void UpdateVisualState(bool isLit)
        {
            if (isLit)
            {
                KeySprite.Texture = KeyDownTexture;
                KeyGlowSprite.FadeIn(FadeDuration);
                TextContainer.Colour = KeyDownTextColor;
            }
            else
            {
                KeySprite.Texture = KeyUpTexture;
                KeyGlowSprite.FadeOut(FadeDuration);
                TextContainer.Colour = KeyUpTextColor;
            }
        }

        public void Reset() => Value = 0;
    }
}