// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Game.Overlays.News;
using osu.Framework.Graphics;
using osu.Game.Overlays;
using osu.Framework.Allocation;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneNewsHeader : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        private TestHeader header;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Child = header = new TestHeader
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            };
        });

        [Test]
        public void TestControl()
        {
            AddAssert("Front page selected", () => header.Current.Value == NewsHeader.FrontPageString);
            AddAssert("1 tab total", () => header.TabCount == 1);

            AddStep("Set article 1", () => header.SetArticle("1"));
            AddAssert("Article 1 selected", () => header.Current.Value == "1");
            AddAssert("2 tabs total", () => header.TabCount == 2);

            AddStep("Set article 2", () => header.SetArticle("2"));
            AddAssert("Article 2 selected", () => header.Current.Value == "2");
            AddAssert("2 tabs total", () => header.TabCount == 2);

            AddStep("Set front page", () => header.SetFrontPage());
            AddAssert("Front page selected", () => header.Current.Value == NewsHeader.FrontPageString);
            AddAssert("1 tab total", () => header.TabCount == 1);
        }

        private partial class TestHeader : NewsHeader
        {
            public int TabCount => TabControl.Items.Count;
        }
    }
}
