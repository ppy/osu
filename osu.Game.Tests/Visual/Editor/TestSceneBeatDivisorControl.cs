// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Editor
{
    public class TestSceneBeatDivisorControl : ManualInputManagerTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[] { typeof(BindableBeatDivisor) };
        private BeatDivisorControl beatDivisorControl;
        private BindableBeatDivisor bindableBeatDivisor;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Child = beatDivisorControl = new BeatDivisorControl(bindableBeatDivisor = new BindableBeatDivisor(16))
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(90, 90)
            };
        });

        [Test]
        public void TestBindableBeatDivisor()
        {
            AddRepeatStep("Move previous", () => bindableBeatDivisor.Previous(), 4);
            AddAssert("Position at 4", () => bindableBeatDivisor.Value == 4);
            AddRepeatStep("Move next", () => bindableBeatDivisor.Next(), 3);
            AddAssert("Position at 12", () => bindableBeatDivisor.Value == 12);
        }

        [Test]
        public void TestMouseInput()
        {
            AddStep("Move to marker", () =>
            {
                InputManager.MoveMouseTo(beatDivisorControl, new Vector2(38, -18));
                InputManager.PressButton(MouseButton.Left);
            });
            AddStep("Mote to divisor 8", () =>
            {
                InputManager.MoveMouseTo(beatDivisorControl, new Vector2(0, -18));
                InputManager.ReleaseButton(MouseButton.Left);
            });
            AddAssert("Position at 8", () => bindableBeatDivisor.Value == 8);
            AddStep("Prepare to move marker", () => { InputManager.PressButton(MouseButton.Left); });
            AddStep("Trigger marker jump", () =>
            {
                InputManager.MoveMouseTo(beatDivisorControl, new Vector2(30, -18));
            });
            AddStep("Move to divisor ~10", () =>
            {
                InputManager.MoveMouseTo(beatDivisorControl, new Vector2(10, -18));
                InputManager.ReleaseButton(MouseButton.Left);
            });
            AddAssert("Position clamped to 8", () => bindableBeatDivisor.Value == 8);
        }
    }
}
