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
    public partial class TestSceneMediaProxying : OsuTestScene
    {
        [Resolved]
        private GameHost host { get; set; } = null!;

        [Test]
        public void TestExternalImageLink()
        {
            AddStep("load image", () => setup(new OsuMarkdownContainer
            {
                RelativeSizeAxes = Axes.Both,
                Text = "![](https://github.com/ppy/osu-wiki/blob/master/wiki/Announcement_messages/img/notification.png?raw=true)",
            }));
        }

        [Test]
        public void TestLocalImageLink()
        {
            AddStep("load image", () => setup(new OsuMarkdownContainer
            {
                RelativeSizeAxes = Axes.Both,
                Text = "![](https://osu.ppy.sh/help/wiki/shared/news/banners/monthly-beatmapping-contest.png)",
            }));
        }

        [Test]
        public void TestInvalidImageLink()
        {
            AddStep("load image", () => setup(new OsuMarkdownContainer
            {
                RelativeSizeAxes = Axes.Both,
                Text = "![](https://this-site-does-not-exist.com/img.png)",
            }));
        }

        private void setup(Drawable drawable)
        {
            var onlineStore = new OsuOnlineStore(@"https://osu.ppy.sh");
            var textureStore = new TextureStore(host.Renderer, host.CreateTextureLoaderStore(onlineStore));

            Child = new DependencyProvidingContainer
            {
                RelativeSizeAxes = Axes.Both,
                CachedDependencies = new (Type, object)[] { (typeof(TextureStore), textureStore) },
                Child = drawable,
            };
        }
    }
}
