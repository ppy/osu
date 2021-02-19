using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Mvis.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Graphics
{
    public class TextEditIndicator : VisibilityContainer
    {
        private readonly OsuSpriteText spriteText;
        private readonly Box flashBox;
        private readonly BindableBool optUI = new BindableBool();
        private readonly FillFlowContainer placeHolderContainer;

        [Resolved(canBeNull: true)]
        private OsuGame game { get; set; }

        public string Text
        {
            get => spriteText.Text;
            set
            {
                if (value == spriteText.Text)
                    return;

                if (string.IsNullOrEmpty(value))
                    Schedule(() =>
                    {
                        placeHolderContainer.FadeIn(150);
                        spriteText.FadeOut(150);
                    });
                else
                    Schedule(() =>
                    {
                        placeHolderContainer.FadeOut(150);
                        spriteText.FadeIn(150);
                    });

                spriteText.Text = value;
            }
        }

        public TextEditIndicator()
        {
            AutoSizeAxes = Axes.Both;
            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;
            Masking = true;
            CornerRadius = 5f;
            EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Radius = 7,
                Colour = Color4.Black.Opacity(0.1f)
            };

            Children = new Drawable[]
            {
                flashBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black.Opacity(0.5f),
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Blending = BlendingParameters.Mixture
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    AutoSizeDuration = 300,
                    AutoSizeEasing = Easing.OutQuint,
                    Margin = new MarginPadding(10),
                    Spacing = new Vector2(3),
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Children = new Drawable[]
                            {
                                new PlaceHolder
                                {
                                    Height = 28
                                },
                                placeHolderContainer = new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Spacing = new Vector2(5),
                                    Margin = new MarginPadding { Vertical = 5 },
                                    Colour = Color4.White.Opacity(0.6f),
                                    Children = new Drawable[]
                                    {
                                        new SpriteIcon
                                        {
                                            Size = new Vector2(14),
                                            Icon = FontAwesome.Solid.Pen,
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            Margin = new MarginPadding { Vertical = 2 }
                                        },
                                        new OsuSpriteText
                                        {
                                            Text = "暂无输入",
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft
                                        }
                                    }
                                },
                                spriteText = new OsuSpriteText
                                {
                                    UseLegacyUnicode = true,
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Margin = new MarginPadding { Vertical = 5 },
                                },
                            }
                        },
                        new Circle
                        {
                            Height = 3,
                            RelativeSizeAxes = Axes.X,
                            Width = 0.8f,
                            Colour = Color4.White,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre
                        },
                        new OsuSpriteText
                        {
                            Text = "若输入法完成编辑后没有出现文字，请尝试多按几次空格",
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(MConfigManager config)
        {
            config.BindWith(MSetting.OptUI, optUI);

            optUI.BindValueChanged(v =>
            {
                if (!v.NewValue) Hide();
                else if (!string.IsNullOrEmpty(Text)) Show();
            });
        }

        protected override void UpdateAfterChildren()
        {
            Margin = new MarginPadding { Top = (game?.ToolbarOffset ?? 0) + 5 };
            base.UpdateAfterChildren();
        }

        public override void Show()
        {
            if (!optUI.Value) return;

            base.Show();
        }

        protected override void PopIn()
        {
            this.FadeIn(300, Easing.OutSine)
                .MoveToY(0, 300, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            this.FadeOut(300, Easing.OutQuint)
                .MoveToY(-23, 300, Easing.OutQuint);
        }

        public void Flash() =>
            flashBox.FlashColour(Color4.Gold, 1000, Easing.OutQuint);
    }
}
