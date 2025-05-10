// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public partial class TestSceneHistoryTextBox : OsuManualInputManagerTestScene
    {
        private const string temp = "Temp message";

        private int messageCounter;

        private HistoryTextBox box = null!;
        private OsuSpriteText text = null!;

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

                box.OnCommit += (_, _) =>
                {
                    if (string.IsNullOrEmpty(box.Text))
                        return;

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
            AddStep("Set text", () => box.Text = temp);

            AddStep("Move down", () => InputManager.Key(Key.Down));
            AddAssert("Text is unchanged", () => box.Text == temp);

            AddStep("Move up", () => InputManager.Key(Key.Up));
            AddAssert("Text is unchanged", () => box.Text == temp);
        }

        [Test]
        public void TestPartialHistory()
        {
            addMessages(3);
            AddStep("Set text", () => box.Text = temp);

            AddStep("Move down", () => InputManager.Key(Key.Down));
            AddAssert("Text is unchanged", () => box.Text == temp);

            AddRepeatStep("Move up", () => InputManager.Key(Key.Up), 3);
            AddAssert("Same as 1st message", () => box.Text == "Message 1");

            AddStep("Move up", () => InputManager.Key(Key.Up));
            AddAssert("Same as 1st message", () => box.Text == "Message 1");

            AddStep("Move down", () => InputManager.Key(Key.Down));
            AddAssert("Same as 2nd message", () => box.Text == "Message 2");

            AddRepeatStep("Move down", () => InputManager.Key(Key.Down), 2);
            AddAssert("Temporary message restored", () => box.Text == temp);

            AddStep("Move down", () => InputManager.Key(Key.Down));
            AddAssert("Text is unchanged", () => box.Text == temp);
        }

        [Test]
        public void TestFullHistory()
        {
            addMessages(7);
            AddStep("Set text", () => box.Text = temp);

            AddStep("Move down", () => InputManager.Key(Key.Down));
            AddAssert("Text is unchanged", () => box.Text == temp);

            AddRepeatStep("Move up", () => InputManager.Key(Key.Up), 5);
            AddAssert("Same as 3rd message", () => box.Text == "Message 3");

            AddStep("Move up", () => InputManager.Key(Key.Up));
            AddAssert("Same as 3rd message", () => box.Text == "Message 3");

            AddRepeatStep("Move down", () => InputManager.Key(Key.Down), 4);
            AddAssert("Same as 7th message", () => box.Text == "Message 7");

            AddStep("Move down", () => InputManager.Key(Key.Down));
            AddAssert("Temporary message restored", () => box.Text == temp);

            AddStep("Move down", () => InputManager.Key(Key.Down));
            AddAssert("Text is unchanged", () => box.Text == temp);
        }

        [Test]
        public void TestChangedHistory()
        {
            addMessages(2);
            AddStep("Set text", () => box.Text = temp);
            AddStep("Move up", () => InputManager.Key(Key.Up));

            AddStep("Change text", () => box.Text = "New message");
            AddStep("Move down", () => InputManager.Key(Key.Down));
            AddStep("Move up", () => InputManager.Key(Key.Up));
            AddAssert("Changes lost", () => box.Text == "Message 2");
        }

        [Test]
        public void TestInputOnEdge()
        {
            addMessages(2);
            AddStep("Set text", () => box.Text = temp);

            AddStep("Move down", () => InputManager.Key(Key.Down));
            AddAssert("Text unchanged", () => box.Text == temp);

            AddRepeatStep("Move up", () => InputManager.Key(Key.Up), 2);
            AddAssert("Same as 1st message", () => box.Text == "Message 1");

            AddStep("Move up", () => InputManager.Key(Key.Up));
            AddAssert("Text unchanged", () => box.Text == "Message 1");
        }

        [Test]
        public void TestResetIndex()
        {
            addMessages(2);

            AddRepeatStep("Move Up", () => InputManager.Key(Key.Up), 2);
            AddAssert("Same as 1st message", () => box.Text == "Message 1");

            AddStep("Change text", () => box.Text = "New message");
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
