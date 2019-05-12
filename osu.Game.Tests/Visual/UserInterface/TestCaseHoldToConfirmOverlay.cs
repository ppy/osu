// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Menu;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestCaseHoldToConfirmOverlay : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[] { typeof(ExitConfirmOverlay) };

        public TestCaseHoldToConfirmOverlay()
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

            AddAssert("ensure aborted", () => !fired);

            AddStep("start confirming", () => overlay.Begin());

            AddUntilStep("wait until confirmed", () => fired);
        }

        private class TestHoldToConfirmOverlay : ExitConfirmOverlay
        {
            protected override bool AllowMultipleFires => true;

            public void Begin() => BeginConfirm();
            public void Abort() => AbortConfirm();
        }
    }
}
