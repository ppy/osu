// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD.ClicksPerSecond;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneClicksPerSecondCalculator : OsuTestScene
    {
        private ClicksPerSecondController controller = null!;

        private TestGameplayClock manualGameplayClock = null!;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create components", () =>
            {
                manualGameplayClock = new TestGameplayClock();

                Child = new DependencyProvidingContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    CachedDependencies = new (Type, object)[] { (typeof(IGameplayClock), manualGameplayClock) },
                    Children = new Drawable[]
                    {
                        controller = new ClicksPerSecondController(),
                        new DependencyProvidingContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            CachedDependencies = new (Type, object)[] { (typeof(ClicksPerSecondController), controller) },
                            Child = new ClicksPerSecondCounter
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Scale = new Vector2(5),
                            }
                        }
                    },
                };
            });
        }

        [Test]
        public void TestBasicConsistency()
        {
            seek(1000);
            AddStep("add inputs in past", () => addInputs(new double[] { 0, 100, 200, 300, 400, 500, 600, 700, 800, 900 }));
            checkClicksPerSecondValue(10);
        }

        [Test]
        public void TestRateAdjustConsistency()
        {
            seek(1000);
            AddStep("add inputs in past", () => addInputs(new double[] { 0, 100, 200, 300, 400, 500, 600, 700, 800, 900 }));
            checkClicksPerSecondValue(10);
            AddStep("set rate 0.5x", () => manualGameplayClock.TrueGameplayRate = 0.5);
            checkClicksPerSecondValue(5);
        }

        [Test]
        public void TestInputsDiscardedOnRewind()
        {
            seek(1000);
            AddStep("add inputs in past", () => addInputs(new double[] { 0, 100, 200, 300, 400, 500, 600, 700, 800, 900 }));
            checkClicksPerSecondValue(10);
            seek(500);
            checkClicksPerSecondValue(6);
            seek(1000);
            checkClicksPerSecondValue(6);
        }

        private void checkClicksPerSecondValue(int i) => AddAssert("clicks/s is correct", () => controller.Value, () => Is.EqualTo(i));

        private void seekClockImmediately(double time) => manualGameplayClock.CurrentTime = time;

        private void seek(double time) => AddStep($"Seek to {time}ms", () => seekClockImmediately(time));

        private void addInputs(IEnumerable<double> inputs)
        {
            double baseTime = manualGameplayClock.CurrentTime;

            foreach (double timestamp in inputs)
            {
                seekClockImmediately(timestamp);
                controller.AddInputTimestamp();
            }

            seekClockImmediately(baseTime);
        }

        private class TestGameplayClock : IGameplayClock
        {
            public double CurrentTime { get; set; }

            public double Rate => 1;

            public bool IsRunning => true;

            public double TrueGameplayRate { set => adjustableAudioComponent.Tempo.Value = value; }

            private readonly AudioAdjustments adjustableAudioComponent = new AudioAdjustments();

            public void ProcessFrame()
            {
            }

            public double ElapsedFrameTime => throw new NotImplementedException();
            public double FramesPerSecond => throw new NotImplementedException();
            public FrameTimeInfo TimeInfo => throw new NotImplementedException();
            public double StartTime => throw new NotImplementedException();

            public IAdjustableAudioComponent AdjustmentsFromMods => adjustableAudioComponent;

            public IEnumerable<double> NonGameplayAdjustments => throw new NotImplementedException();
            public IBindable<bool> IsPaused => throw new NotImplementedException();
            public bool IsRewinding => false;
        }
    }
}
