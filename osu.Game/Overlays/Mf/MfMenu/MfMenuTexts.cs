// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osuTK;
using osu.Game.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Users;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.MfMenu
{
    public class MfMenuTexts : MfMenuContent
    {
        private static Vector2 FillFlowSpacing = new Vector2(0, 15);
        private static void Titlefont(SpriteText t) => t.Font = OsuFont.GetFont(size: 30, weight: FontWeight.SemiBold);
        private static void QuestionTitlefont(SpriteText t) => t.Font = OsuFont.GetFont(size: 22, weight: FontWeight.SemiBold);
        private static void AnswerTitlefont(SpriteText t) => t.Font = OsuFont.GetFont(weight: FontWeight.SemiBold);

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Spacing = new Vector2(0, 5),
                Margin = new MarginPadding{ Top = 20, Bottom = 50 },
                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Text = "介绍",
                        Font = OsuFont.GetFont(size: 30),
                    },
                    new GridContainer
                    {
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
                                    Spacing = FillFlowSpacing,
                                    Children = new Drawable[]
                                    {
                                            new MfMenuTextBoxContainer
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                                d = introduceTextBox()
                                            },
                                            new MfMenuTextBoxContainer
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                                d = reportIssuesTextBox()
                                            },
                                            new MfMenuTextBoxContainer
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                                d = projectRefsTextBox(),
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
                                    Spacing = FillFlowSpacing,
                                    Children = new Drawable[]
                                    {
                                        new MfMenuTextBoxContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            d = staffTextBox()
                                        },
                                        new MfMenuTextBoxContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            d = warningsTextBox()
                                        },
                                        new MfMenuTextBoxContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            d = specialThanksTextBox()
                                        }
                                    }
                                },
                            },
                        }
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Text = "F&Q",
                        Font = OsuFont.GetFont(size: 30),
                    },
                    new GridContainer
                    {
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
                                    Spacing = FillFlowSpacing,
                                    Children = new Drawable[]
                                    {
                                            new MfMenuTextBoxContainer
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                                d = faqLongCoverLoad()
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
                                    Spacing = FillFlowSpacing,
                                    Children = new Drawable[]
                                    {
                                        new MfMenuTextBoxContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            d = faqSayobotFail()
                                        }
                                    }
                                },
                            },
                        }
                    },
                }
            };
        }

        protected Drawable introduceTextBox()
        {
            var t = new MfTextBox();
            
            t.NewParagraph();
            t.AddParagraph("关于Mf-osu", Titlefont );
            t.NewParagraph();
            t.NewParagraph();
            t.AddLink("Mf-osu","https://github.com/MATRIX-feather/osu");
            t.AddText("是一个基于");
            t.AddLink("官方osu!lazer","https://github.com/ppy/osu");
            t.AddText("的分支版本。");

            return t;
        }

        protected Drawable staffTextBox()
        {
            var t = new MfTextBox();

            t.NewParagraph();
            t.AddParagraph("参与过完善该分支的人(按首", Titlefont );
            t.AddText("字母排序)", Titlefont );
            t.NewParagraph();
            t.NewParagraph();

            t.AddUserLink(new User
                        {
                            Username = "A M D (比赛端、游戏内翻译修正)",
                            Id = 5321112
                        });
            t.NewParagraph();
            t.AddUserLink(new User
                        {
                            Username = "MATRIX-feather (主要翻译, 项目发起和维护)",
                            Id = 13870362
                        });
            t.NewParagraph();
            t.AddUserLink(new User
                        {
                            Username = "pedajilao (游戏内翻译修正)",
                            Id = 13851970
                        });
            t.NewParagraph();

            return t;
            
        }

        protected Drawable reportIssuesTextBox()
        {
            var t = new MfTextBox();

            t.NewParagraph();
            t.AddParagraph("Bug反馈/提出建议", Titlefont);
            t.NewParagraph();
            t.NewParagraph();

            t.AddText("任何与翻译文本、字体大小等有关的问题, 请前往");
            t.AddLink("Mf-osu的issue页面","https://github.com/MATRIX-feather/osu/issues");
            t.AddText("提交新的issue来讨论。");
            t.AddParagraph("如果你在游玩时发现了一个游戏功能bug, 请先");
            t.AddLink("下载最新官方版本","https://github.com/ppy/osu/releases/latest");
            t.AddText(", 如果该问题仍然存在, 则请前往");
            t.AddLink("osu!lazer的issue页面","https://github.com/ppy/osu/issues");
            t.AddText("提交新的issue, 反之则前往");
            t.AddLink("Mf-osu的issue页面","https://github.com/MATRIX-feather/osu/issues");
            t.AddText("来提交新的issue。");

            return t;
        }

        protected Drawable warningsTextBox()
        {
            var t = new MfTextBox();

            t.NewParagraph();
            t.AddParagraph("注意事项", Titlefont );
            t.NewParagraph();
            t.NewParagraph();

            t.AddText("虽然osu!lazer和他的框架osu!framework");
            t.AddLink("是基于MIT协议","https://opensource.org/licenses/MIT");
            t.AddText("开源的, 但这并不覆盖有关\"osu\"和\"ppy\"在软件、 资源、 广告和促销中的的任何用法, 因为这些都是注册商标并受商标法的保护, ");

            t.AddText("详细信息可以通过");
            t.AddLink("官方README","https://github.com/ppy/osu#licence");
            t.AddText("查询。");
            t.AddParagraph("如果仍有疑惑, 您可以发送邮件至");
            t.AddLink("contact@ppy.sh","mailto:contact@ppy.sh");
            t.AddParagraph("与汉化版有关的问题，请发送邮件至");
            t.AddLink("midnightcarnival@outlook.com","mailto:midnightcarnival@outlook.com");

            return t;
        }

        protected Drawable projectRefsTextBox()
        {
            var t = new MfTextBox();

            t.NewParagraph();
            t.AddParagraph("项目引用", Titlefont );
            t.NewParagraph();
            t.NewParagraph();

            t.AddText("Mf-osu项目在跟进和维护的同时也会尝试");
            t.AddText("添加一些新奇的功能。");
            t.AddParagraph("大部分功能会保持其原有的实现方式。");
            t.AddParagraph("如果你觉得下面的某个功能很赞，您可以前往");
            t.AddParagraph("该项目的主页点个Star以支持原作者, 或者帮助其完善和发展。");
            t.NewParagraph();
            t.AddLink("osu!下的Mirror Mod → pr7334[Open]","https://github.com/ppy/osu/pull/7334");
            t.NewParagraph();
            t.AddLink("osu!tau模式(因为兼容性问题不在此版本中) → Altenhh/tau (9c77fab)","https://github.com/Altenhh/tau");
            t.NewParagraph();
            t.AddLink("谱面在线列表 → pr7912[Merged]","https://github.com/ppy/osu/pull/7912");
            t.NewParagraph();
            t.AddLink("看板 → pr8771[Merged]","https://github.com/ppy/osu/pull/8771");
            t.NewParagraph();
            t.AddLink("从osu/rulesets目录读取自定义游戏模式 → pr8607[Merged]","https://github.com/ppy/osu/pull/8607");
            t.NewParagraph();
            t.AddLink("Mvis播放器 → 基于EVAST9919/lazer-m-vis","https://github.com/EVAST9919/lazer-m-vis");
            t.AddParagraph("暂时不知道tau模式是否可以使用在线功能");

            return t;
        }

        protected Drawable specialThanksTextBox()
        {
            var t = new MfTextBox();

            t.NewParagraph();
            t.AddParagraph("Special Thanks", Titlefont );
            t.NewParagraph();
            t.NewParagraph();

            t.AddUserLink(new User
            {
                Username = "A M D",
                Id = 5321112
            });
            t.NewParagraph();
            t.AddLink("小夜", "https://osu.sayobot.cn/");
            t.NewParagraph();

            return t;
        }

        protected Drawable faqLongCoverLoad()
        {
            var t = new MfTextBox();

            t.AddParagraph("Q: 为什么加载谱面封面/音频预览的时间", QuestionTitlefont);
            t.AddText("会那么长?", QuestionTitlefont);
            t.NewParagraph();
            t.AddParagraph("A: 这与你的系统和当前的网络环境等一系列因素", AnswerTitlefont);
            t.AddText("有关, 也可能是你一次性发送了过多的资源请求, 请多等待一会, 你也可以尝试重新进入谱面列表/信息界面", AnswerTitlefont);

            return t;
        }

        protected Drawable faqSayobotFail()
        {
            var t = new MfTextBox();

            t.AddParagraph("Q: 为什么我突然没法下图了?", QuestionTitlefont);
            t.NewParagraph();
            t.AddParagraph("A: 这可能是因为小夜那边出了点状况, 尝试访问一下", AnswerTitlefont);
            t.AddLink("镜像站官网","https://osu.sayobot.cn/", AnswerTitlefont);
            t.AddText(", 如果页面空白, 请通过右上角的 更多>帮助 进行反馈； 如果官网正常, 而游戏内无法下图, 请联系", AnswerTitlefont);
            t.AddLink("MATRIX-feather", "mailto:midnightcarnival@outlook.com" ,AnswerTitlefont);
            t.AddText("并附上日志文件", AnswerTitlefont);
            t.AddParagraph("你也可以通过临时关闭 Mf-osu>启用Sayobot功能 来使用官方源", AnswerTitlefont);
            return t;
        }
    }
}
