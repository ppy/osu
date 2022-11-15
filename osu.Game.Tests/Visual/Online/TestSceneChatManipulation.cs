// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays.Chat;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public class TestSceneChatManipulation : OsuTestScene
    {
        private ChatTextBox box;
        private OsuSpriteText text;

        [SetUp]
        public void SetUp()
        {
            Schedule(() =>
            {
                Children = new Drawable[]
                {
                    box = new ChatTextBox
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.X,
                        Width = 0.99f,
                    },
                    text = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.X,
                        Width = 0.99f,
                        Y = -box.Height,
                        Font = OsuFont.Default.With(size: 20),
                    }
                };
                box.OnCommit += (_, __) =>
                {
                    text.Text = $"{nameof(box.OnCommit)}: {box.Text}";
                    box.Text = string.Empty;
                    box.TakeFocus();
                    text.FadeOutFromOne(1000, Easing.InQuint);
                };
            });
        }

        [Test]
        public void TestReachingLimitOfMessages()
        {
        }

        [Test]
        public void TestStayOnLastIndex()
        {
        }

        [Test]
        public void TestKeepOriginalMessage()
        {
        }

        [Test]
        public void TestResetIndexOnEmpty()
        {
        }
    }
}
