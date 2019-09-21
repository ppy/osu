// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing.Input;
using osu.Game.Rulesets.Osu.UI.Cursor;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public class TestSceneGameplayCursor : SkinnableTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(OsuCursorContainer),
            typeof(CursorTrail)
        };

        [BackgroundDependencyLoader]
        private void load()
        {
            SetContents(() => new MovingCursorInputManager
            {
                Child = new ClickingCursorContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                }
            });
        }

        private class ClickingCursorContainer : OsuCursorContainer
        {
            protected override void Update()
            {
                base.Update();

                double currentTime = Time.Current;

                if (((int)(currentTime / 1000)) % 2 == 0)
                    OnPressed(OsuAction.LeftButton);
                else
                    OnReleased(OsuAction.LeftButton);
            }
        }

        private class MovingCursorInputManager : ManualInputManager
        {
            public MovingCursorInputManager()
            {
                UseParentInput = false;
            }

            protected override void Update()
            {
                base.Update();

                const double spin_duration = 5000;
                double currentTime = Time.Current;

                double angle = (currentTime % spin_duration) / spin_duration * 2 * Math.PI;
                Vector2 rPos = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));

                MoveMouseTo(ToScreenSpace(DrawSize / 2 + DrawSize / 3 * rPos));
            }
        }
    }
}
