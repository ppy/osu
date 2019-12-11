// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
        }

        private class TestNewsOverlay : NewsOverlay
        {
            public new void LoadAndShowChild(NewsContent content) => base.LoadAndShowChild(content);
        }
    }
}
