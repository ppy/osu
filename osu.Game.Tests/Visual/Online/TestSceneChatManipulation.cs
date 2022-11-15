// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays.Chat;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public class TestSceneChatManipulation : OsuManualInputManagerTestScene
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

                box.TakeFocus();
            });
        }

        [Test]
        public void TestStayOnLastIndex()
        {
            addMessages(2);
            AddRepeatStep("Move to last", () => InputManager.Key(Key.Up), 2);

            string lastText = string.Empty;
            AddStep("Move up", () =>
            {
                lastText = box.Text;
                InputManager.Key(Key.Up);
            });

            AddAssert("Text hasn't changed", () => lastText == box.Text);
        }

        [Test]
        public void TestKeepOriginalMessage()
        {
            addMessages(1);
            AddStep("Start writing", () => box.Text = "A random 文, ...");

            AddStep("Move up", () => InputManager.Key(Key.Up));
            AddStep("Rewrite old message", () => box.Text = "Old Message");

            AddStep("Move back down", () => InputManager.Key(Key.Down));
            AddAssert("Text back to previous", () => box.Text == "A random 文, ...");
        }

        [Test]
        public void TestResetIndexOnEmpty()
        {
            addMessages(2);
            AddRepeatStep("Move up", () => InputManager.Key(Key.Up), 2);
            AddStep("Remove text", () => box.Text = string.Empty);

            AddStep("Move up again", () => InputManager.Key(Key.Up));
            AddAssert("Back to first message", () => box.Text == "Message 2");
        }

        [Test]
        public void TestReachingLimitOfMessages()
        {
            addMessages(100);
            AddAssert("List is full of <100-1>", () =>
                Enumerable.Range(0, 100).Select(number => $"Message {100 - number}").SequenceEqual(box.MessageHistory));

            addMessages(2);
            AddAssert("List is full of <102-3>", () =>
                Enumerable.Range(0, 100).Select(number => $"Message {102 - number}").SequenceEqual(box.MessageHistory));
        }

        private void addMessages(int count)
        {
            int iterations = 0;
            AddRepeatStep("Add messages", () =>
            {
                box.Text = $"Message {++iterations}";
                InputManager.Key(Key.Enter);
            }, count);
        }
    }
}
