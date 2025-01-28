﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD;
using osu.Game.Skinning;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public partial class TestSceneKeyCounter : OsuManualInputManagerTestScene
    {
        [Cached]
        private readonly InputCountController controller;

        public TestSceneKeyCounter()
        {
            Children = new Drawable[]
            {
                controller = new InputCountController(),
                new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(20),
                    Children = new Drawable[]
                    {
                        new DefaultKeyCounterDisplay
                        {
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                        },
                        new DefaultKeyCounterDisplay
                        {
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            Scale = new Vector2(1, -1)
                        },
                        new ArgonKeyCounterDisplay
                        {
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                        },
                        new ArgonKeyCounterDisplay
                        {
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            Scale = new Vector2(1, -1)
                        },
                        new LegacyKeyCounterDisplay
                        {
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            Spacing = new Vector2(20),
                            Children = new Drawable[]
                            {
                                new DefaultKeyCounterDisplay
                                {
                                    Origin = Anchor.Centre,
                                    Anchor = Anchor.Centre,
                                    Rotation = -90,
                                },
                                new DefaultKeyCounterDisplay
                                {
                                    Origin = Anchor.Centre,
                                    Anchor = Anchor.Centre,
                                    Rotation = 90,
                                },
                                new ArgonKeyCounterDisplay
                                {
                                    Origin = Anchor.Centre,
                                    Anchor = Anchor.Centre,
                                    Rotation = -90,
                                },
                                new ArgonKeyCounterDisplay
                                {
                                    Origin = Anchor.Centre,
                                    Anchor = Anchor.Centre,
                                    Rotation = 90,
                                },
                                new LegacyKeyCounterDisplay
                                {
                                    Origin = Anchor.Centre,
                                    Anchor = Anchor.Centre,
                                    Rotation = 90,
                                },
                            }
                        },
                    }
                }
            };

            var inputTriggers = new InputTrigger[]
            {
                new KeyCounterKeyboardTrigger(Key.X),
                new KeyCounterKeyboardTrigger(Key.X),
                new KeyCounterMouseTrigger(MouseButton.Left),
                new KeyCounterMouseTrigger(MouseButton.Right),
            };

            AddRange(inputTriggers);
            controller.AddRange(inputTriggers);

            AddStep("Add random", () =>
            {
                Key key = (Key)((int)Key.A + RNG.Next(26));
                var trigger = new KeyCounterKeyboardTrigger(key);
                Add(trigger);
                controller.Add(trigger);
            });

            InputTrigger testTrigger = controller.Triggers.First();
            Key testKey = ((KeyCounterKeyboardTrigger)testTrigger).Key;

            addPressKeyStep();
            AddAssert($"Check {testKey} counter after keypress", () => testTrigger.ActivationCount.Value == 1);
            addPressKeyStep();
            AddAssert($"Check {testKey} counter after keypress", () => testTrigger.ActivationCount.Value == 2);
            AddStep("Disable counting", () => controller.IsCounting.Value = false);
            addPressKeyStep();
            AddAssert($"Check {testKey} count has not changed", () => testTrigger.ActivationCount.Value == 2);
            AddStep("Enable counting", () => controller.IsCounting.Value = true);
            addPressKeyStep(100);
            addPressKeyStep(1000);

            void addPressKeyStep(int repeat = 1) => AddStep($"Press {testKey} key {repeat} times", () =>
            {
                for (int i = 0; i < repeat; i++)
                    InputManager.Key(testKey);
            });
        }
    }
}
