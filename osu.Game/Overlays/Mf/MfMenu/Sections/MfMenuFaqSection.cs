using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API;
using osuTK;

namespace osu.Game.Overlays.MfMenu
{
    public class MfMenuFaqSection : Container, IOnlineComponent
    {
        private IAPIProvider API;

        private OsuSpriteText faqCannotUseOnlineFunctionText = new OsuSpriteText();

        public static void AnswerTitleFont(SpriteText t) => t.Font = OsuFont.GetFont(weight: FontWeight.SemiBold);

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api)
        {
            this.API = api;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;
            InternalChild = new GridContainer
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
                            LayoutDuration = 500,
                            LayoutEasing = Easing.OutQuint,
                            Padding = new MarginPadding{ Right = 25f },
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
                                    D = faqLongCoverLoad()
                                },
                                new MfMenuDropDownTextBoxContainer
                                {
                                    Title = "为什么我没法查看谱面/在线列表/排名/聊天/看板?",
                                    D = faqCannotUseOnlineFunction()
                                }
                            }
                        },
                        //Right
                        new FillFlowContainer
                        {
                            LayoutDuration = 500,
                            LayoutEasing = Easing.OutQuint,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Padding = new MarginPadding{ Left = 25f },
                            Spacing = new Vector2(0, 15),
                            Children = new Drawable[]
                            {
                                new MfMenuDropDownTextBoxContainer
                                {
                                    Title = "为什么我突然没法从Sayobot下图了?",
                                    D = faqSayobotFail()
                                },
                            }
                        },
                    },
                }
            };
        }

        protected override void LoadComplete()
        {
            API.Register(this);
            base.LoadComplete();
        }

        #region faq

        protected Drawable faqLongCoverLoad()
        {
            var t = new MfText();

            t.AddParagraph("这与你的系统和当前的网络环境等", AnswerTitleFont);
            t.AddText("一系列因素有关, 也可能是你一次性发送了过多的资源请求, 请多等待一会, 你也可以尝试重新进入谱面列表/信息界面", AnswerTitleFont);

            return t;
        }

        protected Drawable faqSayobotFail()
        {
            var t = new MfText();

            t.AddParagraph("这可能是因为小夜那边出了点状况, 尝试访问一下", AnswerTitleFont);
            t.AddLink("镜像站官网","https://osu.sayobot.cn/", AnswerTitleFont);
            t.AddText(", 如果页面空白, 请通过右上角的 更多>帮助 进行反馈； 如果官网正常, 而游戏内无法下图, 请联系", AnswerTitleFont);
            t.AddLink("MATRIX-feather", "mailto:midnightcarnival@outlook.com", AnswerTitleFont);
            t.AddText("并附上日志文件", AnswerTitleFont);
            t.AddParagraph("你也可以通过临时关闭 Mf-osu>启用Sayobot功能 来使用官方源", AnswerTitleFont);
            return t;
        }

        protected Drawable faqCannotUseOnlineFunction()
        {
            var c = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Children = new Drawable[]
                {
                    faqCannotUseOnlineFunctionText
                },
            };

            return c;
        }

        #endregion faq

        #region 功能函数
        public void APIStateChanged(IAPIProvider api, APIState state)
        {
            switch (state)
            {
                default:
                    faqCannotUseOnlineFunctionText.Text = "请点击右上角的\"游客\"进行登录";
                    break;

                case APIState.Failing:
                    faqCannotUseOnlineFunctionText.Text = "请检查你的网络环境";
                    break;

                case APIState.Connecting:
                    faqCannotUseOnlineFunctionText.Text = "当前正在连接至服务器, 请稍等片刻";
                    break;

                case APIState.Online:
                    faqCannotUseOnlineFunctionText.Text = "请检查你的网络环境, 也可能是ppy那边出了点状况";
                    break;
            }
        }

        #endregion 功能函数
    }
}