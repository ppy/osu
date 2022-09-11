// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using Humanizer;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Screens.Play;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestSceneTouchInput : TestSceneOsuPlayer
    {
        private OsuInputManager osuInputManager => Player.DrawableRuleset.ChildrenOfType<OsuInputManager>().First();

        private OsuTouchInputMapper touchInputMapper => osuInputManager.ChildrenOfType<OsuTouchInputMapper>().First();

        private OsuActionKeyCounter leftKeyCounter = null!;

        private OsuActionKeyCounter rightKeyCounter = null!;

        private Vector2 touchPosition => osuInputManager.ScreenSpaceDrawQuad.Centre;

        private void touch(TouchSource source) => InputManager.BeginTouch(new Touch(source, touchPosition));

        private void release(TouchSource source) => InputManager.EndTouch(new Touch(source, touchPosition));

        [SetUpSteps]
        public void Setup()
        {
            AddStep("Release touches", () =>
            {
                foreach (TouchSource source in Enum.GetValues(typeof(TouchSource)))
                    release(source);
            });
            AddStep("Create key counter", () => osuInputManager.Add(new Container
            {
                Children = new Drawable[]
                {
                    leftKeyCounter = new OsuActionKeyCounter(OsuAction.LeftButton),
                    rightKeyCounter =  new OsuActionKeyCounter(OsuAction.RightButton) { Margin = new MarginPadding { Left = 150 } }
                },
                Position = osuInputManager.ToLocalSpace(ScreenSpaceDrawQuad.Centre)
            }));
        }

        private string getTouchString(TouchSource source) => (source - TouchSource.Touch1 + 1).ToOrdinalWords();

        private void addTouchWithFingerStep(TouchSource source) => AddStep($"Touch with {getTouchString(source)} finger", () => touch(source));

        private void addFirstFingerTouchStep() => addTouchWithFingerStep(TouchSource.Touch1);

        private void assertTapTouch(TouchSource source) => AddAssert($"The {getTouchString(source)} touch is a tap touch", () => touchInputMapper.IsTapTouch(source));

        private void expectTapTouchesAmount(int expect) => AddAssert($"Has {expect} tap touches active", () => touchInputMapper.ActiveTapTouches.Count == expect);

        private void assertAllowingTouchInput() => AddAssert("Allowing other touch input", () => touchInputMapper.AllowingOtherTouch);

        private string getExpectKeyCounterText(string name, int expect) => $"The {name} key was pressed {expect} times";

        private bool getExpectKeyCounterCondition(KeyCounter counter, int expect) => counter.CountPresses == expect;

        private void expectLeftKeyCounterPressedTimes(int expect) => AddAssert(getExpectKeyCounterText("left", expect), () => getExpectKeyCounterCondition(leftKeyCounter, expect));

        private void expectRightKeyCounterPressedTimes(int expect) => AddAssert(getExpectKeyCounterText("right", expect), () => getExpectKeyCounterCondition(rightKeyCounter, expect));

        private void steppedTouchWithTwoFingers()
        {
            addFirstFingerTouchStep();
            addTouchWithFingerStep(TouchSource.Touch2);
        }

        private void steppedTouchWithThreeFingers()
        {
            steppedTouchWithTwoFingers();
            addTouchWithFingerStep(TouchSource.Touch3);
        }

        [Test]
        public void TestOneFingerInput()
        {
            addFirstFingerTouchStep();

            assertAllowingTouchInput();
            expectLeftKeyCounterPressedTimes(1);
            AddAssert("The touch is a cursor touch", () => touchInputMapper.IsCursorTouch(TouchSource.Touch1));
        }

        [Test]
        public void TestTwoFingersInput()
        {
            steppedTouchWithTwoFingers();

            assertTapTouch(TouchSource.Touch2);
            expectTapTouchesAmount(1);
            assertAllowingTouchInput();
            expectLeftKeyCounterPressedTimes(1);
            expectRightKeyCounterPressedTimes(1);
        }

        [Test]
        public void TestThreeFingersInput()
        {
            steppedTouchWithThreeFingers();

            expectTapTouchesAmount(2);
            assertTapTouch(TouchSource.Touch3);
            expectRightKeyCounterPressedTimes(1);
            expectLeftKeyCounterPressedTimes(2);
            AddAssert("Tap only key mapping", () => touchInputMapper.TapOnlyMapping && touchInputMapper.EnteredTapOnlyMapping);
        }

        [Test]
        public void TestInvalidFingerInput()
        {
            steppedTouchWithThreeFingers();
            addTouchWithFingerStep(TouchSource.Touch4);

            expectTapTouchesAmount(2);
            expectLeftKeyCounterPressedTimes(2);
            expectRightKeyCounterPressedTimes(1);
            AddAssert("Touch input is blocked", () => !touchInputMapper.AllowingOtherTouch);
        }

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new Beatmap { HitObjects = new List<HitObject> { new HitCircle { StartTime = 99999 } } };

        public class OsuActionKeyCounter : KeyCounter, IKeyBindingHandler<OsuAction>
        {
            public OsuAction Action { get; }

            public OsuActionKeyCounter(OsuAction action)
                : base(action.ToString())
            {
                Action = action;
            }

            public bool OnPressed(KeyBindingPressEvent<OsuAction> e)
            {
                if (e.Action == Action)
                {
                    IsLit = true;
                    Increment();
                }

                return false;
            }

            public void OnReleased(KeyBindingReleaseEvent<OsuAction> e)
            {
                if (e.Action == Action) IsLit = false;
            }
        }
    }
}
