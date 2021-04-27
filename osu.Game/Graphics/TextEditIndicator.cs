using System.Text;
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
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Mvis.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Graphics
{
    public class TextEditIndicator : VisibilityContainer
    {
        private readonly OsuSpriteText spriteText;
        private readonly ProgressBar bg;
        private readonly Box flashBox;
        private IBindable<bool> optUI;
        private IBindable<bool> alwaysHide;
        private readonly FillFlowContainer placeHolderContainer;
        private readonly Circle bar;

        [Resolved(canBeNull: true)]
        private OsuGame game { get; set; }

        private string text;

        public string Text
        {
            get => text;
            set
            {
                if (value == text)
                    return;

                text = value;

                //猜测: 因为文本输入在input上，因此不使用Schedule()会导致osu.Framework.Graphics.Drawable+InvalidThreadForMutationException

                if (string.IsNullOrEmpty(value))
                {
                    if (State.Value == Visibility.Visible)
                        Schedule(executeTimeoutHide);

                    Schedule(() =>
                    {
                        placeHolderContainer.FadeIn(150);
                        spriteText.FadeOut(150);
                    });
                }
                else
                {
                    Schedule(() =>
                    {
                        abortTimeoutHide(true);

                        placeHolderContainer.FadeOut(150);
                        spriteText.FadeIn(150);
                    });
                }

                Schedule(() =>
                {
                    spriteText.Text = value;
                    bg.CurrentTime = Encoding.Default.GetBytes(Text).Length;
                });
            }
        }

        private void executeTimeoutHide() =>
            bar.ResizeWidthTo(0, 3000).OnComplete(_ => Hide());

        private void abortTimeoutHide(bool animate) =>
            bar.ResizeWidthTo(0.8f, animate ? 300 : 0, Easing.OutQuint);

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
                bg = new ByteLengthIndicator(false)
                {
                    RelativeSizeAxes = Axes.Both,
                    BackgroundColour = Color4.Black.Opacity(0.5f),
                    FillColour = Color4.Aqua.Opacity(0.5f),
                    EndTime = 31,
                    CurrentTime = 0
                },
                flashBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Gold.Opacity(0f),
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
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Margin = new MarginPadding { Vertical = 5 },
                                },
                            }
                        },
                        bar = new Circle
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
            optUI = config.GetBindable<bool>(MSetting.OptUI);
            alwaysHide = config.GetBindable<bool>(MSetting.AlwaysHideTextIndicator);

            optUI.BindValueChanged(v =>
            {
                if (!v.NewValue) Hide();
                else if (!string.IsNullOrEmpty(Text)) Show();
            });

            alwaysHide.BindValueChanged(v =>
            {
                if (v.NewValue) Hide();
            });
        }

        protected override void UpdateAfterChildren()
        {
            Margin = new MarginPadding { Top = (game?.ToolbarOffset ?? 0) + 5 };
            base.UpdateAfterChildren();
        }

        public override void Show()
        {
            if (!optUI.Value || alwaysHide.Value) return;

            base.Show();
        }

        protected override void PopIn()
        {
            var emptyText = string.IsNullOrEmpty(Text);
            abortTimeoutHide(!emptyText);

            //在某些系统下窗口会莫名进入编辑状态，此时因为没有文本，所以不要显示
            if (emptyText) return;

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

        private class ByteLengthIndicator : ProgressBar
        {
            protected override void UpdateValue(float value)
            {
                fill.ResizeWidthTo(value, 300, Easing.OutQuint);
                fill.FadeColour(CurrentNumber.Value >= CurrentNumber.MaxValue
                    ? Color4.Red.Opacity(0.5f)
                    : Color4.Aqua.Opacity(0.5f), 300);
            }

            public ByteLengthIndicator(bool allowSeek)
                : base(allowSeek)
            {
                fill.RelativeSizeAxes = Axes.Both;
            }
        }
    }
}
