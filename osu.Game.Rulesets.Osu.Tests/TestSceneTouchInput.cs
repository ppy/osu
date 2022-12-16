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
    [TestFixture]
    public partial class TestSceneTouchInput : TestSceneOsuPlayer
    {
        private OsuActionKeyCounter leftKeyCounter = null!;

        private OsuActionKeyCounter rightKeyCounter = null!;

        private OsuInputManager osuInputManager => Player.DrawableRuleset.ChildrenOfType<OsuInputManager>().First();

        private OsuTouchInputMapper touchInputMapper => osuInputManager.ChildrenOfType<OsuTouchInputMapper>().First();

        private Vector2 touchPosition => osuInputManager.ScreenSpaceDrawQuad.Centre;

        private void touch(TouchSource source) => InputManager.BeginTouch(new Touch(source, touchPosition));

        private void release(TouchSource source) => InputManager.EndTouch(new Touch(source, touchPosition));

        [SetUpSteps]
        public void Setup()
        {
            AddStep("Release touches", () =>
            {
                foreach (TouchSource source in osuInputManager.CurrentState.Touch.ActiveSources)
                    release(source);
            });
            AddStep("Create key counter", () => osuInputManager.Add(new Container
            {
                Children = new Drawable[]
                {
                    leftKeyCounter = new OsuActionKeyCounter(OsuAction.LeftButton),
                    rightKeyCounter = new OsuActionKeyCounter(OsuAction.RightButton) { Margin = new MarginPadding { Left = 150 } }
                },
                Position = osuInputManager.ToLocalSpace(ScreenSpaceDrawQuad.Centre)
            }));
        }

        private void expectTapTouchesAmount(int expect) => AddAssert($"Has {expect} tap touches active", () => touchInputMapper.ActiveTapTouchesCount == expect);

        private void assertAllowingTouchInput() => AddAssert("Allowing other touch input", () => touchInputMapper.AcceptingTouchInputs);

        private string getTouchString(TouchSource source) => (source - TouchSource.Touch1 + 1).ToOrdinalWords();

        private void addTouchWithFingerStep(TouchSource source) => AddStep($"Touch with {getTouchString(source)} finger", () => touch(source));

        private void addReleaseFingerStep(TouchSource source) => AddStep($"Released the {getTouchString(source)} finger", () => release(source));

        private void addFirstFingerTouchStep() => addTouchWithFingerStep(TouchSource.Touch1);

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

        private void expectKeyCountersCountingBe(int left, int right)
        {
            bool getExpectKeyCounterCondition(KeyCounter counter, int expect) => counter.CountPresses == expect;
            string getExpectKeyCounterText(string name, int expect) => $"The {name} key was pressed {expect} times";

            AddAssert(getExpectKeyCounterText("left", left), () => getExpectKeyCounterCondition(leftKeyCounter, left));
            AddAssert(getExpectKeyCounterText("right", right), () => getExpectKeyCounterCondition(rightKeyCounter, right));
        }

        private void expectPressedCurrently(OsuAction action) => AddAssert($"Is pressing {action}", () => osuInputManager.PressedActions.Contains(action));

        private void expectBothKeysPressed()
        {
            expectPressedCurrently(OsuAction.LeftButton);
            expectPressedCurrently(OsuAction.RightButton);
        }

        [Test]
        public void TestTouchSources()
        {
            AddAssert("Cursor touch is properly set", () => touchInputMapper.IsCursorTouch(TouchSource.Touch1));
            addTouchWithFingerStep(TouchSource.Touch1);
            AddAssert("All other touches are tap touches", () => Enum.GetValues(typeof(TouchSource)).Cast<TouchSource>().Skip(1).All(source => touchInputMapper.IsTapTouch(source)));
        }

        [Test]
        public void TestTouchSourcesWithDisabledCursorMovement()
        {
            AddStep("Disable cursor movement", () => osuInputManager.AllowUserCursorMovement = false);
            steppedTouchWithThreeFingers();
            expectBothKeysPressed();
            expectKeyCountersCountingBe(1, 1);
        }

        [Test]
        public void TestOneFingerInput()
        {
            addFirstFingerTouchStep();

            assertAllowingTouchInput();
            expectTapTouchesAmount(0);
            expectKeyCountersCountingBe(1, 0);
            expectPressedCurrently(OsuAction.LeftButton);
        }

        [Test]
        public void TestTwoFingersInput()
        {
            steppedTouchWithTwoFingers();

            expectTapTouchesAmount(1);
            assertAllowingTouchInput();
            expectBothKeysPressed();
            expectKeyCountersCountingBe(1, 1);
        }

        private void assertAcceptedOnlyThreeSequentialInputs()
        {
            expectBothKeysPressed();
            expectTapTouchesAmount(2);
            expectKeyCountersCountingBe(2, 1);
        }

        [Test]
        public void TestThreeFingersInput()
        {
            steppedTouchWithThreeFingers();

            assertAcceptedOnlyThreeSequentialInputs();
            AddAssert("Touch input is blocked", () => !touchInputMapper.AcceptingTouchInputs);
            AddAssert("Is on stream mode", () => touchInputMapper.IsStreamMode);
        }

        [Test]
        public void TestStreamMode()
        {
            steppedTouchWithThreeFingers();
            AddAssert("Is on stream mode", () => touchInputMapper.IsStreamMode);

            addReleaseFingerStep(TouchSource.Touch3);
            addReleaseFingerStep(TouchSource.Touch2);
            addReleaseFingerStep(TouchSource.Touch1);

            AddAssert("Is not on stream mode", () => !touchInputMapper.IsStreamMode);
        }

        [Test]
        public void TestInvalidFingerInput()
        {
            steppedTouchWithThreeFingers();
            addTouchWithFingerStep(TouchSource.Touch4);

            assertAcceptedOnlyThreeSequentialInputs();
        }

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new Beatmap { HitObjects = new List<HitObject> { new HitCircle { StartTime = 99999 } } };

        public partial class OsuActionKeyCounter : KeyCounter, IKeyBindingHandler<OsuAction>
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
