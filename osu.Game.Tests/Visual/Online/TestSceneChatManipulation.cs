// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public class TestSceneChatManipulation : OsuManualInputManagerTestScene
    {
        private HistoryTextBox box;
        private OsuSpriteText text;

        private int messageCounter;

        [SetUp]
        public void SetUp()
        {
            Schedule(() =>
            {
                Children = new Drawable[]
                {
                    box = new HistoryTextBox(5)
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

                messageCounter = 0;

                box.TakeFocus();
            });
        }

        [Test]
        public void TestEmptyHistory()
        {
            const string temp = "Temp message";
            AddStep("Set text", () => box.Text = temp);

            AddStep("Move down", () => InputManager.Key(Key.Down));
            AddAssert("Text is the same", () => box.Text == temp);

            AddStep("Move Up", () => InputManager.Key(Key.Up));
            AddAssert("Text is the same", () => box.Text == temp);
        }

        [Test]
        public void TestPartialHistory()
        {
            addMessages(2);

            const string temp = "Temp message";
            AddStep("Set text", () => box.Text = temp);

            AddStep("Move down", () => InputManager.Key(Key.Down));
            AddAssert("Text is the same", () => box.Text == temp);

            AddRepeatStep("Move Up", () => InputManager.Key(Key.Up), 2);
            AddAssert("Same as 1st message", () => box.Text == "Message 1");

            AddStep("Move Up", () => InputManager.Key(Key.Up));
            AddAssert("Text is the same", () => box.Text == "Message 1");

            AddRepeatStep("Move down", () => InputManager.Key(Key.Down), 2);
            AddAssert("Same as temp message", () => box.Text == temp);

            AddStep("Move down", () => InputManager.Key(Key.Down));
            AddAssert("Text is the same", () => box.Text == temp);
        }

        [Test]
        public void TestFullHistory()
        {
            addMessages(5);
            AddAssert("History saved as <5-1>", () =>
                Enumerable.Range(0, 5).Select(number => $"Message {5 - number}").SequenceEqual(box.MessageHistory));

            const string temp = "Temp message";
            AddStep("Set text", () => box.Text = temp);

            AddStep("Move down", () => InputManager.Key(Key.Down));
            AddAssert("Text is the same", () => box.Text == temp);

            addMessages(2);
            AddAssert("Overrode history to <7-3>", () =>
                Enumerable.Range(0, 5).Select(number => $"Message {7 - number}").SequenceEqual(box.MessageHistory));

            AddStep("Set text", () => box.Text = temp);

            AddRepeatStep("Move Up", () => InputManager.Key(Key.Up), 5);
            AddAssert("Same as 3rd message", () => box.Text == "Message 3");

            AddStep("Move Up", () => InputManager.Key(Key.Up));
            AddAssert("Text is the same", () => box.Text == "Message 3");

            AddRepeatStep("Move down", () => InputManager.Key(Key.Down), 4);
            AddAssert("Same as previous message", () => box.Text == "Message 7");

            AddStep("Move down", () => InputManager.Key(Key.Down));
            AddAssert("Same as temp message", () => box.Text == temp);
        }

        [Test]
        public void TestOverrideFullHistory()
        {
            addMessages(5);
            AddAssert("History saved as <5-1>", () =>
                Enumerable.Range(0, 5).Select(number => $"Message {5 - number}").SequenceEqual(box.MessageHistory));

            addMessages(6);
            AddAssert("Overrode history to <11-7>", () =>
                Enumerable.Range(0, 5).Select(number => $"Message {11 - number}").SequenceEqual(box.MessageHistory));
        }

        [Test]
        public void TestResetIndex()
        {
            addMessages(2);

            AddRepeatStep("Move Up", () => InputManager.Key(Key.Up), 2);
            AddAssert("Same as 1st message", () => box.Text == "Message 1");

            AddStep("Remove text", () => box.Text = string.Empty);
            AddStep("Move Up", () => InputManager.Key(Key.Up));
            AddAssert("Same as previous message", () => box.Text == "Message 2");

            AddStep("Move Up", () => InputManager.Key(Key.Up));
            AddStep("Select text", () => InputManager.Keys(PlatformAction.SelectAll));
            AddStep("Replace text", () => box.Text = "New text");
            AddStep("Move Up", () => InputManager.Key(Key.Up));
            AddAssert("Same as previous message", () => box.Text == "Message 2");
        }

        private void addMessages(int count)
        {
            AddRepeatStep("Add messages", () =>
            {
                box.Text = $"Message {++messageCounter}";
                InputManager.Key(Key.Enter);
            }, count);
        }
    }
}
