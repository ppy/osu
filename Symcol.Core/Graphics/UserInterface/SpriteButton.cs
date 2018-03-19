using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input;
using Symcol.Core.Graphics.Containers;

namespace Symcol.Core.Graphics.UserInterface
{
    /// <summary>
    /// just a Button with a sprite
    /// </summary>
    public class SpriteButton : SymcolClickableContainer
    {
        private readonly string textureName;

        public string Text
        {
            get { return spriteText?.Text; }
            set
            {
                if (spriteText != null)
                    spriteText.Text = value;
            }
        }

        private readonly Sprite sprite;
        private readonly SpriteText spriteText;

        public SpriteButton(string textureName)
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            this.textureName = textureName;
            Masking = true;

            Children = new Drawable[]
            {
                sprite = new Sprite
                {
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fill
                },
                spriteText = new SpriteText
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            sprite.Texture = textures.Get(textureName);
        }

        protected override bool OnClick(InputState state)
        {
            if (Enabled.Value)
            {
                var flash = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.5f
                };

                Add(flash);

                flash.Blending = BlendingMode.Additive;
                flash.FadeOut(200);
                flash.Expire();
            }

            return base.OnClick(state);
        }
    }
}
