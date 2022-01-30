// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
    public abstract class IntroTestScene : OsuTestScene
    {
        [Cached]
        private OsuLogo logo;

        protected abstract bool IntroReliesOnTrack { get; }

        protected OsuScreenStack IntroStack;

        private IntroScreen intro;

        [Cached]
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
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                }
            };
        }

        [Test]
        public virtual void TestPlayIntro()
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

                IntroStack.Push(intro = CreateScreen());
            });

            AddUntilStep("wait for menu", () => intro.DidLoadMenu);
        }

        [Test]
        public virtual void TestPlayIntroWithFailingAudioDevice()
        {
            AddStep("hide notifications", () => notifications.Hide());
            AddStep("restart sequence", () =>
            {
                logo.FinishTransforms();
                logo.IsTracking = false;

                IntroStack?.Expire();

                Add(IntroStack = new OsuScreenStack
                {
                    RelativeSizeAxes = Axes.Both,
                });

                IntroStack.Push(intro = CreateScreen());
            });

            AddStep("trigger failure", () =>
            {
                trackResetDelegate = Scheduler.AddDelayed(() =>
                {
                    intro.Beatmap.Value.Track.Seek(0);
                }, 0, true);
            });

            AddUntilStep("wait for menu", () => intro.DidLoadMenu);

            if (IntroReliesOnTrack)
                AddUntilStep("wait for notification", () => notifications.UnreadCount.Value == 1);

            AddStep("uninstall delegate", () => trackResetDelegate?.Cancel());
        }

        protected abstract IntroScreen CreateScreen();
    }
}
