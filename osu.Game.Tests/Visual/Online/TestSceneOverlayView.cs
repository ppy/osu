// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.Placeholders;
using osu.Game.Overlays;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneOverlayView : OsuTestScene
    {
        private readonly TestOverlayView view;

        public TestSceneOverlayView()
        {
            Child = view = new TestOverlayView
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
            };
        }

        [Test]
        public void TestOfflineStatus()
        {
            AddStep("set status to offline", () => ((DummyAPIAccess)API).State = APIState.Offline);
            AddUntilStep("placeholder is visible", () => view.InternalContents.OfType<LoginPlaceholder>().First().IsPresent);
            AddUntilStep("spinner is hidden", () => !view.InternalContents.OfType<LoadingSpinner>().First().IsPresent);
        }

        [Test]
        public void TestConnectingStatus()
        {
            AddStep("set status to connecting", () => ((DummyAPIAccess)API).State = APIState.Connecting);
            AddUntilStep("placeholder is hidden", () => !view.InternalContents.OfType<LoginPlaceholder>().First().IsPresent);
            AddUntilStep("spinner is visible", () => view.InternalContents.OfType<LoadingSpinner>().First().IsPresent);
        }

        [Test]
        public void TestFailingStatus()
        {
            AddStep("set status to failing", () => ((DummyAPIAccess)API).State = APIState.Failing);
            AddUntilStep("placeholder is hidden", () => !view.InternalContents.OfType<LoginPlaceholder>().First().IsPresent);
            AddUntilStep("spinner is visible", () => view.InternalContents.OfType<LoadingSpinner>().First().IsPresent);
        }

        [Test]
        public void TestOnlineStatus()
        {
            AddStep("setup API request handler", () => ((DummyAPIAccess)API).HandleRequest = req => req.TriggerSuccess());
            AddStep("set status to online", () => ((DummyAPIAccess)API).State = APIState.Online);
            AddUntilStep("placeholder is hidden", () => !view.InternalContents.OfType<LoginPlaceholder>().First().IsPresent);
            AddUntilStep("spinner is hidden", () => !view.InternalContents.OfType<LoadingSpinner>().First().IsPresent);
        }

        private class TestOverlayView : OverlayView<DummyData>
        {
            public IEnumerable<Drawable> InternalContents => InternalChildren;

            public new void PerformFetch() => base.PerformFetch();

            public TestOverlayView()
            {
                Child = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 400,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Blue
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = OsuFont.GetFont(Typeface.Torus, 32),
                            Text = "View content"
                        },
                    }
                };
            }

            protected override APIRequest<DummyData> CreateRequest() => new DummyAPIRequest();

            protected override void OnSuccess(DummyData response)
            {
            }

            private class DummyAPIRequest : APIRequest<DummyData>
            {
                protected override string Target => "";
            }
        }

        //ReSharper disable once ClassNeverInstantiated.Local
        private class DummyData
        {
        }
    }
}
