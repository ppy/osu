// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Logging;
using osu.Framework.Testing;
using osu.Game.Graphics.Sprites;
using osu.Game.Online;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Components
{
    [HeadlessTest]
    public partial class TestScenePollingComponent : OsuTestScene
    {
        private Container pollBox;
        private TestPoller poller;

        private const float safety_adjust = 1f;
        private int count;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            count = 0;

            Children = new Drawable[]
            {
                pollBox = new Container
                {
                    Alpha = 0,
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Scale = new Vector2(0.4f),
                            Colour = Color4.LimeGreen,
                            RelativeSizeAxes = Axes.Both,
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = "Poll!",
                        }
                    }
                }
            };
        });

        [Test]
        [Ignore("polling is threaded, and it's very hard to hook into it correctly")]
        public void TestInstantPolling()
        {
            createPoller(true);

            AddStep("set poll interval to 1", () => poller.TimeBetweenPolls.Value = TimePerAction * safety_adjust);
            checkCount(1);
            checkCount(2);
            checkCount(3);

            AddStep("set poll interval to 5", () => poller.TimeBetweenPolls.Value = TimePerAction * safety_adjust * 5);
            checkCount(4);
            checkCount(4);
            checkCount(4);

            skip();

            checkCount(5);
            checkCount(5);

            AddStep("set poll interval to 1", () => poller.TimeBetweenPolls.Value = TimePerAction * safety_adjust);
            checkCount(6);
            checkCount(7);
        }

        [Test]
        [Ignore("i have no idea how to fix the timing of this one")]
        public void TestSlowPolling()
        {
            createPoller(false);

            AddStep("set poll interval to 1", () => poller.TimeBetweenPolls.Value = TimePerAction * safety_adjust * 5);
            checkCount(0);
            skip();
            checkCount(0);
            skip();
            skip();
            checkCount(0);
            skip();
            skip();
            checkCount(0);
        }

        private void skip() => AddStep("skip", () =>
        {
            // could be 4 or 5 at this point due to timing discrepancies (safety_adjust @ 0.2 * 5 ~= 1)
            // easiest to just ignore the value at this point and move on.
        });

        private void checkCount(int checkValue)
        {
            AddAssert($"count is {checkValue}", () =>
            {
                Logger.Log($"value is {count}");
                return count == checkValue;
            });
        }

        private void createPoller(bool instant) => AddStep("create poller", () =>
        {
            poller?.Expire();

            Add(poller = instant ? new TestPoller() : new TestSlowPoller());
            poller.OnPoll += () =>
            {
                pollBox.FadeOutFromOne(500);
                count++;
            };
        });

        protected override double TimePerAction => 500;

        public partial class TestPoller : PollingComponent
        {
            public event Action OnPoll;

            protected override Task Poll()
            {
                Schedule(() => OnPoll?.Invoke());
                return base.Poll();
            }
        }

        public partial class TestSlowPoller : TestPoller
        {
            protected override Task Poll() => Task.Delay((int)(TimeBetweenPolls.Value / 2f / Clock.Rate)).ContinueWith(_ => base.Poll());
        }
    }
}
