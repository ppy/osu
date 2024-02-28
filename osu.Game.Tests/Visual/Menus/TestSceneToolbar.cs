// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Moq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Overlays.Toolbar;
using osu.Game.Rulesets;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Menus
{
    [TestFixture]
    public partial class TestSceneToolbar : OsuManualInputManagerTestScene
    {
        private TestToolbar toolbar;

        [Resolved]
        private IRulesetStore rulesets { get; set; }

        [Cached]
        private readonly NowPlayingOverlay nowPlayingOverlay = new NowPlayingOverlay
        {
            Anchor = Anchor.TopRight,
            Origin = Anchor.TopRight,
            Y = Toolbar.HEIGHT,
        };

        [Cached]
        private readonly VolumeOverlay volumeOverlay = new VolumeOverlay
        {
            Anchor = Anchor.CentreLeft,
            Origin = Anchor.CentreLeft,
        };

        private readonly Mock<TestNotificationOverlay> notifications = new Mock<TestNotificationOverlay>();

        private readonly BindableInt unreadNotificationCount = new BindableInt();

        [BackgroundDependencyLoader]
        private void load()
        {
            Dependencies.CacheAs<INotificationOverlay>(notifications.Object);
            notifications.SetupGet(n => n.UnreadCount).Returns(unreadNotificationCount);
        }

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Remove(nowPlayingOverlay, false);
            Remove(volumeOverlay, false);

            Children = new Drawable[]
            {
                nowPlayingOverlay,
                volumeOverlay,
                toolbar = new TestToolbar { State = { Value = Visibility.Visible } },
            };
        });

        [Test]
        public void TestNotificationCounter()
        {
            setNotifications(1);
            setNotifications(2);
            setNotifications(3);
            setNotifications(0);
            setNotifications(144);

            void setNotifications(int count)
                => AddStep($"set notification count to {count}",
                    () => unreadNotificationCount.Value = count);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestRulesetSwitchingShortcut(bool toolbarHidden)
        {
            ToolbarRulesetSelector rulesetSelector = null;

            if (toolbarHidden)
                AddStep("hide toolbar", () => toolbar.Hide());

            AddStep("retrieve ruleset selector", () => rulesetSelector = toolbar.ChildrenOfType<ToolbarRulesetSelector>().Single());

            for (int i = 0; i < 4; i++)
            {
                var expected = rulesets.AvailableRulesets.ElementAt(i);
                var numberKey = Key.Number1 + i;

                AddStep($"switch to ruleset {i} via shortcut", () =>
                {
                    InputManager.PressKey(Key.ControlLeft);
                    InputManager.Key(numberKey);
                    InputManager.ReleaseKey(Key.ControlLeft);
                });

                AddUntilStep("ruleset switched", () => rulesetSelector.Current.Value.Equals(expected));
            }
        }

        [TestCase(OverlayActivation.All)]
        [TestCase(OverlayActivation.Disabled)]
        public void TestButtonKeyboardInputRespectsOverlayActivation(OverlayActivation mode)
        {
            AddStep($"set activation mode to {mode}", () => toolbar.OverlayActivationMode.Value = mode);
            AddStep("hide toolbar", () => toolbar.Hide());

            if (mode == OverlayActivation.Disabled)
                AddAssert("check buttons not accepting input", () => InputManager.NonPositionalInputQueue.OfType<ToolbarButton>().Count(), () => Is.Zero);
            else
                AddAssert("check buttons accepting input", () => InputManager.NonPositionalInputQueue.OfType<ToolbarButton>().Count(), () => Is.Not.Zero);
        }

        [TestCase(OverlayActivation.All)]
        [TestCase(OverlayActivation.Disabled)]
        public void TestRespectsOverlayActivation(OverlayActivation mode)
        {
            AddStep($"set activation mode to {mode}", () => toolbar.OverlayActivationMode.Value = mode);
            AddStep("hide toolbar", () => toolbar.Hide());
            AddStep("try to show toolbar", () => toolbar.Show());

            if (mode == OverlayActivation.Disabled)
                AddAssert("toolbar still hidden", () => toolbar.State.Value == Visibility.Hidden);
            else
                AddAssert("toolbar is visible", () => toolbar.State.Value == Visibility.Visible);
        }

        [Test]
        public void TestScrollInput()
        {
            OsuScrollContainer scroll = null;

            AddStep("add scroll layer", () => Add(scroll = new OsuScrollContainer
            {
                Depth = 1f,
                RelativeSizeAxes = Axes.Both,
                Child = new Box
                {
                    RelativeSizeAxes = Axes.X,
                    Height = DrawHeight * 2,
                    Colour = ColourInfo.GradientVertical(Color4.Gray, Color4.DarkGray),
                }
            }));

            AddStep("hover toolbar", () => InputManager.MoveMouseTo(toolbar));
            AddStep("perform scroll", () => InputManager.ScrollVerticalBy(500));
            AddAssert("not scrolled", () => scroll.Current == 0);
        }

        [Test]
        public void TestVolumeControlViaMusicButtonScroll()
        {
            AddStep("hover toolbar music button", () => InputManager.MoveMouseTo(this.ChildrenOfType<ToolbarMusicButton>().Single()));

            AddStep("reset volume", () => Audio.Volume.Value = 1);
            AddStep("hide volume overlay", () => volumeOverlay.Hide());

            AddRepeatStep("scroll down", () => InputManager.ScrollVerticalBy(-10), 5);
            AddAssert("volume lowered down", () => Audio.Volume.Value < 1);
            AddRepeatStep("scroll up", () => InputManager.ScrollVerticalBy(10), 5);
            AddAssert("volume raised up", () => Audio.Volume.Value == 1);

            AddStep("move mouse away", () => InputManager.MoveMouseTo(Vector2.Zero));
            AddAssert("button not hovered", () => !this.ChildrenOfType<ToolbarMusicButton>().Single().IsHovered);

            AddStep("set volume to 0.5", () => Audio.Volume.Value = 0.5);
            AddStep("hide volume overlay", () => volumeOverlay.Hide());

            AddRepeatStep("scroll down", () => InputManager.ScrollVerticalBy(-10), 5);
            AddAssert("volume not changed", () => Audio.Volume.Value == 0.5);
            AddRepeatStep("scroll up", () => InputManager.ScrollVerticalBy(10), 5);
            AddAssert("volume not changed", () => Audio.Volume.Value == 0.5);
        }

        [Test]
        public void TestVolumeControlViaMusicButtonArrowKeys()
        {
            AddStep("hover toolbar music button", () => InputManager.MoveMouseTo(this.ChildrenOfType<ToolbarMusicButton>().Single()));

            AddStep("reset volume", () => Audio.Volume.Value = 1);
            AddStep("hide volume overlay", () => volumeOverlay.Hide());

            AddRepeatStep("arrow down", () => InputManager.Key(Key.Down), 5);
            AddAssert("volume lowered down", () => Audio.Volume.Value < 1);
            AddRepeatStep("arrow up", () => InputManager.Key(Key.Up), 5);
            AddAssert("volume raised up", () => Audio.Volume.Value == 1);

            AddStep("hide volume overlay", () => volumeOverlay.Hide());
            AddStep("move mouse away", () => InputManager.MoveMouseTo(Vector2.Zero));
            AddAssert("button not hovered", () => !this.ChildrenOfType<ToolbarMusicButton>().Single().IsHovered);

            AddStep("set volume", () => Audio.Volume.Value = 0.5);
            AddStep("hide volume overlay", () => volumeOverlay.Hide());

            AddRepeatStep("arrow down", () => InputManager.Key(Key.Down), 5);
            AddAssert("volume not changed", () => Audio.Volume.Value == 0.5);
            AddRepeatStep("arrow up", () => InputManager.Key(Key.Up), 5);
            AddAssert("volume not changed", () => Audio.Volume.Value == 0.5);
        }

        [Test]
        public void TestRulesetSelectorOverflow()
        {
            AddStep("set toolbar width", () =>
            {
                toolbar.RelativeSizeAxes = Axes.None;
                toolbar.Width = 400;
            });
            AddStep("move mouse over news toggle button", () =>
            {
                var button = toolbar.ChildrenOfType<ToolbarNewsButton>().Single();
                InputManager.MoveMouseTo(button);
            });
            AddAssert("no ruleset toggle buttons hovered", () => !toolbar.ChildrenOfType<ToolbarRulesetTabButton>().Any(button => button.IsHovered));
            AddUntilStep("toolbar gradient visible", () => toolbar.ChildrenOfType<Toolbar.ToolbarBackground>().Single().Children.All(d => d.Alpha > 0));
        }

        public partial class TestToolbar : Toolbar
        {
            public new Bindable<OverlayActivation> OverlayActivationMode => base.OverlayActivationMode as Bindable<OverlayActivation>;
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
    }
}
