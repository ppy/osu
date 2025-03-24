// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers.Markdown;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Game.Graphics.Containers.Markdown;
using osu.Game.Overlays.Comments;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneImageProxying : OsuTestScene
    {
        [Test]
        public void TestExternalImageLink()
        {
            MarkdownContainer markdown = null!;

            // use base MarkdownContainer as a method of directly attempting to load an image without proxying logic.
            AddStep("load external without proxying", () => Child = markdown = new MarkdownContainer
            {
                RelativeSizeAxes = Axes.Both,
                Text = "![](https://github.com/ppy/osu-wiki/blob/master/wiki/Announcement_messages/img/notification.png?raw=true)",
            });
            AddWaitStep("wait", 5);
            AddAssert("image not loaded", () => markdown.ChildrenOfType<Sprite>().SingleOrDefault()?.Texture == null);

            AddStep("load external with proxying", () => Child = markdown = new OsuMarkdownContainer
            {
                RelativeSizeAxes = Axes.Both,
                Text = "![](https://github.com/ppy/osu-wiki/blob/master/wiki/Announcement_messages/img/notification.png?raw=true)",
            });
            AddUntilStep("image loaded", () => markdown.ChildrenOfType<Sprite>().SingleOrDefault()?.Texture != null);
        }

        [Test]
        public void TestExternalImageLinkInComments()
        {
            MarkdownContainer markdown = null!;

            AddStep("load external with proxying", () => Child = markdown = new CommentMarkdownContainer
            {
                RelativeSizeAxes = Axes.Both,
                Text = "![](https://github.com/ppy/osu-wiki/blob/master/wiki/Announcement_messages/img/notification.png?raw=true)",
            });
            AddUntilStep("image loaded", () => markdown.ChildrenOfType<Sprite>().SingleOrDefault()?.Texture != null);
        }
    }
}
