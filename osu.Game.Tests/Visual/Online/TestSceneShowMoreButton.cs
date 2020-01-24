// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Framework.Allocation;
using osu.Game.Graphics;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneShowMoreButton : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(ShowMoreButton),
        };

        public TestSceneShowMoreButton()
        {
            TestButton button = null;

            int fireCount = 0;

            Add(button = new TestButton
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Action = () =>
                {
                    fireCount++;
                    // ReSharper disable once AccessToModifiedClosure
                    // ReSharper disable once PossibleNullReferenceException
                    Scheduler.AddDelayed(() => button.IsLoading = false, 2000);
                }
            });

            AddStep("click button", () => button.Click());

            AddAssert("action fired once", () => fireCount == 1);
            AddAssert("is in loading state", () => button.IsLoading);

            AddStep("click button", () => button.Click());

            AddAssert("action not fired", () => fireCount == 1);
            AddAssert("is in loading state", () => button.IsLoading);

            AddUntilStep("wait for loaded", () => !button.IsLoading);

            AddStep("click button", () => button.Click());

            AddAssert("action fired twice", () => fireCount == 2);
            AddAssert("is in loading state", () => button.IsLoading);
        }

        private class TestButton : ShowMoreButton
        {
            [BackgroundDependencyLoader]
            private void load(OsuColour colors)
            {
                IdleColour = colors.YellowDark;
                HoverColour = colors.Yellow;
                ChevronIconColour = colors.Red;
            }
        }
    }
}
