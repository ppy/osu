// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Testing;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online;
using osu.Game.Online.API;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public class TestSceneOnlineViewLayer : ManualInputManagerTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(OnlineViewLayer)
        };

        private readonly Container con;

        private readonly InputBox box;

        private readonly OnlineViewLayer view;

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
                        Child = box = new InputBox
                        {
                            RelativeSizeAxes = Axes.Both
                        }
                    },
                    view = new OnlineViewLayer("Please sign in to view content!", con)
                }
            };
        }

        [SetUp]
        public void Setup()
        {
            InputManager.MoveMouseTo(box);
        }

        [Test]
        public void TestOfflineStateVisibility()
        {
            AddStep("set status to offline", () => ((DummyAPIAccess)API).State = APIState.Offline);
            AddUntilStep("content is dimmed", () => con.Colour != Color4.White);
            AddAssert("loading animation is not visible", () => !view.ChildrenOfType<LoadingSpinner>().First().IsPresent);
            AddAssert("input is blocked", () => !box.IsHovered);
        }

        [Test]
        public void TestConnectingStateVisibility()
        {
            AddStep("set status to connecting", () => ((DummyAPIAccess)API).State = APIState.Connecting);
            AddUntilStep("content is dimmed", () => con.Colour != Color4.White);
            AddUntilStep("loading animation is visible", () => view.ChildrenOfType<LoadingSpinner>().First().IsPresent);
            AddAssert("input is blocked", () => !box.IsHovered);
        }

        [Test]
        public void TestFailingStateVisibility()
        {
            AddStep("set status to failing", () => ((DummyAPIAccess)API).State = APIState.Failing);
            AddAssert("content is dimmed", () => con.Colour != Color4.White);
            AddAssert("loading animation is visible", () => view.ChildrenOfType<LoadingSpinner>().First().IsPresent);
            AddAssert("input is blocked", () => !box.IsHovered);
        }

        [Test]
        public void TestOnlineStateVisibility()
        {
            AddStep("set status to online", () => ((DummyAPIAccess)API).State = APIState.Online);
            AddUntilStep("content is not dimmed", () => con.Colour == Color4.White);
            AddAssert("loading animation is not visible", () => !view.ChildrenOfType<LoadingSpinner>().First().IsPresent);
            AddAssert("input is not blocked", () => box.IsHovered);
        }

        private class InputBox : Container
        {
            private readonly OsuSpriteText text;

            public InputBox()
            {
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Blue,
                    },
                    text = new OsuSpriteText
                    {
                        Font = OsuFont.Default.With(size: 40),
                        Anchor = Anchor.Centre,
                        Margin = new MarginPadding { Top = 50 },
                        Origin = Anchor.Centre,
                    }
                };
            }

            protected override bool OnHover(HoverEvent e)
            {
                text.Text = "receives input";
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                text.Text = "doesn't receive input";
                base.OnHoverLost(e);
            }
        }
    }
}
