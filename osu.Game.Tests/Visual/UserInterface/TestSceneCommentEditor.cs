// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Overlays.Comments;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneCommentEditor : OsuManualInputManagerTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        private TestCommentEditor commentEditor = null!;
        private TestCancellableCommentEditor cancellableCommentEditor = null!;

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
                InputManager.MoveMouseTo(commentEditor.ChildrenOfType<TextBox>().Single());
                InputManager.Click(MouseButton.Left);
            });
            AddStep("enter text", () => commentEditor.Current.Value = "text");

            AddStep("press Enter", () => InputManager.Key(Key.Enter));

            AddUntilStep("button is loading", () => commentEditor.IsSpinnerShown);
            AddAssert("text committed", () => commentEditor.CommittedText == "text");
            AddUntilStep("button is not loading", () => !commentEditor.IsSpinnerShown);
        }

        [Test]
        public void TestCommitViaKeyboardWhenEmpty()
        {
            AddStep("click on text box", () =>
            {
                InputManager.MoveMouseTo(commentEditor.ChildrenOfType<TextBox>().Single());
                InputManager.Click(MouseButton.Left);
            });

            AddStep("press Enter", () => InputManager.Key(Key.Enter));

            AddAssert("button is not loading", () => !commentEditor.IsSpinnerShown);
            AddAssert("no text committed", () => commentEditor.CommittedText.Length == 0);
        }

        [Test]
        public void TestCommitViaButton()
        {
            AddStep("click on text box", () =>
            {
                InputManager.MoveMouseTo(commentEditor.ChildrenOfType<TextBox>().Single());
                InputManager.Click(MouseButton.Left);
            });
            AddStep("enter text", () => commentEditor.Current.Value = "some other text");

            AddStep("click submit", () =>
            {
                InputManager.MoveMouseTo(commentEditor.ButtonsContainer);
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("button is loading", () => commentEditor.IsSpinnerShown);
            AddAssert("text committed", () => commentEditor.CommittedText == "some other text");
            AddUntilStep("button is not loading", () => !commentEditor.IsSpinnerShown);
        }

        [Test]
        public void TestCancelAction()
        {
            AddStep("click cancel button", () =>
            {
                InputManager.MoveMouseTo(cancellableCommentEditor.ButtonsContainer[1]);
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("cancel action fired", () => cancellableCommentEditor.Cancelled);
        }

        private partial class TestCommentEditor : CommentEditor
        {
            public new Bindable<string> Current => base.Current;
            public new FillFlowContainer ButtonsContainer => base.ButtonsContainer;

            public string CommittedText { get; private set; } = string.Empty;

            public bool IsSpinnerShown => this.ChildrenOfType<LoadingSpinner>().Single().IsPresent;

            protected override void OnCommit(string value)
            {
                ShowLoadingSpinner = true;
                CommittedText = value;
                Scheduler.AddDelayed(() => ShowLoadingSpinner = false, 1000);
            }

            protected override LocalisableString FooterText => @"Footer text. And it is pretty long. Cool.";
            protected override LocalisableString GetCommitButtonText(bool isLoggedIn) => @"Commit";
            protected override LocalisableString GetPlaceholderText(bool isLoggedIn) => @"This text box is empty";
        }

        private partial class TestCancellableCommentEditor : CancellableCommentEditor
        {
            public new FillFlowContainer ButtonsContainer => base.ButtonsContainer;

            protected override LocalisableString FooterText => @"Wow, another one. Sicc";

            public bool Cancelled { get; private set; }

            public TestCancellableCommentEditor()
            {
                OnCancel = () => Cancelled = true;
            }

            protected override void OnCommit(string text)
            {
            }

            protected override LocalisableString GetCommitButtonText(bool isLoggedIn) => @"Save";
            protected override LocalisableString GetPlaceholderText(bool isLoggedIn) => @"Multiline textboxes soon";
        }
    }
}
