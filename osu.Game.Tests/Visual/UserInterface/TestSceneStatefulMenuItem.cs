// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneStatefulMenuItem : ManualInputManagerTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(OsuMenu),
            typeof(StatefulMenuItem),
            typeof(TernaryStateMenuItem),
            typeof(DrawableStatefulMenuItem),
        };

        [Test]
        public void TestTernaryMenuItem()
        {
            OsuMenu menu = null;

            Bindable<TernaryState> state = new Bindable<TernaryState>(TernaryState.Indeterminate);

            AddStep("create menu", () =>
            {
                state.Value = TernaryState.Indeterminate;

                Child = menu = new OsuMenu(Direction.Vertical, true)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Items = new[]
                    {
                        new TernaryStateMenuItem("First"),
                        new TernaryStateMenuItem("Second") { State = { BindTarget = state } },
                        new TernaryStateMenuItem("Third") { State = { Value = TernaryState.True } },
                    }
                };
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

            AddStep("change state via bindable", () => state.Value = TernaryState.True);

            void click() =>
                AddStep("click", () =>
                {
                    InputManager.MoveMouseTo(menu.ScreenSpaceDrawQuad.Centre);
                    InputManager.Click(MouseButton.Left);
                });

            void checkState(TernaryState expected)
                => AddAssert($"state is {expected}", () => state.Value == expected);
        }

        [Test]
        public void TestCustomState()
        {
            AddStep("create menu", () =>
            {
                Child = new OsuMenu(Direction.Vertical, true)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Items = new[]
                    {
                        new TestMenuItem("First", MenuItemType.Standard, getNextState),
                        new TestMenuItem("Second", MenuItemType.Standard, getNextState) { State = { Value = TestStates.State2 } },
                        new TestMenuItem("Third", MenuItemType.Standard, getNextState) { State = { Value = TestStates.State3 } },
                    }
                };
            });
        }

        private TestStates getNextState(TestStates state)
        {
            switch (state)
            {
                case TestStates.State1:
                    return TestStates.State2;

                case TestStates.State2:
                    return TestStates.State3;

                case TestStates.State3:
                    return TestStates.State1;
            }

            return TestStates.State1;
        }

        private class TestMenuItem : StatefulMenuItem<TestStates>
        {
            public TestMenuItem(string text, MenuItemType type, Func<TestStates, TestStates> changeStateFunc)
                : base(text, changeStateFunc, type)
            {
            }

            public override IconUsage? GetIconForState(TestStates state)
            {
                switch (state)
                {
                    case TestStates.State1:
                        return FontAwesome.Solid.DiceOne;

                    case TestStates.State2:
                        return FontAwesome.Solid.DiceTwo;

                    case TestStates.State3:
                        return FontAwesome.Solid.DiceThree;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(state), state, null);
                }
            }
        }

        private enum TestStates
        {
            State1,
            State2,
            State3
        }
    }
}
