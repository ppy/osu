// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Configuration;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Menu.Exiting;
using osu.Game.Tests.Visual.Navigation;

namespace osu.Game.Tests.Visual.Menus
{
    [System.ComponentModel.Description("Ensurance of exiting from game safely and intentionally")]
    public class TestSceneSafeExiting : OsuGameTestScene
    {
        private MainMenu mainMenu => (MainMenu)Game.ScreenStack.CurrentScreen;
        private NotificationOverlay notificationOverlay => Game.ChildrenOfType<NotificationOverlay>().Single();

        [Test]
        public void TestConfirmExitOnZeroHoldToExit()
        {
            performHoldToExit(0f);
            waitForDialogAndClose<ConfirmExitDialog>();
        }

        [Test]
        public void TestConfirmExitOnWindowCloseRequest()
        {
            requestCloseWindow();
            waitForDialogAndClose<ConfirmExitDialog>();
        }

        [Test]
        public void TestCannotExitWithUnCancellableWork()
        {
            doUnCancellableWork();
            AddStep("hide notifications", () => notificationOverlay.State.Value = Visibility.Hidden);

            pressExitButton();
            waitForDialogAndClose<CannotExitDialog>();

            // notifications overlay should be displayed to the user to show the un-cancellable work.
            AddAssert("notifications visible", () => notificationOverlay.State.Value == Visibility.Visible);

            performHoldToExit(200f);
            waitForDialogAndClose<CannotExitDialog>();

            performHoldToExit(0f);
            waitForDialogAndClose<CannotExitDialog>();

            requestCloseWindow();
            waitForDialogAndClose<CannotExitDialog>();
        }

        #region Exit methods

        private void pressExitButton() => AddStep("press exit button", () => mainMenu.ChildrenOfType<ButtonSystem>().Single().OnExit.Invoke());

        private void performHoldToExit(float activationDelay)
        {
            AddStep($"set hold-delay to {activationDelay}", () => Game.LocalConfig.Set(OsuSetting.UIHoldActivationDelay, activationDelay));
            AddStep("perform hold-to-exit", () => mainMenu.ChildrenOfType<ExitConfirmOverlay>().Single().OnPressed(GlobalAction.Back));
        }

        private void requestCloseWindow() => AddStep("request window close", () => Game.GracefullyExit(false));

        #endregion

        private void doUnCancellableWork()
        {
            AddStep("do un-cancellable work", () => notificationOverlay.Post(new ProgressNotification
            {
                Text = "Some un-cancellable work",
                State = ProgressNotificationState.Active,
                Cancellable = false,
            }));

            // post requests take a bit before they're processed if overlay is hidden, wait for that.
            AddUntilStep("wait for posting", () => notificationOverlay.UnreadCount.Value > 0);
        }

        private void waitForDialogAndClose<T>()
        {
            AddUntilStep($"wait for '{typeof(T).ReadableName()}' dialog", () =>
                Game.ChildrenOfType<DialogOverlay>().Single().CurrentDialog is T);

            AddStep("close dialog", () => Game.ChildrenOfType<DialogOverlay>().Single().Hide());
        }
    }
}
