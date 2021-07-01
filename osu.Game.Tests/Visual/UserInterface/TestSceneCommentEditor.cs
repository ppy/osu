// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays;
using osu.Game.Overlays.Comments;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneCommentEditor : OsuManualInputManagerTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        private TestCommentEditor commentEditor;
        private TestCancellableCommentEditor cancellableCommentEditor;

        [SetUp]
        public void SetUp() => Schedule(() =>
            Add(new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Y,
                Width = 800,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 20),
                Children = new Drawable[]
                {
                    commentEditor = new TestCommentEditor(),
                    cancellableCommentEditor = new TestCancellableCommentEditor()
                }
            }));

        [Test]
        public void TestCommitViaKeyboard()
        {
            AddStep("click on text box", () =>
            {
                InputManager.MoveMouseTo(commentEditor);
                InputManager.Click(MouseButton.Left);
            });
            AddStep("enter text", () => commentEditor.Current.Value = "text");

            AddStep("press Enter", () => InputManager.Key(Key.Enter));

            AddAssert("text committed", () => commentEditor.CommittedText == "text");
            AddAssert("button is loading", () => commentEditor.IsLoading);
        }

        [Test]
        public void TestCommitViaKeyboardWhenEmpty()
        {
            AddStep("click on text box", () =>
            {
                InputManager.MoveMouseTo(commentEditor);
                InputManager.Click(MouseButton.Left);
            });

            AddStep("press Enter", () => InputManager.Key(Key.Enter));

            AddAssert("no text committed", () => commentEditor.CommittedText == null);
            AddAssert("button is not loading", () => !commentEditor.IsLoading);
        }

        [Test]
        public void TestCommitViaButton()
        {
            AddStep("click on text box", () =>
            {
                InputManager.MoveMouseTo(commentEditor);
                InputManager.Click(MouseButton.Left);
            });
            AddStep("enter text", () => commentEditor.Current.Value = "some other text");

            AddStep("click submit", () =>
            {
                InputManager.MoveMouseTo(commentEditor.ButtonsContainer);
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("text committed", () => commentEditor.CommittedText == "some other text");
            AddAssert("button is loading", () => commentEditor.IsLoading);
        }

        [Test]
        public void TestCancelAction()
        {
            AddStep("click cancel button", () =>
            {
                InputManager.MoveMouseTo(cancellableCommentEditor.ButtonsContainer);
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("cancel action fired", () => cancellableCommentEditor.Cancelled);
        }

        private class TestCommentEditor : CommentEditor
        {
            public new Bindable<string> Current => base.Current;
            public new FillFlowContainer ButtonsContainer => base.ButtonsContainer;

            public string CommittedText { get; private set; }

            public TestCommentEditor()
            {
                OnCommit = onCommit;
            }

            private void onCommit(string value)
            {
                CommittedText = value;
                Scheduler.AddDelayed(() => IsLoading = false, 1000);
            }

            protected override string FooterText => @"Footer text. And it is pretty long. Cool.";
            protected override string CommitButtonText => @"Commit";
            protected override string TextBoxPlaceholder => @"This text box is empty";
        }

        private class TestCancellableCommentEditor : CancellableCommentEditor
        {
            public new FillFlowContainer ButtonsContainer => base.ButtonsContainer;
            protected override string FooterText => @"Wow, another one. Sicc";

            public bool Cancelled { get; private set; }

            public TestCancellableCommentEditor()
            {
                OnCancel = () => Cancelled = true;
            }

            protected override string CommitButtonText => @"Save";
            protected override string TextBoxPlaceholder => @"Multiline textboxes soon";
        }
    }
}
