using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays.MfMenu
{
    public class MfMenuIntroduceSection : MfMenuSection
    {
        public override string Title => "关于";
        public override string SectionId => "Introduce";

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
                            Padding = new MarginPadding{ Right = 25f },
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Spacing = new Vector2(0, 15),
                            Children = new Drawable[]
                            {
                                    new MfMenuTextBoxContainer
                                    {
                                        Title = "关于Mf-osu",
                                        d = introduceText()
                                    },
                                    new MfMenuTextBoxContainer
                                    {
                                        Title = "Bug反馈/提出建议",
                                        d = reportIssuesText()
                                    },
                                    new MfMenuTextBoxContainer
                                    {
                                        Title = "项目引用",
                                        d = projectRefsText(),
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
                            Padding = new MarginPadding{ Left = 25f },
                            Spacing = new Vector2(0, 15),
                            Children = new Drawable[]
                            {
                                new MfMenuTextBoxContainer
                                {
                                    Title = "参与过完善该分支的人(按首字母排序)",
                                    d = staffText()
                                },
                                new MfMenuTextBoxContainer
                                {
                                    Title = "注意事项",
                                    d = attentionsText()
                                },
                                new MfMenuTextBoxContainer
                                {
                                    Title = "Special Thanks",
                                    d = specialThanksText()
                                }
                            }
                        },
                    },
                }
            };
        }

        #region 介绍

        protected Drawable introduceText()
        {
            var t = new MfText();

            t.AddLink("Mf-osu","https://github.com/MATRIX-feather/osu");
            t.AddText("是一个基于");
            t.AddLink("官方osu!lazer","https://github.com/ppy/osu");
            t.AddText("的分支版本。");

            return t;
        }

        protected Drawable staffText()
        {
            var t = new MfText();

            t.AddUserLink(new User
                        {
                            Username = "A M D (比赛端、游戏内翻译修正)",
                            Id = 5321112
                        });
            t.NewParagraph();
            t.AddUserLink(new User
                        {
                            Username = "MATRIX-feather (主要翻译, 项目发起和维护等)",
                            Id = 13870362
                        });
            t.NewParagraph();
            t.AddUserLink(new User
                        {
                            Username = "pedajilao (游戏内翻译修正)",
                            Id = 13851970
                        });

            return t;
        }

        protected Drawable reportIssuesText()
        {
            var t = new MfText();

            t.AddText("任何与翻译文本、字体大小等有关的问题, 请前往");
            t.AddLink("Mf-osu的issue页面","https://github.com/MATRIX-feather/osu/issues");
            t.AddText("提交新的issue来讨论。");
            t.AddParagraph("如果你在使用时发现了一个bug, 请先");
            t.AddLink("下载最新官方版本","https://github.com/ppy/osu/releases/latest");
            t.AddText(", 如果该问题仍然存在, 请前往");
            t.AddLink("osu!lazer的issue页面","https://github.com/ppy/osu/issues");
            t.AddText("提交新的issue, 反之则前往");
            t.AddLink("Mf-osu的issue页面","https://github.com/MATRIX-feather/osu/issues");
            t.AddText("来提交新的issue。");

            return t;
        }

        protected Drawable attentionsText()
        {
            var t = new MfText();

            t.AddText("虽然osu!lazer和他的框架osu!");
            t.AddText("framework");
            t.AddLink("是基于MIT协议","https://opensource.org/licenses/MIT");
            t.AddText("开源的, 但这并不覆盖有关\"osu\"和\"ppy\"在软件、 资源、 广告和促销中的的任何用法, 因为这些都是注册商标并受商标法的保护, ");

            t.AddText("详细信息可以通过");
            t.AddLink("官方README","https://github.com/ppy/osu#licence");
            t.AddText("查询。");
            t.AddParagraph("如果仍有疑惑, 您可以发送邮件至");
            t.AddLink("contact@ppy.sh","mailto:contact@ppy.sh");
            t.AddText(";");
            t.AddParagraph("与本项目有关的问题, 请发送邮件至");
            t.AddLink("midnightcarnival@outlook.com","mailto:midnightcarnival@outlook.com");
            t.AddText(", 一般情况下, 本人将会在一周内给予回应");

            t.AddParagraph("与本项目二进制发行版有关的问题, 请联系您的二进制发行方。");

            return t;
        }

        protected Drawable projectRefsText()
        {
            var t = new MfText();

            t.AddText("Mf-osu项目在跟进和维护的同时也会尝试");
            t.AddText("添加一些新奇的功能。");
            t.AddParagraph("大部分功能会保持其原有的实现方式。");
            t.AddParagraph("如果你觉得下面的某个功能很赞, 您可以前往");
            t.AddParagraph("该项目的主页点个Star以支持原作者, 或者帮助其完善和发展。");
            t.NewParagraph();
            t.AddLink("osu!下物件的高潮闪光(Kiai Flash) → pr7316[Open]","https://github.com/ppy/osu/pull/7316");
            t.NewParagraph();
            t.AddLink("osu!下的Mirror Mod → pr7334[Open]","https://github.com/ppy/osu/pull/7334");
            t.NewParagraph();
            t.AddLink("osu!tau模式 → Altenhh/tau (ca7bd14)","https://github.com/Altenhh/tau");
            t.NewParagraph();
            t.AddLink("谱面在线列表 → pr7912[Merged]","https://github.com/ppy/osu/pull/7912");
            t.NewParagraph();
            t.AddLink("看板 → pr8771[Merged]","https://github.com/ppy/osu/pull/8771");
            t.NewParagraph();
            t.AddLink("从osu/rulesets目录读取自定义游戏模式 → pr8607[Merged]","https://github.com/ppy/osu/pull/8607");
            t.NewParagraph();
            t.AddLink("Mvis播放器 → 基于EVAST9919/lazer-m-vis","https://github.com/EVAST9919/lazer-m-vis");

            return t;
        }

        protected Drawable specialThanksText()
        {
            var t = new MfText();

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

        #endregion 介绍

    }
}