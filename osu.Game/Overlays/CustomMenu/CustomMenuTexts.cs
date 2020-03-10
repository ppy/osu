// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;
using osu.Game.Overlays.Profile.Header.Components;
using System;
using osu.Game.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Users;

namespace osu.Game.Overlays.CustomMenu
{
    public class CustomMenuTexts : CustomMenuContent
    {
        private LinkFlowContainer textFlow;

        private OverlinedInfoContainer timeBar;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            InternalChildren = new Drawable[]
            {
                new FillFlowContainer{
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Padding = new MarginPadding(20),
                    Spacing = new Vector2(0, 5),
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Font = OsuFont.GetFont(size: 24),
                            Text = "时间",
                        },
                        timeBar = new OverlinedInfoContainer
                        {
                            Title = "当前时间",
                            LineColour = colourProvider.Highlight1,
                        },
                        new OsuSpriteText{ Text = " ", },
                        textFlow = new LinkFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            TextAnchor = Anchor.TopLeft,
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Spacing = new Vector2(0, 2),
                        },
                    }
                }
            };
            static void Titlefont(SpriteText t) => t.Font = OsuFont.GetFont(size: 30, weight: FontWeight.SemiBold);

            textFlow.AddParagraph("关于Mf-osu", Titlefont );
            textFlow.NewParagraph();
            textFlow.AddLink("Mf-osu","https://github.com/MATRIX-feather/osu");
            textFlow.AddText("是一个基于");
            textFlow.AddLink("官方osu!lazer","https://github.com/ppy/osu");
            textFlow.AddText("的分支版本。");

            textFlow.NewParagraph();
            textFlow.AddParagraph("为该分支贡献过自己一份力量的人(按首字母排序)", Titlefont );
            textFlow.NewParagraph();

            textFlow.AddUserLink(new User
                        {
                            Username = "A M D (比赛端翻译修正)",
                            Id = 5321112
                        });
            textFlow.NewParagraph();
            textFlow.AddUserLink(new User
                        {
                            Username = "MATRIX-feather (主要翻译，项目发起和维护)",
                            Id = 13870362
                        });
            textFlow.NewParagraph();
            textFlow.AddUserLink(new User
                        {
                            Username = "poly000 (游戏内翻译修正)",
                            Id = 13851970
                        });
            textFlow.NewParagraph();
            textFlow.AddLink("以及所有为官方lazer作出过贡献的人<3","https://github.com/ppy/osu/graphs/contributors");

            textFlow.NewParagraph();
            textFlow.AddParagraph("注意事项", Titlefont );
            textFlow.NewParagraph();

            textFlow.AddText("虽然osu!lazer和他的框架osu!framework是基于");
            textFlow.AddLink("MIT协议","https://opensource.org/licenses/MIT");
            textFlow.AddText("开源的, 这允许你只要在软件/源代码的任何副本中包含原始版权和许可声明，你可以做任何事情。");

            textFlow.AddParagraph("但这并不覆盖有关\"osu\"和\"ppy\"的任何用法，因为这些都是注册商标并受商标法的保护，");
            textFlow.AddText("请不要公开分发包含这些内容的项目，如果有疑惑请发送邮件至");
            textFlow.AddLink("contact@ppy.sh","mailto:contact@ppy.sh");

            textFlow.AddParagraph("如果汉化版侵犯到了您的相关权益，请发送邮件至");
            textFlow.AddLink("midnightcarnival@outlook.com","mailto:midnightcarnival@outlook.com");
            textFlow.AddText("以商讨相关问题");

        }

        private DateTime GetTimeInfo()
        {
            var dt = DateTime.Now;
            return dt;
        }
        protected override void Update()
        {
            base.Update();

            timeBar.Content = GetTimeInfo().ToString() ?? "未知";

        }
    }
}