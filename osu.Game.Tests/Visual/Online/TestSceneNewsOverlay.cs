// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Game.Overlays;
using osu.Game.Overlays.News;

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

            AddStep(@"Article covers", () => news.LoadAndShowContent(new NewsCoverTest()));
        }

        private class TestNewsOverlay : NewsOverlay
        {
            public new void LoadAndShowContent(NewsContent content) => base.LoadAndShowContent(content);
        }

        private class NewsCoverTest : NewsContent
        {
            public NewsCoverTest()
            {
                Spacing = new osuTK.Vector2(0, 10);

                var article = new NewsArticleCover.ArticleInfo
                {
                    Author = "Ephemeral",
                    CoverUrl = "https://assets.ppy.sh/artists/58/header.jpg",
                    Time = new DateTime(2019, 12, 4),
                    Title = "New Featured Artist: Kurokotei"
                };

                Children = new Drawable[]
                {
                    new NewsArticleCover(article)
                    {
                        Height = 200
                    },
                    new NewsArticleCover(article)
                    {
                        Height = 120
                    },
                    new NewsArticleCover(article)
                    {
                        RelativeSizeAxes = Axes.None,
                        Size = new osuTK.Vector2(400, 200),
                    }
                };
            }
        }
    }
}
