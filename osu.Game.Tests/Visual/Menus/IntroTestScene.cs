// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Threading;
using osu.Game.Overlays;
using osu.Game.Screens;
using osu.Game.Screens.Menu;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Menus
{
    [TestFixture]
    public abstract partial class IntroTestScene : OsuTestScene
    {
        [Cached]
        private OsuLogo logo;

        protected abstract bool IntroReliesOnTrack { get; }

        protected OsuScreenStack IntroStack;

        protected IntroScreen Intro { get; private set; }

        [Cached(typeof(INotificationOverlay))]
        private NotificationOverlay notifications;

        private ScheduledDelegate trackResetDelegate;

        protected IntroTestScene()
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = float.MaxValue,
                    Colour = Color4.Black,
                },
                logo = new OsuLogo
                {
                    Alpha = 0,
                    RelativePositionAxes = Axes.Both,
                    Depth = float.MinValue,
                    Position = new Vector2(0.5f),
                },
                notifications = new NotificationOverlay
                {
                    Depth = float.MinValue,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                }
            };
        }

        [Test]
        public virtual void TestPlayIntro()
        {
            RestartIntro();

            WaitForMenu();
        }

        [Test]
        public virtual void TestPlayIntroWithFailingAudioDevice()
        {
            AddStep("reset notifications", () =>
            {
                notifications.Show();
                notifications.Hide();
            });

            AddUntilStep("wait for no notifications", () => notifications.UnreadCount.Value, () => Is.EqualTo(0));

            AddStep("restart sequence", () =>
            {
                logo.FinishTransforms();
                logo.IsTracking = false;

                IntroStack?.Expire();

                Add(IntroStack = new OsuScreenStack
                {
                    RelativeSizeAxes = Axes.Both,
                });

                IntroStack.Push(Intro = CreateScreen());
            });

            AddStep("trigger failure", () =>
            {
                trackResetDelegate = Scheduler.AddDelayed(() =>
                {
                    Intro.Beatmap.Value.Track.Seek(0);
                }, 0, true);
            });

            WaitForMenu();

            if (IntroReliesOnTrack)
                AddUntilStep("wait for notification", () => notifications.UnreadCount.Value == 1);

            AddStep("uninstall delegate", () => trackResetDelegate?.Cancel());
        }

        protected void RestartIntro()
        {
            AddStep("restart sequence", () =>
            {
                logo.FinishTransforms();
                logo.IsTracking = false;

                IntroStack?.Expire();

                Add(IntroStack = new OsuScreenStack
                {
                    RelativeSizeAxes = Axes.Both,
                });

                IntroStack.Push(Intro = CreateScreen());
            });
        }

        protected void WaitForMenu()
        {
            AddUntilStep("wait for menu", () => Intro.DidLoadMenu);
        }

        protected abstract IntroScreen CreateScreen();
    }
}
