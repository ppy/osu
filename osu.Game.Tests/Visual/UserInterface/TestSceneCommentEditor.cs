// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
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
    public class TestSceneCommentEditor : ManualInputManagerTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(CommentEditor),
            typeof(CancellableCommentEditor),
        };

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        private TestCommentEditor commentEditor;
        private TestCancellableCommentEditor cancellableCommentEditor;
        private string commitText;
        private bool cancelActionFired;

        [SetUp]
        public void SetUp()
        {
            commitText = string.Empty;
            cancelActionFired = false;

            Schedule(() => Add(new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Y,
                Width = 800,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 20),
                Children = new Drawable[]
                {
                    commentEditor = new TestCommentEditor
                    {
                        OnCommit = onCommit,
                    },
                    cancellableCommentEditor = new TestCancellableCommentEditor
                    {
                        OnCommit = onCommit,
                        OnCancel = onCancel
                    }
                }
            }));
        }

        [Test]
        public void TestCommitViaKeyboard()
        {
            AddStep("Click on textbox", () =>
            {
                InputManager.MoveMouseTo(commentEditor);
                InputManager.Click(MouseButton.Left);
            });
            AddStep("Write something", () => commentEditor.Current.Value = "text");
            AddStep("Click Enter", () => press(Key.Enter));
            AddAssert("Text has been invoked", () => !string.IsNullOrEmpty(commitText));
            AddAssert("Button is loading", () => commentEditor.IsLoading);
        }

        [Test]
        public void TestCommitViaKeyboardWhenEmpty()
        {
            AddStep("Click on textbox", () =>
            {
                InputManager.MoveMouseTo(commentEditor);
                InputManager.Click(MouseButton.Left);
            });
            AddStep("Click Enter", () => press(Key.Enter));
            AddAssert("Text not invoked", () => string.IsNullOrEmpty(commitText));
            AddAssert("Button is not loading", () => !commentEditor.IsLoading);
        }

        [Test]
        public void TestCommitViaButton()
        {
            AddStep("Click on textbox", () =>
            {
                InputManager.MoveMouseTo(commentEditor);
                InputManager.Click(MouseButton.Left);
            });
            AddStep("Write something", () => commentEditor.Current.Value = "text");
            AddStep("Click on button", () =>
            {
                InputManager.MoveMouseTo(commentEditor.ButtonsContainer);
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("Text has been invoked", () => !string.IsNullOrEmpty(commitText));
            AddAssert("Button is loading", () => commentEditor.IsLoading);
        }

        [Test]
        public void TestCancelAction()
        {
            AddStep("Click on cancel button", () =>
            {
                InputManager.MoveMouseTo(cancellableCommentEditor.ButtonsContainer);
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("Cancel action is fired", () => cancelActionFired);
        }

        private void onCommit(string value)
        {
            commitText = value;

            Scheduler.AddDelayed(() =>
            {
                commentEditor.IsLoading = false;
                cancellableCommentEditor.IsLoading = false;
            }, 1000);
        }

        private void onCancel() => cancelActionFired = true;

        private void press(Key key)
        {
            InputManager.PressKey(key);
            InputManager.ReleaseKey(key);
        }

        private class TestCommentEditor : CommentEditor
        {
            public new Bindable<string> Current => base.Current;

            public new FillFlowContainer ButtonsContainer => base.ButtonsContainer;

            protected override string FooterText => @"Footer text. And it is pretty long. Cool.";

            protected override string CommitButtonText => @"Commit";

            protected override string TextboxPlaceholderText => @"This textbox is empty";
        }

        private class TestCancellableCommentEditor : CancellableCommentEditor
        {
            public new FillFlowContainer ButtonsContainer => base.ButtonsContainer;

            protected override string FooterText => @"Wow, another one. Sicc";

            protected override string CommitButtonText => @"Save";

            protected override string TextboxPlaceholderText => @"Miltiline textboxes soon";
        }
    }
}
