// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Game.Online.API;
using osu.Game.Online.Solo;
using osu.Game.Overlays.Toolbar;
using osu.Game.Scoring;
using osu.Game.Users;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Menus
{
    [TestFixture]
    public partial class TestSceneToolbarUserButton : OsuManualInputManagerTestScene
    {
        private DummyAPIAccess dummyAPI => (DummyAPIAccess)API;

        public TestSceneToolbarUserButton()
        {
            Container mainContainer;

            Children = new Drawable[]
            {
                mainContainer = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    Height = Toolbar.HEIGHT,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = Color4.Black,
                            RelativeSizeAxes = Axes.Both,
                        },
                        new FillFlowContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Y,
                            AutoSizeAxes = Axes.X,
                            Direction = FillDirection.Horizontal,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Colour = Color4.DarkRed,
                                    RelativeSizeAxes = Axes.Y,
                                    Width = 2,
                                },
                                new ToolbarUserButton(),
                                new Box
                                {
                                    Colour = Color4.DarkRed,
                                    RelativeSizeAxes = Axes.Y,
                                    Width = 2,
                                },
                            }
                        },
                    }
                },
            };

            AddSliderStep("scale", 0.5, 4, 1, scale => mainContainer.Scale = new Vector2((float)scale));
        }

        [Test]
        public void TestLoginLogout()
        {
            AddStep("Log out", () => dummyAPI.Logout());
            AddStep("Log in", () => dummyAPI.Login("wang", "jang"));
            AddStep("Authenticate via second factor", () => dummyAPI.AuthenticateSecondFactor("abcdefgh"));
        }

        [Test]
        public void TestStates()
        {
            AddStep("Log in", () => dummyAPI.Login("wang", "jang"));
            AddStep("Authenticate via second factor", () => dummyAPI.AuthenticateSecondFactor("abcdefgh"));

            foreach (var state in Enum.GetValues<APIState>())
            {
                AddStep($"Change state to {state}", () => dummyAPI.SetState(state));
            }
        }

        [Test]
        public void TestTransientUserStatisticsDisplay()
        {
            AddStep("Log in", () => dummyAPI.Login("wang", "jang"));
            AddStep("Gain", () =>
            {
                var transientUpdateDisplay = this.ChildrenOfType<TransientUserStatisticsUpdateDisplay>().Single();
                transientUpdateDisplay.LatestUpdate.Value = new UserStatisticsUpdate(
                    new ScoreInfo(),
                    new UserStatistics
                    {
                        GlobalRank = 123_456,
                        PP = 1234
                    },
                    new UserStatistics
                    {
                        GlobalRank = 111_111,
                        PP = 1357
                    });
            });
            AddStep("Loss", () =>
            {
                var transientUpdateDisplay = this.ChildrenOfType<TransientUserStatisticsUpdateDisplay>().Single();
                transientUpdateDisplay.LatestUpdate.Value = new UserStatisticsUpdate(
                    new ScoreInfo(),
                    new UserStatistics
                    {
                        GlobalRank = 111_111,
                        PP = 1357
                    },
                    new UserStatistics
                    {
                        GlobalRank = 123_456,
                        PP = 1234
                    });
            });
            AddStep("No change", () =>
            {
                var transientUpdateDisplay = this.ChildrenOfType<TransientUserStatisticsUpdateDisplay>().Single();
                transientUpdateDisplay.LatestUpdate.Value = new UserStatisticsUpdate(
                    new ScoreInfo(),
                    new UserStatistics
                    {
                        GlobalRank = 111_111,
                        PP = 1357
                    },
                    new UserStatistics
                    {
                        GlobalRank = 111_111,
                        PP = 1357
                    });
            });
            AddStep("Was null", () =>
            {
                var transientUpdateDisplay = this.ChildrenOfType<TransientUserStatisticsUpdateDisplay>().Single();
                transientUpdateDisplay.LatestUpdate.Value = new UserStatisticsUpdate(
                    new ScoreInfo(),
                    new UserStatistics
                    {
                        GlobalRank = null,
                        PP = null
                    },
                    new UserStatistics
                    {
                        GlobalRank = 111_111,
                        PP = 1357
                    });
            });
            AddStep("Became null", () =>
            {
                var transientUpdateDisplay = this.ChildrenOfType<TransientUserStatisticsUpdateDisplay>().Single();
                transientUpdateDisplay.LatestUpdate.Value = new UserStatisticsUpdate(
                    new ScoreInfo(),
                    new UserStatistics
                    {
                        GlobalRank = 111_111,
                        PP = 1357
                    },
                    new UserStatistics
                    {
                        GlobalRank = null,
                        PP = null
                    });
            });
        }
    }
}
