// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osu.Game.Overlays.News;
using osuTK;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneNewsOverlay : OsuTestScene
    {
        private TestNewsOverlay news;

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Add(news = new TestNewsOverlay());
            AddStep(@"Show", news.Show);
            AddStep(@"Hide", news.Hide);

            AddStep(@"Show front page", () => news.ShowFrontPage());
            AddStep(@"Custom article", () => news.Current.Value = "Test Article 101");

            AddStep(@"News article covers", () => news.ShowContent(new TestNewsContent()));
        }

        private class TestNewsOverlay : NewsOverlay
        {
            public void ShowContent(NewsContent content) => LoadChildContent(content);
        }

        private class TestNewsContent : NewsContent
        {
            public TestNewsContent()
            {
                Spacing = new Vector2(0, 10);
                Add(new TestCard("https://osu.ppy.sh/help/wiki/shared/news/banners/CWC_2019_banner.jpg"));
                Add(new TestCard("https://osu.ppy.sh/help/wiki/shared/news/banners/CWC_2019_banner.jpg"));
                Add(new TestCard("https://osu.ppy.sh/help/wiki/shared/news/banners/CWC_2019_banner.jpg"));
            }

            private class TestCard : Container
            {
                public TestCard(string url)
                {
                    RelativeSizeAxes = Axes.X;
                    Height = 250;
                    Masking = true;
                    CornerRadius = 4;
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = ColourInfo.GradientVertical(OsuColour.Gray(0.2f), OsuColour.Gray(0.1f))
                        },
                        new NewsArticleCover(url)
                        {
                            RelativeSizeAxes = Axes.Both,
                        }
                    };
                }
            }
        }
    }
}
