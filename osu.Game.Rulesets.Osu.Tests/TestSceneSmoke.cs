// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Framework.Input.States;
using osu.Framework.Testing.Input;
using osu.Game.Rulesets.Osu.UI;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestSceneSmoke : OsuSkinnableTestScene
    {
        [Test]
        public void TestSmoking()
        {
            AddStep("Create smoke", () =>
            {
                SetContents(_ =>
                {
                    return new SmokingInputManager
                    {
                        RelativeSizeAxes = Axes.Both,
                        Size = new Vector2(0.95f),
                        Child = new TestSmokeContainer { RelativeSizeAxes = Axes.Both },
                    };
                });
            });
        }

        private const double spin_duration = 5_000;
        private const float spin_angle = 4 * MathF.PI;

        private class SmokingInputManager : ManualInputManager
        {
            private double? startTime;

            public SmokingInputManager()
            {
                UseParentInput = false;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                MoveMouseTo(ToScreenSpace(DrawSize / 2));
            }

            protected override void Update()
            {
                base.Update();

                startTime ??= Time.Current;

                float fraction = (float)((Time.Current - startTime) / spin_duration);

                float angle = fraction * spin_angle;
                float radius = fraction * Math.Min(DrawSize.X, DrawSize.Y) / 2;

                Vector2 pos = radius * new Vector2(MathF.Cos(angle), MathF.Sin(angle)) + DrawSize / 2;
                MoveMouseTo(ToScreenSpace(pos));
            }
        }

        private class TestSmokeContainer : SmokeContainer
        {
            private double? startTime;
            private bool isPressing;
            private bool isFinished;

            protected override void Update()
            {
                base.Update();

                startTime ??= Time.Current;

                if (!isPressing && !isFinished && Time.Current > startTime + 0.1)
                {
                    OnPressed(new KeyBindingPressEvent<OsuAction>(new InputState(), OsuAction.Smoke));
                    isPressing = true;
                    isFinished = false;
                }

                if (isPressing && Time.Current > startTime + spin_duration)
                {
                    OnReleased(new KeyBindingReleaseEvent<OsuAction>(new InputState(), OsuAction.Smoke));
                    isPressing = false;
                    isFinished = true;
                }
            }
        }
    }
}
