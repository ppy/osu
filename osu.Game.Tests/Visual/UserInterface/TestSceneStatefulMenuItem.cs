// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneStatefulMenuItem : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(OsuMenu),
            typeof(StatefulMenuItem),
            typeof(DrawableStatefulMenuItem)
        };

        public TestSceneStatefulMenuItem()
        {
            Add(new OsuMenu(Direction.Vertical, true)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Items = new[]
                {
                    new TestMenuItem("First", MenuItemType.Standard, getNextState),
                    new TestMenuItem("Second", MenuItemType.Standard, getNextState) { State = { Value = TestStates.State2 } },
                    new TestMenuItem("Third", MenuItemType.Standard, getNextState) { State = { Value = TestStates.State3 } },
                }
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
