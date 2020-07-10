// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing.Input;
using osu.Framework.Utils;
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

        private Drawable background;

        public TestSceneGameplayCursor()
        {
            gameplayBeatmap = new GameplayBeatmap(CreateBeatmap(new OsuRuleset().RulesetInfo));

            AddStep("change background colour", () =>
            {
                background?.Expire();

                Add(background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = float.MaxValue,
                    Colour = new Colour4(RNG.NextSingle(), RNG.NextSingle(), RNG.NextSingle(), 1)
                });
            });

            AddSliderStep("circle size", 0f, 10f, 0f, val =>
            {
                config.Set(OsuSetting.AutoCursorSize, true);
                gameplayBeatmap.BeatmapInfo.BaseDifficulty.CircleSize = val;
                Scheduler.AddOnce(recreate);
            });

            AddStep("test cursor container", recreate);

            void recreate() => SetContents(() => new OsuInputManager(new OsuRuleset().RulesetInfo) { Child = new OsuCursorContainer() });
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
            private bool pressed;

            public bool Pressed
            {
                set
                {
                    if (value == pressed)
                        return;

                    pressed = value;
                    if (value)
                        OnPressed(OsuAction.LeftButton);
                    else
                        OnReleased(OsuAction.LeftButton);
                }
            }

            protected override void Update()
            {
                base.Update();
                Pressed = ((int)(Time.Current / 1000)) % 2 == 0;
            }
        }

        private class MovingCursorInputManager : ManualInputManager
        {
            public MovingCursorInputManager()
            {
                UseParentInput = false;
                ShowVisualCursorGuide = false;
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
