using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API;
using osu.Game.Overlays.Mf.TextBox;
using osuTK;

namespace osu.Game.Overlays.Mf.Sections
{
    public class MfMenuFaqSection : MfMenuSection
    {
        public override string Title => "常见问题";
        public override string SectionId => "Faq";

        private readonly IBindable<APIState> apiState = new Bindable<APIState>();

        [Resolved]
        private IAPIProvider api { get; set; }

        private readonly OsuSpriteText faqCannotUseOnlineFunctionText = new OsuSpriteText
        {
            RelativeSizeAxes = Axes.X
        };

        [BackgroundDependencyLoader]
        private void load()
        {
            ChildDrawable = new GridContainer
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                RowDimensions = new[]
                {
                    new Dimension(GridSizeMode.AutoSize),
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        //Left
                        new FillFlowContainer
                        {
                            LayoutDuration = 300,
                            LayoutEasing = Easing.OutQuint,
                            Padding = new MarginPadding { Right = 25f },
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Spacing = new Vector2(0, 15),
                            Children = new Drawable[]
                            {
                                new MfMenuDropDownTextBoxContainer
                                {
                                    Title = "为什么加载谱面封面/音频预览的时间会那么长?",
                                    D = new OsuSpriteText
                                    {
                                        Text = "这与你的系统和当前的网络环境等一系列因素有关，"
                                               + "也可能是你一次性发送了过多的资源请求。请多等待一会，你也可以尝试重新进入谱面列表/信息界面。",
                                        RelativeSizeAxes = Axes.X,
                                    }
                                },
                                new MfMenuDropDownTextBoxContainer
                                {
                                    Title = "为什么我没法查看谱面/在线列表/排名/聊天/看板?",
                                    D = faqCannotUseOnlineFunctionText
                                }
                            }
                        },
                        //Right
                        new FillFlowContainer
                        {
                            LayoutDuration = 300,
                            LayoutEasing = Easing.OutQuint,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Padding = new MarginPadding { Left = 25f },
                            Spacing = new Vector2(0, 15),
                            Children = new Drawable[]
                            {
                                new MfMenuDropDownTextBoxContainer
                                {
                                    Title = "为什么我突然没法从Sayobot下图了?",
                                    D = new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Spacing = new Vector2(0, 2),
                                        Children = new Drawable[]
                                        {
                                            new OsuSpriteText
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                Text = "这可能是因为小夜那边出了点状况。"
                                            },
                                            new OsuSpriteText
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                Text = "尝试访问一下镜像站官网（osu.sayobot.cn）。"
                                            },
                                            new OsuSpriteText
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                Text = "如果页面空白，请通过右上角的 “更多>帮助” 进行反馈。"
                                            },
                                            new Box
                                            {
                                                Height = 19,
                                                Colour = Colour4.Black.Opacity(0)
                                            },
                                            new OsuSpriteText
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                Text = "如果官网正常，但是游戏内无法下图，你可以前往项目地址开个新的issue"
                                            }
                                        }
                                    }
                                },
                                new MfMenuDropDownTextBoxContainer
                                {
                                    Title = "为什么我没法在Mf-osu上多人游戏?",
                                    D = new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Spacing = new Vector2(0, 2),
                                        Children = new Drawable[]
                                        {
                                            new OsuSpriteText
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                Text = "这是因为osu!lazer在2020.801.0版本中加入了不允许旧版或非官方版lazer进行多人游戏的功能。"
                                            },
                                            new OsuSpriteText
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                Text = "虽然现在可以通过更改代码来实现绕过这个检测，但考虑到该行为不会受到官方认可，因此在后期绕过检测的功能被移除。"
                                            },
                                            new Box
                                            {
                                                Height = 19,
                                                Colour = Colour4.Black.Opacity(0)
                                            },
                                            new OsuSpriteText
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                Text = "另一个主要原因是我没足够的时间和精力去保证汉化版的4个游戏模式和上游完全一致。",
                                                Font = OsuFont.GetFont(size: 14)
                                            },
                                            new OsuSpriteText
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                Text = "如果哪次合并出了问题导致不公平的多人游戏环境那不是得被查水表。",
                                                Font = OsuFont.GetFont(size: 14)
                                            }
                                        }
                                    }
                                },
                            }
                        },
                    },
                }
            };
        }

        protected override void LoadComplete()
        {
            apiState.BindTo(api.State);
            apiState.BindValueChanged(onApiStateChanged, true);
            base.LoadComplete();
        }

        #region 功能函数

        private void onApiStateChanged(ValueChangedEvent<APIState> v)
        {
            switch (v.NewValue)
            {
                default:
                    faqCannotUseOnlineFunctionText.Text = "请点击右上角的\"游客\"进行登录/注册。";
                    break;

                case APIState.Failing:
                    faqCannotUseOnlineFunctionText.Text = "请检查你的网络环境。";
                    break;

                case APIState.Connecting:
                    faqCannotUseOnlineFunctionText.Text = "当前正在连接至服务器，请稍等片刻。";
                    break;

                case APIState.Online:
                    faqCannotUseOnlineFunctionText.Text = "请检查你的网络环境，也可能是ppy那边出了点状况。";
                    break;
            }
        }

        #endregion 功能函数
    }
}
