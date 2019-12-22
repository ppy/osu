// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osuTK.Graphics;

namespace osu.Game.Overlays.Changelog
{
    public class Comments : CompositeDrawable
    {
        private readonly APIChangelogBuild build;

        public Comments(APIChangelogBuild build)
        {
            this.build = build;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Padding = new MarginPadding
            {
                Horizontal = 50,
                Vertical = 20,
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            LinkFlowContainer text;

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 10,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colours.GreyVioletDarker
                    },
                },
                text = new LinkFlowContainer(t =>
                {
                    t.Colour = colours.PinkLighter;
                    t.Font = OsuFont.Default.With(size: 14);
                })
                {
                    Padding = new MarginPadding(20),
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                }
            };

            text.AddParagraph("反馈问题?", t =>
            {
                t.Colour = Color4.White;
                t.Font = OsuFont.Default.With(italics: true, size: 24);
                t.Padding = new MarginPadding { Bottom = 20 };
            });

            text.AddParagraph("我们想知道你如何看待这次更新! ",t =>
            {
                t.Font = OsuFont.Default.With(size: 20);
            });
            text.AddIcon(FontAwesome.Regular.GrinHearts);

            text.AddParagraph("请访问",t =>
            {
                t.Font = OsuFont.Default.With(size: 20);
            });
            text.AddLink("网页版", $"{build.Url}#comments",t =>
            {
                t.Font = OsuFont.Default.With(size: 20);
            });
            text.AddText("的更改日志来留言.",t =>
            {
                t.Font = OsuFont.Default.With(size: 20);
            });

            text.AddParagraph("另外,你还可以通过访问",t =>
            {
                t.Font = OsuFont.Default.With(size: 17);
            });
            text.AddLink("这个链接","https://github.com/ppy/osu/graphs/contributors",t =>
            {
                t.Font = OsuFont.Default.With(size: 17);
            });
            text.AddParagraph("和",t =>
            {
                t.Font = OsuFont.Default.With(size: 17);
            });
            text.AddLink("这个链接","https://github.com/matrix-feather/osu/graphs/contributors",t =>
            {
                t.Font = OsuFont.Default.With(size: 17);
            });
            text.AddText("来查看迄今为止所有参与过osu!lazer以及中文版osu!lazer开发的人员!感谢他们的辛勤付出!",t =>
            {
                t.Font = OsuFont.Default.With(size: 17);
            });
        }
    }
}
