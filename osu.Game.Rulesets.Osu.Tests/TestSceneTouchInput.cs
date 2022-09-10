// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Input;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestSceneTouchInput : TestSceneOsuPlayer
    {
        private Vector2 circlePosition => new Vector2(200);

        private OsuInputManager osuInputManager => Player.DrawableRuleset.ChildrenOfType<OsuInputManager>().First();
        private OsuTouchInputMapper touchInputMapper => osuInputManager.TouchInputMapper;

        private void touch(TouchSource source, Vector2 position) => InputManager.BeginTouch(new Touch(source, position));

        private void release(TouchSource source, Vector2 position) => InputManager.EndTouch(new Touch(source, position));

        [Test]
        public void TestTouchInput()
        {
            // Cursor touch
            AddStep("Touch with cursor finger", () => touch(TouchSource.Touch1, Player.DrawableRuleset.Playfield.ToScreenSpace(circlePosition)));

            AddAssert("The touch is a cursor touch", () => touchInputMapper.IsCursorTouch(TouchSource.Touch1));
            AddAssert("Allowing other touch", () => touchInputMapper.AllowingOtherTouch);

            // Left button touch
            AddStep("Touch with other finger", () => touch(TouchSource.Touch2, osuInputManager.ScreenSpaceDrawQuad.Centre));

            AddAssert("Pressed other finger key", () => osuInputManager.PressedActions.Contains(OsuAction.RightButton));
            AddAssert("The touch is a tap touch", () => touchInputMapper.IsTapTouch(TouchSource.Touch2));
            AddAssert("Check active tap touches", () => touchInputMapper.ActiveTapTouches.Count == 1);
            AddAssert("Allowing other touch", () => touchInputMapper.AllowingOtherTouch);

            // Right button touch
            AddStep("Touch with another finger (Doubletapping)...", () => touch(TouchSource.Touch3, osuInputManager.ScreenSpaceDrawQuad.Centre - new Vector2(100)));

            AddAssert("The other touch is also a tap touch", () => touchInputMapper.IsTapTouch(TouchSource.Touch3));
            AddAssert("Both keys are pressed", () => osuInputManager.PressedActions.Count() == 2);
            AddAssert("Check active tap touches", () => touchInputMapper.ActiveTapTouches.Count == 2);
            AddAssert("Dragging cursor", () => touchInputMapper.DraggingCursorMode);

            // Invalid touch
            AddStep("Touch with an invalid touch", () => touch(TouchSource.Touch4, osuInputManager.ScreenSpaceDrawQuad.Centre - new Vector2(150)));

            AddAssert("Touch is blocked", () => !touchInputMapper.AllowingOtherTouch);
            AddAssert("Check active tap touches", () => touchInputMapper.ActiveTapTouches.Count == 2);

            // Cleanup
            AddStep("Release", () =>
            {
                foreach (TouchSource source in Enum.GetValues(typeof(TouchSource)))
                    release(source, osuInputManager.CurrentState.Touch.GetTouchPosition(source).GetValueOrDefault());
            });
        }

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new Beatmap
        {
            HitObjects = new List<HitObject>
            {
                new HitCircle()
                {
                    StartTime = 1000,
                    Position = circlePosition
                },
                new HitCircle()
                {
                    StartTime = 2000,
                    Position = circlePosition
                },
                new HitCircle()
                {
                    StartTime = 3000,
                    Position = circlePosition
                },
            }
        };
    }
}
