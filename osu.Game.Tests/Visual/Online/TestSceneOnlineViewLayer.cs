// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online;
using osu.Game.Online.API;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public class TestSceneOnlineViewLayer : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(OnlineViewLayer)
        };

        private readonly Container con;

        private readonly TestOnlineViewLayer view;

        public TestSceneOnlineViewLayer()
        {
            Child = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    con = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.Blue,
                            },
                            new OsuButton
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Size = new Vector2(500, 500),
                                Text = "Click me!",
                                Action = () => { }
                            }
                        }
                    },
                    view = new TestOnlineViewLayer("Please sign in to view content!", con)
                }
            };
        }

        [Test]
        public void TestOfflineStateVisibility()
        {
            AddStep("set status to offline", () => ((DummyAPIAccess)API).State = APIState.Offline);
            AddWaitStep("wait for animation to finish", 5);
            AddAssert("content is dimmed", () => con.Colour != Color4.White);
            AddAssert("loading animation is not visible", () => !view.LoadingSpinner.IsPresent);
        }

        [Test]
        public void TestConnectingStateVisibility()
        {
            AddStep("set status to connecting", () => ((DummyAPIAccess)API).State = APIState.Connecting);
            AddWaitStep("wait for animation to finish", 5);
            AddAssert("content is dimmed", () => con.Colour != Color4.White);
            AddUntilStep("loading animation is visible", () => view.LoadingSpinner.IsPresent);
        }

        [Test]
        public void TestFailingStateVisibility()
        {
            AddStep("set status to failing", () => ((DummyAPIAccess)API).State = APIState.Failing);
            AddWaitStep("wait for animation to finish", 5);
            AddAssert("content is dimmed", () => con.Colour != Color4.White);
            AddAssert("loading animation is visible", () => view.LoadingSpinner.IsPresent);
        }

        [Test]
        public void TestOnlineStateVisibility()
        {
            AddStep("set status to online", () => ((DummyAPIAccess)API).State = APIState.Online);
            AddWaitStep("wait for animation to finish", 5);
            AddAssert("content is not dimmed", () => con.Colour == Color4.White);
            AddAssert("loading animation is not visible", () => !view.LoadingSpinner.IsPresent);
        }

        private class TestOnlineViewLayer : OnlineViewLayer
        {
            public TestOnlineViewLayer(string placeholderMessage, Drawable viewTarget)
                : base(placeholderMessage, viewTarget)
            {
            }

            public new LoadingSpinner LoadingSpinner => base.LoadingSpinner;
        }
    }
}
