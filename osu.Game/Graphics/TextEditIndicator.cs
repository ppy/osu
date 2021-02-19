using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Configuration;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Graphics
{
    public class TextEditIndicator : VisibilityContainer
    {
        private readonly OsuSpriteText spriteText;
        private readonly Box flashBox;
        private readonly BindableBool optUI = new BindableBool();

        [Resolved(canBeNull: true)]
        private OsuGame game { get; set; }

        public string Text
        {
            get => spriteText.Text;
            set
            {
                if (value == spriteText.Text)
                    return;

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
                        spriteText = new OsuSpriteText
                        {
                            UseLegacyUnicode = true,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre
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
