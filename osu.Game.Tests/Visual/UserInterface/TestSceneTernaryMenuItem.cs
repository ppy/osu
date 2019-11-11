// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneTernaryMenuItem : ManualInputManagerTestScene
    {
        private readonly OsuMenu menu;

        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(OsuMenu),
            typeof(ThreeStateMenuItem),
            typeof(DrawableStatefulMenuItem)
        };

        private readonly Bindable<TernaryState> state = new Bindable<TernaryState>(TernaryState.Indeterminate);

        public TestSceneTernaryMenuItem()
        {
            Add(menu = new OsuMenu(Direction.Vertical, true)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Items = new[]
                {
                    new ThreeStateMenuItem("First"),
                    new ThreeStateMenuItem("Second") { State = { BindTarget = state } },
                    new ThreeStateMenuItem("Third") { State = { Value = TernaryState.True } },
                }
            });

            checkState(TernaryState.Indeterminate);

            click();
            checkState(TernaryState.True);

            click();
            checkState(TernaryState.False);

            click();
            checkState(TernaryState.True);

            click();
            checkState(TernaryState.False);
        }

        private void click() =>
            AddStep("click", () =>
            {
                InputManager.MoveMouseTo(menu.ScreenSpaceDrawQuad.Centre);
                InputManager.Click(MouseButton.Left);
            });

        private void checkState(TernaryState expected)
            => AddAssert($"state is {expected}", () => state.Value == expected);
    }
}
