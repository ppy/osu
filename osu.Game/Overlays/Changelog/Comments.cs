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

            text.AddParagraph("Got feedback?", t =>
            {
                t.Colour = Color4.White;
                t.Font = OsuFont.Default.With(italics: true, size: 20);
                t.Padding = new MarginPadding { Bottom = 20 };
            });

            text.AddParagraph("We would love to hear what you think of this update! ");
            text.AddIcon(FontAwesome.Regular.GrinHearts);

            text.AddParagraph("Please visit the ");
            text.AddLink("web version", $"{build.Url}#comments");
            text.AddText(" of this changelog to leave any comments.");
        }
    }
}
