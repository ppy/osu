// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Menu;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneHoldToExitGameOverlay : OsuTestScene
    {
        protected override double TimePerAction => 100; // required for the early exit test, since hold-to-confirm delay is 200ms

        public TestSceneHoldToExitGameOverlay()
        {
            bool fired = false;

            var firedText = new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Text = "Fired!",
                Font = OsuFont.GetFont(size: 50),
                Alpha = 0,
            };

            var overlay = new TestHoldToExitGameOverlay
            {
                Action = () =>
                {
                    fired = true;
                    firedText.FadeTo(1).Then().FadeOut(1000);
                }
            };

            Children = new Drawable[]
            {
                overlay,
                firedText
            };

            AddStep("start confirming", () => overlay.Begin());
            AddStep("abort confirming", () => overlay.Abort());

            AddAssert("ensure not fired internally", () => !overlay.Fired);
            AddAssert("ensure aborted", () => !fired);

            AddStep("start confirming", () => overlay.Begin());

            AddUntilStep("wait until confirmed", () => fired);
            AddAssert("ensure fired internally", () => overlay.Fired);

            AddStep("abort after fire", () => overlay.Abort());
            AddAssert("ensure not fired internally", () => !overlay.Fired);
            AddStep("start confirming", () => overlay.Begin());
            AddUntilStep("wait until fired again", () => overlay.Fired);
        }

        private partial class TestHoldToExitGameOverlay : HoldToExitGameOverlay
        {
            public void Begin() => BeginConfirm();
        }
    }
}
