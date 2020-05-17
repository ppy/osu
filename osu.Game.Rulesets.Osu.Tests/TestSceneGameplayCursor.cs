// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing.Input;
using osu.Game.Configuration;
using osu.Game.Rulesets.Osu.UI.Cursor;
using osu.Game.Screens.Play;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public class TestSceneGameplayCursor : OsuSkinnableTestScene
    {
        [Cached]
        private GameplayBeatmap gameplayBeatmap;

        private ClickingCursorContainer lastContainer;

        [Resolved]
        private OsuConfigManager config { get; set; }

        public TestSceneGameplayCursor()
        {
            gameplayBeatmap = new GameplayBeatmap(CreateBeatmap(new OsuRuleset().RulesetInfo));
        }

        [TestCase(1, 1)]
        [TestCase(5, 1)]
        [TestCase(10, 1)]
        [TestCase(1, 1.5f)]
        [TestCase(5, 1.5f)]
        [TestCase(10, 1.5f)]
        public void TestSizing(int circleSize, float userScale)
        {
            AddStep($"set user scale to {userScale}", () => config.Set(OsuSetting.GameplayCursorSize, userScale));
            AddStep($"adjust cs to {circleSize}", () => gameplayBeatmap.BeatmapInfo.BaseDifficulty.CircleSize = circleSize);
            AddStep("turn on autosizing", () => config.Set(OsuSetting.AutoCursorSize, true));

            AddStep("load content", loadContent);

            AddUntilStep("cursor size correct", () => lastContainer.ActiveCursor.Scale.X == OsuCursorContainer.GetScaleForCircleSize(circleSize) * userScale);

            AddStep("set user scale to 1", () => config.Set(OsuSetting.GameplayCursorSize, 1f));
            AddUntilStep("cursor size correct", () => lastContainer.ActiveCursor.Scale.X == OsuCursorContainer.GetScaleForCircleSize(circleSize));

            AddStep("turn off autosizing", () => config.Set(OsuSetting.AutoCursorSize, false));
            AddUntilStep("cursor size correct", () => lastContainer.ActiveCursor.Scale.X == 1);

            AddStep($"set user scale to {userScale}", () => config.Set(OsuSetting.GameplayCursorSize, userScale));
            AddUntilStep("cursor size correct", () => lastContainer.ActiveCursor.Scale.X == userScale);
        }

        private void loadContent()
        {
            SetContents(() => new MovingCursorInputManager
            {
                Child = lastContainer = new ClickingCursorContainer
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
