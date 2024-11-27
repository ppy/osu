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
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
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
        private DummyAPIAccess dummyAPI => (DummyAPIAccess)API;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create content", () => Child = new FillFlowContainer
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
            });
        }

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
        public void TestLoggingInAndOut()
        {
            void assertLoggedInState()
            {
                AddAssert("commit button visible", () => commentEditor.ButtonsContainer[0].Alpha == 1);
                AddAssert("login button hidden", () => commentEditor.ButtonsContainer[1].Alpha == 0);
                AddAssert("text box editable", () => !commentEditor.TextBox.ReadOnly);
            }

            void assertLoggedOutState()
            {
                AddAssert("commit button hidden", () => commentEditor.ButtonsContainer[0].Alpha == 0);
                AddAssert("login button visible", () => commentEditor.ButtonsContainer[1].Alpha == 1);
                AddAssert("text box readonly", () => commentEditor.TextBox.ReadOnly);
            }

            // there's also the case of starting logged out, but more annoying to test.

            // starting logged in
            assertLoggedInState();

            // moving from logged in -> logged out
            AddStep("log out", () => dummyAPI.Logout());
            assertLoggedOutState();

            // moving from logged out -> logged in
            AddStep("log back in", () =>
            {
                dummyAPI.Login("username", "password");
                dummyAPI.AuthenticateSecondFactor("abcdefgh");
            });
            assertLoggedInState();
        }

        [Test]
        public void TestCommentsDisabled()
        {
            AddStep("no reason for disable", () => commentEditor.CommentableMeta.Value = new CommentableMeta
            {
                CurrentUserAttributes = new CommentableMeta.CommentableCurrentUserAttributes(),
            });
            AddAssert("textbox enabled", () => commentEditor.ChildrenOfType<TextBox>().Single().ReadOnly, () => Is.False);

            AddStep("specific reason for disable", () => commentEditor.CommentableMeta.Value = new CommentableMeta
            {
                CurrentUserAttributes = new CommentableMeta.CommentableCurrentUserAttributes
                {
                    CanNewCommentReason = "This comment section is disabled. For reasons.",
                }
            });
            AddAssert("textbox disabled", () => commentEditor.ChildrenOfType<TextBox>().Single().ReadOnly, () => Is.True);

            AddStep("entire commentable meta missing", () => commentEditor.CommentableMeta.Value = null);
            AddAssert("textbox enabled", () => commentEditor.ChildrenOfType<TextBox>().Single().ReadOnly, () => Is.False);

            AddStep("current user attributes missing", () => commentEditor.CommentableMeta.Value = new CommentableMeta
            {
                CurrentUserAttributes = null,
            });
            AddAssert("textbox enabled", () => commentEditor.ChildrenOfType<TextBox>().Single().ReadOnly, () => Is.True);
        }

        [Test]
        public void TestCancelAction()
        {
            AddStep("click cancel button", () =>
            {
                InputManager.MoveMouseTo(cancellableCommentEditor.ButtonsContainer[2]);
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("cancel action fired", () => cancellableCommentEditor.Cancelled);
        }

        private partial class TestCommentEditor : CommentEditor
        {
            public new Bindable<string> Current => base.Current;
            public new FillFlowContainer ButtonsContainer => base.ButtonsContainer;
            public new TextBox TextBox => base.TextBox;

            public string CommittedText { get; private set; } = string.Empty;

            public bool IsSpinnerShown => this.ChildrenOfType<LoadingSpinner>().Single().IsPresent;

            protected override void OnCommit(string value)
            {
                ShowLoadingSpinner = true;
                CommittedText = value;
                Scheduler.AddDelayed(() => ShowLoadingSpinner = false, 1000);
            }

            protected override LocalisableString FooterText => @"Footer text. And it is pretty long. Cool.";

            protected override LocalisableString GetButtonText(bool isLoggedIn) =>
                isLoggedIn ? @"Commit" : "You're logged out!";

            protected override LocalisableString GetPlaceholderText() => @"This text box is empty";
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

            protected override LocalisableString GetButtonText(bool isLoggedIn) => @"Save";
            protected override LocalisableString GetPlaceholderText() => @"Multiline textboxes soon";
        }
    }
}
