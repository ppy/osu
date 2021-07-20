// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public class TestSceneDialogOverlay : OsuTestScene
    {
        private DialogOverlay overlay;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create dialog overlay", () => Child = overlay = new DialogOverlay());
        }

        [Test]
        public void TestBasic()
        {
            TestPopupDialog dialog = null;

            AddStep("dialog #1", () => overlay.Push(dialog = new TestPopupDialog
            {
                Icon = FontAwesome.Regular.TrashAlt,
                HeaderText = @"Confirm deletion of",
                BodyText = @"Ayase Rie - Yuima-ru*World TVver.",
                Buttons = new PopupDialogButton[]
                {
                    new PopupDialogOkButton
                    {
                        Text = @"I never want to see this again.",
                        Action = () => System.Console.WriteLine(@"OK"),
                    },
                    new PopupDialogCancelButton
                    {
                        Text = @"Firetruck, I still want quick ranks!",
                        Action = () => System.Console.WriteLine(@"Cancel"),
                    },
                },
            }));

            AddAssert("first dialog displayed", () => overlay.CurrentDialog == dialog);

            AddStep("dialog #2", () => overlay.Push(dialog = new TestPopupDialog
            {
                Icon = FontAwesome.Solid.Cog,
                HeaderText = @"What do you want to do with",
                BodyText = "Camellia as \"Bang Riot\" - Blastix Riotz",
                Buttons = new PopupDialogButton[]
                {
                    new PopupDialogOkButton
                    {
                        Text = @"Manage collections",
                    },
                    new PopupDialogOkButton
                    {
                        Text = @"Delete...",
                    },
                    new PopupDialogOkButton
                    {
                        Text = @"Remove from unplayed",
                    },
                    new PopupDialogOkButton
                    {
                        Text = @"Clear local scores",
                    },
                    new PopupDialogOkButton
                    {
                        Text = @"Edit",
                    },
                    new PopupDialogCancelButton
                    {
                        Text = @"Cancel",
                    },
                },
            }));

            AddAssert("second dialog displayed", () => overlay.CurrentDialog == dialog);
        }

        [Test]
        public void TestDismissBeforePush()
        {
            AddStep("dismissed dialog push", () =>
            {
                overlay.Push(new TestPopupDialog
                {
                    State = { Value = Visibility.Hidden }
                });
            });

            AddAssert("no dialog pushed", () => overlay.CurrentDialog == null);
        }

        [Test]
        public void TestDismissBeforePushViaButtonPress()
        {
            AddStep("dismissed dialog push", () =>
            {
                TestPopupDialog dialog;
                overlay.Push(dialog = new TestPopupDialog
                {
                    Buttons = new PopupDialogButton[]
                    {
                        new PopupDialogOkButton { Text = @"OK" },
                    },
                });

                dialog.PerformOkAction();
            });

            AddAssert("no dialog pushed", () => overlay.CurrentDialog == null);
        }

        private class TestPopupDialog : PopupDialog
        {
        }
    }
}
