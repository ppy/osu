// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Threading;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public partial class TestSceneDialogOverlay : OsuTestScene
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
            TestPopupDialog firstDialog = null;
            TestPopupDialog secondDialog = null;

            AddStep("dialog #1", () => overlay.Push(firstDialog = new TestPopupDialog
            {
                Icon = FontAwesome.Regular.TrashAlt,
                HeaderText = @"Confirm deletion of",
                BodyText = @"Ayase Rie - Yuima-ru*World TVver.",
                Buttons = new PopupDialogButton[]
                {
                    new PopupDialogOkButton
                    {
                        Text = @"I never want to see this again.",
                        Action = () => Console.WriteLine(@"OK"),
                    },
                    new PopupDialogCancelButton
                    {
                        Text = @"Firetruck, I still want quick ranks!",
                        Action = () => Console.WriteLine(@"Cancel"),
                    },
                },
            }));

            AddAssert("first dialog displayed", () => overlay.CurrentDialog == firstDialog);

            AddStep("dialog #2", () => overlay.Push(secondDialog = new TestPopupDialog
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

            AddAssert("second dialog displayed", () => overlay.CurrentDialog == secondDialog);
            AddUntilStep("first dialog is not part of hierarchy", () => firstDialog.Parent == null);
        }

        [Test]
        public void TestPushBeforeLoad()
        {
            PopupDialog dialog = null;

            AddStep("create slow loading dialog overlay", () => overlay = new SlowLoadingDialogOverlay());

            AddStep("start loading overlay", () => LoadComponentAsync(overlay, Add));

            AddStep("push dialog before loaded", () =>
            {
                overlay.Push(dialog = new TestPopupDialog
                {
                    Buttons = new PopupDialogButton[]
                    {
                        new PopupDialogOkButton { Text = @"OK" },
                    },
                });
            });

            AddStep("complete load", () => ((SlowLoadingDialogOverlay)overlay).LoadEvent.Set());

            AddUntilStep("wait for load", () => overlay.IsLoaded);

            AddAssert("dialog displayed", () => overlay.CurrentDialog == dialog);
        }

        public partial class SlowLoadingDialogOverlay : DialogOverlay
        {
            public ManualResetEventSlim LoadEvent = new ManualResetEventSlim();

            [BackgroundDependencyLoader]
            private void load()
            {
                LoadEvent.Wait(10000);
            }
        }

        [Test]
        public void TestDismissBeforePush()
        {
            TestPopupDialog testDialog = null;
            AddStep("dismissed dialog push", () =>
            {
                overlay.Push(testDialog = new TestPopupDialog
                {
                    State = { Value = Visibility.Hidden }
                });
            });

            AddAssert("no dialog pushed", () => overlay.CurrentDialog == null);
            AddAssert("dialog is not part of hierarchy", () => testDialog.Parent == null);
        }

        [Test]
        public void TestDismissBeforePushViaButtonPress()
        {
            TestPopupDialog testDialog = null;
            AddStep("dismissed dialog push", () =>
            {
                overlay.Push(testDialog = new TestPopupDialog
                {
                    Buttons = new PopupDialogButton[]
                    {
                        new PopupDialogOkButton { Text = @"OK" },
                    },
                });

                testDialog.PerformOkAction();
            });

            AddAssert("no dialog pushed", () => overlay.CurrentDialog == null);
            AddUntilStep("dialog is not part of hierarchy", () => testDialog.Parent == null);
        }

        private partial class TestPopupDialog : PopupDialog
        {
        }
    }
}
