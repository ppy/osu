using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens
{
    internal class PreMigrateNotifier : CompositeDrawable
    {
        private readonly FillFlowContainer contentFlow;

        public PreMigrateNotifier(Action<bool> onConfirm, Action onCancel)
        {
            OsuSpriteText tipText;
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                tipText = new OsuSpriteText
                {
                    Font = OsuFont.GetFont(size: 25),
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Margin = new MarginPadding { Bottom = 10 }
                },
                contentFlow = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Direction = FillDirection.Vertical,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Spacing = new Vector2(10),
                    Alpha = 0,
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = "即将合并数据库，要继续吗？",
                            Font = OsuFont.Default.With(size: 40)
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = "当前版本的osu已弃用client.db，因此您需要合并数据库才能继续。",
                            Font = OsuFont.Default.With(size: 30)
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = "我们强烈建议您在合并前备份好files目录以免误删现有数据",
                            Font = OsuFont.Default.With(size: 30),
                            Colour = Color4.Gold
                        },
                        new FillFlowContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            AutoSizeAxes = Axes.Both,
                            Spacing = new Vector2(10),
                            Children = new Drawable[]
                            {
                                new BackgroundButton
                                {
                                    Text = "清空现有数据并继续",
                                    Action = () =>
                                    {
                                        this.FadeOut(300, Easing.OutQuint).Finally(_ =>
                                        {
                                            onConfirm?.Invoke(true);
                                            Expire();
                                        });
                                    },
                                    ButtonTooltip = "这将清空现有的谱面以及分数, 但能保证合并正常完成。",
                                    ButtonTooltipSpriteText = tipText
                                },
                                new BackgroundButton
                                {
                                    Text = "保留原有数据并继续",
                                    Action = () =>
                                    {
                                        this.FadeOut(300, Easing.OutQuint).Finally(_ =>
                                        {
                                            onConfirm?.Invoke(false);
                                            Expire();
                                        });
                                    },
                                    ButtonTooltip = "这将保留现有的谱面以及分数，但合并可能会失败。",
                                    ButtonTooltipSpriteText = tipText
                                },
                                new BackgroundButton
                                {
                                    Text = "退出",
                                    Action = () =>
                                    {
                                        this.FadeOut(300, Easing.OutQuint).Finally(_ =>
                                        {
                                            onCancel?.Invoke();
                                            Expire();
                                        });
                                    },
                                    ButtonTooltip = "退出游戏",
                                    ButtonTooltipSpriteText = tipText
                                }
                            }
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            contentFlow.FadeIn(300, Easing.OutQuint);
            base.LoadComplete();
        }

        private class BackgroundButton : OsuAnimatedButton
        {
            public LocalisableString Text
            {
                get => buttonText.Text;
                set => buttonText.Text = value;
            }

            public OsuSpriteText ButtonTooltipSpriteText { get; set; }

            public LocalisableString ButtonTooltip { get; set; }

            private readonly OsuSpriteText buttonText = new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Font = OsuFont.GetFont(size: 20)
            };

            public BackgroundButton()
            {
                Height = 60;
                Width = 180;

                base.Content.Add(buttonText);
            }

            protected override bool OnHover(HoverEvent e)
            {
                if (ButtonTooltipSpriteText != null)
                    ButtonTooltipSpriteText.Text = ButtonTooltip;

                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                if (ButtonTooltipSpriteText != null && ButtonTooltipSpriteText.Text == ButtonTooltip)
                    ButtonTooltipSpriteText.Text = string.Empty;

                base.OnHoverLost(e);
            }
        }
    }
}
