// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Sprites;
using osu.Game.Online;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual
{
    public class TestCasePollingComponent : OsuTestCase
    {
        private Container pollBox;
        private TestPoller poller;

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                poller = new TestPoller(),
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

            int count = 0;

            poller.OnPoll += () =>
            {
                pollBox.FadeOutFromOne(500);
                count++;
            };

            AddStep("set poll to 1 second", () => poller.TimeBetweenPolls = TimePerAction);

            void checkCount(int checkValue) => AddAssert($"count is {checkValue}", () => count == checkValue);

            checkCount(1);
            checkCount(2);
            checkCount(3);

            AddStep("set poll to 5 second", () => poller.TimeBetweenPolls = TimePerAction * 5);

            checkCount(4);
            checkCount(4);
            checkCount(4);
            checkCount(4);

            checkCount(5);
            checkCount(5);
            checkCount(5);

            AddStep("set poll to 5 second", () => poller.TimeBetweenPolls = TimePerAction);

            AddAssert("count is 6", () => count == 6);

        }

        protected override double TimePerAction => 500;

        public class TestPoller : PollingComponent
        {
            public event Action OnPoll;

            protected override Task Poll()
            {
                OnPoll?.Invoke();
                return base.Poll();
            }
        }
    }
}
