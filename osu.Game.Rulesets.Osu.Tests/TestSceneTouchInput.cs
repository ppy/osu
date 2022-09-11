// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
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
                Children = new Drawable[] { new OsuActionKeyCounter(OsuAction.LeftButton), new OsuActionKeyCounter(OsuAction.RightButton) { Margin = new MarginPadding { Left = 150 } } },
                Position = osuInputManager.ToLocalSpace(ScreenSpaceDrawQuad.Centre)
            }));
        }

        [Test]
        public void TestOneFingerInput()
        {
            AddStep("Touch with first finger", () => touch(TouchSource.Touch1));

            AddAssert("The touch is a cursor touch", () => touchInputMapper.IsCursorTouch(TouchSource.Touch1));
            AddAssert("Allowing other touch", () => touchInputMapper.AllowingOtherTouch);
        }

        [Test]
        public void TestTwoFingersInput()
        {
            AddStep("Touch with first finger", () => touch(TouchSource.Touch1));
            AddStep("Touch with second finger", () => touch(TouchSource.Touch2));

            AddAssert("Pressed other finger key", () => osuInputManager.PressedActions.Contains(OsuAction.RightButton));
            AddAssert("The touch is a tap touch", () => touchInputMapper.IsTapTouch(TouchSource.Touch2));
            AddAssert("Check active tap touches", () => touchInputMapper.ActiveTapTouches.Count == 1);
            AddAssert("Allowing other touch", () => touchInputMapper.AllowingOtherTouch);
        }

        [Test]
        public void TestThreeFingersInput()
        {
            AddStep("Touch with first finger", () => touch(TouchSource.Touch1));
            AddStep("Touch with second finger", () => touch(TouchSource.Touch2));
            AddStep("Touch with third finger", () => touch(TouchSource.Touch3));

            AddAssert("Tap only key mapping", () => touchInputMapper.TapOnlyMapping);
            AddAssert("The third touch is a tap touch", () => touchInputMapper.IsTapTouch(TouchSource.Touch3));
            AddAssert("Check active tap touches", () => touchInputMapper.ActiveTapTouches.Count == 2);
            AddAssert("Disabled left key", () => !osuInputManager.PressedActions.Contains(OsuAction.LeftButton));

            AddStep("Release third finger", () => release(TouchSource.Touch3));
            AddStep("Touch with third finger once again", () => touch(TouchSource.Touch3));

            AddAssert("Both keys are pressed", () => osuInputManager.PressedActions.Count() == 2);
        }

        [Test]
        public void TestInvalidFingerInput()
        {
            AddStep("Touch with first finger", () => touch(TouchSource.Touch1));
            AddStep("Touch with second finger", () => touch(TouchSource.Touch2));
            AddStep("Touch with third finger", () => touch(TouchSource.Touch3));
            AddStep("Touch with an invalid finger", () => touch(TouchSource.Touch4));

            AddAssert("Touch is blocked", () => !touchInputMapper.AllowingOtherTouch);
            AddAssert("Check active tap touches", () => touchInputMapper.ActiveTapTouches.Count == 2);
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
