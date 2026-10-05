// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Moq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Game.Overlays.FirstRunSetup;
using osu.Game.Overlays.Notifications;
using osu.Game.Screens;
using osu.Game.Tests.Beatmaps;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneFirstRunSetupOverlay : ScreenTestScene
    {
        private TestFirstRunSetupOverlayScreen screen = null!;
        private FirstRunSetupOverlay overlay => screen.Overlay;

        private readonly Mock<TestPerformerFromScreenRunner> performer = new Mock<TestPerformerFromScreenRunner>();

        private readonly Mock<TestNotificationOverlay> notificationOverlay = new Mock<TestNotificationOverlay>();

        private Notification lastNotification;

        protected OsuConfigManager LocalConfig;

        [BackgroundDependencyLoader]
        private void load()
        {
            Dependencies.Cache(LocalConfig = new OsuConfigManager(LocalStorage));
            Dependencies.CacheAs<IPerformFromScreenRunner>(performer.Object);
            Dependencies.CacheAs<INotificationOverlay>(notificationOverlay.Object);
            Dependencies.CacheAs<BeatmapStore>(new TestBeatmapStore());
        }

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("setup dependencies", () =>
            {
                performer.Reset();
                notificationOverlay.Reset();

                performer.Setup(g => g.PerformFromScreen(It.IsAny<Action<IScreen>>(), It.IsAny<IEnumerable<Type>>()))
                         .Callback((Action<IScreen> action, IEnumerable<Type> _) => action(null));

                notificationOverlay.Setup(n => n.Post(It.IsAny<Notification>()))
                                   .Callback((Notification n) => lastNotification = n);
            });

            AddStep("reset first run", () => LocalConfig.SetValue(OsuSetting.ShowFirstRunSetup, true));

            createScreen();
        }

        [Test]
        public void TestBasic()
        {
            AddAssert("overlay visible", () => overlay.State.Value == Visibility.Visible);
            AddAssert("footer visible", () => ScreenFooter.State.Value == Visibility.Visible);
        }

        [Test]
        public void TestDoesntOpenOnSecondRun()
        {
            AddStep("set first run", () => LocalConfig.SetValue(OsuSetting.ShowFirstRunSetup, true));

            AddUntilStep("step through", () =>
            {
                if (overlay.CurrentScreen?.IsLoaded != false) overlay.NextButton.AsNonNull().TriggerClick();
                return overlay.State.Value == Visibility.Hidden;
            });

            AddAssert("first run false", () => !LocalConfig.Get<bool>(OsuSetting.ShowFirstRunSetup));

            AddStep("exit screen", () => Stack.Exit());
            createScreen();

            AddWaitStep("wait some", 5);

            AddAssert("overlay didn't show", () => overlay.State.Value == Visibility.Hidden);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestOverlayRunsToFinish(bool keyboard)
        {
            AddUntilStep("step through", () =>
            {
                if (overlay.CurrentScreen?.IsLoaded != false)
                {
                    if (keyboard)
                        InputManager.Key(Key.Enter);
                    else
                        overlay.NextButton.AsNonNull().TriggerClick();
                }

                return overlay.State.Value == Visibility.Hidden;
            });

            AddUntilStep("wait for screens removed", () => !overlay.ChildrenOfType<Screen>().Any());

            AddStep("no notifications", () => notificationOverlay.VerifyNoOtherCalls());

            AddStep("display again on demand", () => overlay.Show());

            AddUntilStep("back at start", () => overlay.CurrentScreen is ScreenWelcome);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestBackButton(bool keyboard)
        {
            AddUntilStep("step to last", () =>
            {
                var nextButton = overlay.NextButton.AsNonNull();

                if (overlay.CurrentScreen?.IsLoaded != false)
                    nextButton.TriggerClick();

                return nextButton.Text == CommonStrings.Finish;
            });

            AddUntilStep("step back to start", () =>
            {
                if (overlay.CurrentScreen?.IsLoaded != false && !(overlay.CurrentScreen is ScreenWelcome))
                {
                    if (keyboard)
                        InputManager.Key(Key.Escape);
                    else
                        ScreenFooter.BackButton.TriggerClick();
                }

                return overlay.CurrentScreen is ScreenWelcome;
            });

            AddAssert("overlay not dismissed", () => overlay.State.Value == Visibility.Visible);

            if (keyboard)
            {
                AddStep("exit via keyboard", () => InputManager.Key(Key.Escape));
                AddAssert("overlay dismissed", () => overlay.State.Value == Visibility.Hidden);
            }
            else
            {
                AddStep("press back button", () => ScreenFooter.BackButton.TriggerClick());
                AddAssert("overlay dismissed", () => overlay.State.Value == Visibility.Hidden);
            }
        }

        [Test]
        public void TestClickAwayToExit()
        {
            AddStep("click inside content", () =>
            {
                InputManager.MoveMouseTo(overlay.ScreenSpaceDrawQuad.Centre);
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("overlay not dismissed", () => overlay.State.Value == Visibility.Visible);

            AddStep("click outside content", () =>
            {
                InputManager.MoveMouseTo(new Vector2(overlay.ScreenSpaceDrawQuad.TopLeft.X, overlay.ScreenSpaceDrawQuad.Centre.Y));
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("overlay dismissed", () => overlay.State.Value == Visibility.Hidden);
        }

        [Test]
        public void TestResumeViaNotification()
        {
            AddStep("step to next", () => overlay.NextButton.AsNonNull().TriggerClick());

            AddAssert("is at known screen", () => overlay.CurrentScreen is ScreenUIScale);

            AddStep("hide", () => overlay.Hide());
            AddAssert("overlay hidden", () => overlay.State.Value == Visibility.Hidden);

            AddStep("notification arrived", () => notificationOverlay.Verify(n => n.Post(It.IsAny<Notification>()), Times.Once));

            AddStep("run notification action", () => lastNotification.Activated?.Invoke());

            AddAssert("overlay shown", () => overlay.State.Value == Visibility.Visible);
            AddAssert("is resumed", () => overlay.CurrentScreen is ScreenUIScale);
        }

        private void createScreen()
        {
            AddStep("push screen", () => LoadScreen(screen = new TestFirstRunSetupOverlayScreen()));
            AddUntilStep("wait until screen is loaded", () => screen.IsLoaded, () => Is.True);
        }

        private partial class TestFirstRunSetupOverlayScreen : OsuScreen
        {
            public override bool ShowFooter => true;

            public FirstRunSetupOverlay Overlay = null!;

            [CanBeNull]
            private IDisposable overlayRegistration;

            [CanBeNull]
            [Resolved]
            private IOverlayManager overlayManager { get; set; }

            [Cached]
            private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

            [BackgroundDependencyLoader]
            private void load()
            {
                LoadComponent(Overlay = new FirstRunSetupOverlay());
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                overlayRegistration = overlayManager?.RegisterBlockingOverlay(Overlay);
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);
                overlayRegistration?.Dispose();
            }
        }

        // interface mocks break hot reload, mocking this stub implementation instead works around it.
        // see: https://github.com/moq/moq4/issues/1252
        [UsedImplicitly]
        public class TestNotificationOverlay : INotificationOverlay
        {
            public virtual void Post(Notification notification)
            {
            }

            public virtual void Hide()
            {
            }

            public virtual IBindable<int> UnreadCount { get; } = new Bindable<int>();

            public IEnumerable<Notification> AllNotifications => Enumerable.Empty<Notification>();
        }

        // interface mocks break hot reload, mocking this stub implementation instead works around it.
        // see: https://github.com/moq/moq4/issues/1252
        [UsedImplicitly]
        public class TestPerformerFromScreenRunner : IPerformFromScreenRunner
        {
            public virtual void PerformFromScreen(Action<IScreen> action, IEnumerable<Type> validScreens = null)
            {
            }
        }
    }
}
