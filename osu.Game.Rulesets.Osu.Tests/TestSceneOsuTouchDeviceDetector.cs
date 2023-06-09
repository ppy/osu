// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Input;
using osu.Framework.Testing;
using osu.Game.Rulesets.Osu.UI;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public partial class TestSceneOsuTouchDeviceDetector : OsuTouchableTestScene
    {
        private OsuTouchDeviceDetector osuTouchDeviceDetector => OsuInputManager.ChildrenOfType<OsuTouchDeviceDetector>().First();

        public void TestTouchDevice(Action<Vector2> positionHandler)
        {
            int i = 0;

            AddRepeatStep("Touch some", () =>
            {
                var source = i % 2 == 0 ? TouchSource.Touch1 : TouchSource.Touch2;

                Vector2 position = GetSanePositionForSource(source);
                positionHandler(position);

                i++;
            }, 11);

            AddAssert("Is touch device", () => osuTouchDeviceDetector.DetectedTouchDevice);
            AddAssert("Flagged 11 touch inputs", () => osuTouchDeviceDetector.FlaggedTouchesCount == 10);
        }

        [Test]
        public void TestTouchDeviceDetectionWithTouchInput()
        {
            TestTouchDevice(pos =>
            {
                var touch = new Touch(TouchSource.Touch1, pos);

                InputManager.BeginTouch(touch);
                InputManager.EndTouch(touch);
            });

            BeginTouch(TouchSource.Touch1);
            BeginTouch(TouchSource.Touch2);
            BeginTouch(TouchSource.Touch3);

            AddAssert("Last touch input was not flagged", () => osuTouchDeviceDetector.FlaggedTouchesCount == 12);
        }

        [Test]
        public void TestTouchDeviceDetectionWithMouseInput()
        {
            TestTouchDevice(pos =>
                {
                    InputManager.MoveMouseTo(pos);
                    InputManager.Click(MouseButton.Left);
                }
            );
        }
    }
}
