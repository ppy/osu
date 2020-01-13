// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Sprites;
using osu.Game.Online;
using osu.Game.Online.API;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public class TestSceneOnlineViewContainer : OsuTestScene
    {
        private OnlineViewContainer onlineView;
        private Box box;

        public TestSceneOnlineViewContainer()
        {
            Child = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Child = onlineView = new OnlineViewContainer(@"Please login to view dummy test content")
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        box = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Blue.Opacity(0.8f),
                        },
                        new OsuSpriteText
                        {
                            Text = "dummy online content",
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddStep("set status to offline", () =>
            {
                (API as DummyAPIAccess).State = APIState.Offline;
            });

            AddAssert("children are hidden", () =>
            {
                return !onlineView.Children.First().Parent.IsPresent;
            });

            AddStep("set status to online", () =>
            {
                (API as DummyAPIAccess).State = APIState.Online;
            });

            AddAssert("children are visible", () =>
            {
                return onlineView.Children.First().Parent.IsPresent;
            });
        }
    }
}
