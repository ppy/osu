using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Graphics.Mf
{
    public class HashOverridenWarning : CompositeDrawable
    {
        [BackgroundDependencyLoader]
        private void load(OsuGameBase gameBase, TextureStore textures)
        {
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;

            InternalChildren = new Drawable[]
            {
                new Sprite
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Texture = textures.Get(@"Menu/dev-build-footer"),
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Spacing = new Vector2(5),
                    Margin = new MarginPadding(10),
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Text = "哈希已覆盖",
                            Font = OsuFont.GetFont(size: 30),
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                        },
                        new OsuSpriteText
                        {
                            Text = $"当前版本哈希值已覆盖为一个自定义的值, 我们无法保证mfosu可以与osu-web以及相关功能100%兼容, 请自行承担相关风险",
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                        }
                    }
                }
            };
        }
    }
}
