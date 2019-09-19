// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Menu;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneHoldToConfirmOverlay : OsuTestScene
    {
        protected override double TimePerAction => 100; // required for the early exit test, since hold-to-confirm delay is 200ms

        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(ExitConfirmOverlay),
            typeof(HoldToConfirmContainer),
        };

        public TestSceneHoldToConfirmOverlay()
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

            var overlay = new TestHoldToConfirmOverlay
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

        private class TestHoldToConfirmOverlay : ExitConfirmOverlay
        {
            public void Begin() => BeginConfirm();
        }
    }
}
