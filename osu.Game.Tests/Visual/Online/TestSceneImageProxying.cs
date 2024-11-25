// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Framework.Platform;
using osu.Game.Graphics.Containers.Markdown;
using osu.Game.Online;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneImageProxying : OsuTestScene
    {
        [Resolved]
        private GameHost host { get; set; } = null!;

        [Test]
        public void TestExternalImageLink()
        {
            AddStep("load image", () => Child = new OsuMarkdownContainer
            {
                RelativeSizeAxes = Axes.Both,
                Text = "![](https://github.com/ppy/osu-wiki/blob/master/wiki/Announcement_messages/img/notification.png?raw=true)",
            });
        }

        [Test]
        public void TestLocalImageLink()
        {
            AddStep("load image", () => Child = new OsuMarkdownContainer
            {
                RelativeSizeAxes = Axes.Both,
                Text = "![](https://osu.ppy.sh/help/wiki/shared/news/banners/monthly-beatmapping-contest.png)",
            });
        }

        [Test]
        public void TestInvalidImageLink()
        {
            AddStep("load image", () => Child = new OsuMarkdownContainer
            {
                RelativeSizeAxes = Axes.Both,
                Text = "![](https://this-site-does-not-exist.com/img.png)",
            });
        }
    }
}
