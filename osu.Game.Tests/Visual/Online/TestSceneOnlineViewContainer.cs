﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online;
using osu.Game.Online.API;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public class TestSceneOnlineViewContainer : OsuTestScene
    {
        private readonly TestOnlineViewContainer onlineView;

        public TestSceneOnlineViewContainer()
        {
            Child = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Child = onlineView = new TestOnlineViewContainer()
            };
        }

        [Test]
        public void TestOnlineStateVisibility()
        {
            AddStep("set status to online", () => ((DummyAPIAccess)API).State = APIState.Online);

            AddUntilStep("children are visible", () => onlineView.ViewTarget.IsPresent);
            AddUntilStep("loading animation is not visible", () => !onlineView.LoadingAnimation.IsPresent);
        }

        [Test]
        public void TestOfflineStateVisibility()
        {
            AddStep("set status to offline", () => ((DummyAPIAccess)API).State = APIState.Offline);

            AddUntilStep("children are not visible", () => !onlineView.ViewTarget.IsPresent);
            AddUntilStep("loading animation is not visible", () => !onlineView.LoadingAnimation.IsPresent);
        }

        [Test]
        public void TestConnectingStateVisibility()
        {
            AddStep("set status to connecting", () => ((DummyAPIAccess)API).State = APIState.Connecting);

            AddUntilStep("children are not visible", () => !onlineView.ViewTarget.IsPresent);
            AddUntilStep("loading animation is visible", () => onlineView.LoadingAnimation.IsPresent);
        }

        [Test]
        public void TestFailingStateVisibility()
        {
            AddStep("set status to failing", () => ((DummyAPIAccess)API).State = APIState.Failing);

            AddUntilStep("children are not visible", () => !onlineView.ViewTarget.IsPresent);
            AddUntilStep("loading animation is visible", () => onlineView.LoadingAnimation.IsPresent);
        }

        private class TestOnlineViewContainer : OnlineViewContainer
        {
            public new LoadingAnimation LoadingAnimation => base.LoadingAnimation;

            public CompositeDrawable ViewTarget => base.Content;

            public TestOnlineViewContainer()
                : base(@"Please sign in to view dummy test content")
            {
                RelativeSizeAxes = Axes.Both;

                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Blue.Opacity(0.8f),
                    },
                    new OsuSpriteText
                    {
                        Text = "dummy online content",
                        Font = OsuFont.Default.With(size: 40),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    }
                };
            }
        }
    }
}
