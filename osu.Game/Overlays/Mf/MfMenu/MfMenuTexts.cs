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
using osu.Framework.Graphics.Shapes;
using osuTK.Graphics;

namespace osu.Game.Overlays.MfMenu
{
    public class MfMenuTexts : MfMenuContent
    {
        private static void Titlefont(SpriteText t) => t.Font = OsuFont.GetFont(size: 30, weight: FontWeight.SemiBold);
        private static void QuestionTitlefont(SpriteText t) => t.Font = OsuFont.GetFont(size: 22, weight: FontWeight.SemiBold);
        private static void AnswerTitlefont(SpriteText t) => t.Font = OsuFont.GetFont(weight: FontWeight.SemiBold);
        private LinkFlowContainer textFlow;
        private LinkFlowContainer faqFlow;

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
                Padding = new MarginPadding(20),
                Spacing = new Vector2(0, 5),
                Children = new Drawable[]
                {
                    textFlow = new LinkFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        TextAnchor = Anchor.TopLeft,
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.TopLeft,
                        Spacing = new Vector2(0, 2),
                    },
                    new Box
                    {
                        Colour = new Color4(0, 0, 0, 255),
                        Alpha = 0.3f,
                        RelativeSizeAxes = Axes.X,
                        Height = 2,
                    },
                    faqFlow = new LinkFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        TextAnchor = Anchor.TopLeft,
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.TopLeft,
                        Spacing = new Vector2(0, 2),
                    },
                }
            };
            AddTextFlow();
            AddFaqFlow();
        }

        private void AddTextFlow()
        {
            textFlow.AddParagraph("关于Mf-osu", Titlefont );
            textFlow.NewParagraph();
            textFlow.AddLink("Mf-osu","https://github.com/MATRIX-feather/osu");
            textFlow.AddText("是一个基于");
            textFlow.AddLink("官方osu!lazer","https://github.com/ppy/osu");
            textFlow.AddText("的分支版本。");

            textFlow.NewParagraph();
            textFlow.AddParagraph("参与过完善该分支的人(按首字母排序)", Titlefont );
            textFlow.NewParagraph();

            textFlow.AddUserLink(new User
                        {
                            Username = "A M D (比赛端、游戏内翻译修正)",
                            Id = 5321112
                        });
            textFlow.NewParagraph();
            textFlow.AddUserLink(new User
                        {
                            Username = "MATRIX-feather (主要翻译, 项目发起和维护)",
                            Id = 13870362
                        });
            textFlow.NewParagraph();
            textFlow.AddUserLink(new User
                        {
                            Username = "pedajilao (游戏内翻译修正)",
                            Id = 13851970
                        });
            textFlow.NewParagraph();

            textFlow.NewParagraph();
            textFlow.AddParagraph("Bug反馈/提出建议", Titlefont);
            textFlow.NewParagraph();

            textFlow.AddText("任何与翻译文本、字体大小等有关的问题, 请前往");
            textFlow.AddLink("Mf-osu的issue页面","https://github.com/MATRIX-feather/osu/issues");
            textFlow.AddText("提交新的issue来讨论。");
            textFlow.AddParagraph("如果你在游玩时发现了一个游戏功能bug, 请先");
            textFlow.AddLink("下载最新官方版本","https://github.com/ppy/osu/releases/latest");
            textFlow.AddText(", 如果该问题仍然存在, 则请前往");
            textFlow.AddLink("osu!lazer的issue页面","https://github.com/ppy/osu/issues");
            textFlow.AddText("提交新的issue, 反之则前往");
            textFlow.AddLink("Mf-osu的issue页面","https://github.com/MATRIX-feather/osu/issues");
            textFlow.AddText("来提交新的issue。");

            textFlow.NewParagraph();
            textFlow.AddParagraph("注意事项", Titlefont );
            textFlow.NewParagraph();

            textFlow.AddText("虽然osu!lazer和他的框架osu!framework是基于");
            textFlow.AddLink("MIT协议","https://opensource.org/licenses/MIT");
            textFlow.AddText("开源的, 但这并不覆盖有关\"osu\"和\"ppy\"在软件、 资源、 广告和促销中的的任何用法, 因为这些都是注册商标并受商标法的保护, ");

            textFlow.AddText("详细信息可以通过");
            textFlow.AddLink("官方README","https://github.com/ppy/osu#licence");
            textFlow.AddText("查询。");
            textFlow.AddParagraph("如果仍有疑惑, 您可以发送邮件至");
            textFlow.AddLink("contact@ppy.sh","mailto:contact@ppy.sh");
            textFlow.AddParagraph("与汉化版有关的问题，请发送邮件至");
            textFlow.AddLink("midnightcarnival@outlook.com","mailto:midnightcarnival@outlook.com");

            textFlow.NewParagraph();
            textFlow.AddParagraph("项目引用", Titlefont );
            textFlow.NewParagraph();

            textFlow.AddText("Mf-osu项目在跟进和维护的同时也会尝试添加一些新奇的功能。");
            textFlow.AddParagraph("大部分功能会保持其原有的实现方式。");
            textFlow.AddParagraph("如果你觉得下面的某个功能很赞，您可以前往该项目页面点个Star以支持原作者, 或者帮助其完善和发展。");
            textFlow.NewParagraph();
            textFlow.AddLink("osu!下的Mirror Mod → pr7334[Open]","https://github.com/ppy/osu/pull/7334");
            textFlow.NewParagraph();
            textFlow.AddLink("osu!tau模式(因为兼容性问题不在此版本中) → Altenhh/tau (9c77fab)","https://github.com/Altenhh/tau");
            textFlow.NewParagraph();
            textFlow.AddLink("谱面在线列表 → pr7912[Merged]","https://github.com/ppy/osu/pull/7912");
            textFlow.NewParagraph();
            textFlow.AddLink("看板 → pr8771[Merged]","https://github.com/ppy/osu/pull/8771");
            textFlow.NewParagraph();
            textFlow.AddLink("从osu/rulesets目录读取自定义游戏模式 → pr8607[Merged]","https://github.com/ppy/osu/pull/8607");
            textFlow.NewParagraph();
            textFlow.AddLink("Mvis播放器 → 基于EVAST9919/lazer-m-vis","https://github.com/EVAST9919/lazer-m-vis");
            textFlow.AddParagraph("暂时不知道tau模式是否可以使用在线功能");
        }

        private void AddFaqFlow()
        {
            faqFlow.AddParagraph("一些常见问题", Titlefont );
            faqFlow.AddParagraph("Q: 为什么加载谱面封面/音频预览的时间会那么长?", QuestionTitlefont);
            faqFlow.AddParagraph("A: 这与你的系统和当前的网络环境等一系列因素有关, 也可能是你一次性发送了过多的资源请求, 请多等待一会, 你也可以尝试重新进入谱面列表/信息界面", AnswerTitlefont);
            faqFlow.NewParagraph();
            faqFlow.AddParagraph("Q: 为什么我突然没法下图了?", QuestionTitlefont);
            faqFlow.AddParagraph("A: 这可能是因为小夜那边出了点状况, 尝试访问一下", AnswerTitlefont);
            faqFlow.AddLink("镜像站官网","https://osu.sayobot.cn/", AnswerTitlefont);
            faqFlow.AddText(", 如果页面空白, 请通过右上角的 更多>帮助 进行反馈； 如果官网正常, 而游戏内无法下图, 请联系", AnswerTitlefont);
            faqFlow.AddLink("MATRIX-feather", "mailto:midnightcarnival@outlook.com" ,AnswerTitlefont);
            faqFlow.AddText("并附上日志文件", AnswerTitlefont);
            faqFlow.NewParagraph();
            faqFlow.AddParagraph("未完待续...");
        }
    }
}
