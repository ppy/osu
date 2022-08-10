// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD.KPSCounter;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneKeysPerSecond : OsuTestScene
    {
        private DependencyProvidingContainer? dependencyContainer;
        private MockFrameStableClock? mainClock;
        private KeysPerSecondCalculator? calculator;
        private ManualInputListener? listener;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create components", () =>
            {
                var ruleset = CreateRuleset();

                Debug.Assert(ruleset != null);

                Children = new Drawable[]
                {
                    dependencyContainer = new DependencyProvidingContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        CachedDependencies = new (Type, object)[]
                        {
                            (typeof(GameplayClock), mainClock = new MockFrameStableClock(new MockFrameBasedClock())),
                            (typeof(DrawableRuleset), new MockDrawableRuleset(ruleset, mainClock))
                        }
                    },
                };
            });
        }

        private void createCalculator()
        {
            AddStep("create calculator", () =>
            {
                dependencyContainer!.Children = new Drawable[]
                {
                    calculator = new KeysPerSecondCalculator
                    {
                        Listener = listener = new ManualInputListener()
                    },
                    new DependencyProvidingContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        CachedDependencies = new (Type, object)[] { (typeof(KeysPerSecondCalculator), calculator) },
                        Child = new KeysPerSecondCounter // For visual debugging, has no real purpose in the tests
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Scale = new Vector2(5),
                        }
                    }
                };
            });
        }

        [Test]
        public void TestBasicConsistency()
        {
            createCalculator();

            AddStep("Create gradually increasing KPS inputs", () =>
            {
                addInputs(generateGraduallyIncreasingKps());
            });

            for (int i = 0; i < 10; i++)
            {
                seek(i * 10000);
                advanceForwards(2);
                int kps = i + 1;
                AddAssert($"{kps} KPS", () => calculator!.Value == kps);
            }
        }

        [Test]
        public void TestRateAdjustConsistency()
        {
            createCalculator();

            AddStep("Create consistent KPS inputs", () => addInputs(generateConsistentKps(10)));

            advanceForwards(2);

            for (double i = 1; i <= 2; i += 0.25)
            {
                changeRate(i);
                double rate = i;
                AddAssert($"KPS approx. = {i}", () => MathHelper.ApproximatelyEquivalent(calculator!.Value, 10 * rate, 0.5));
            }

            for (double i = 1; i >= 0.5; i -= 0.25)
            {
                changeRate(i);
                double rate = i;
                AddAssert($"KPS approx. = {i}", () => MathHelper.ApproximatelyEquivalent(calculator!.Value, 10 * rate, 0.5));
            }
        }

        [Test]
        public void TestInputsDiscardedOnRewind()
        {
            createCalculator();

            AddStep("Create consistent KPS inputs", () => addInputs(generateConsistentKps(10)));
            seek(1000);

            AddAssert("KPS = 10", () => calculator!.Value == 10);

            AddStep("Create delayed inputs", () => addInputs(generateConsistentKps(10, 50)));
            seek(1000);
            AddAssert("KPS didn't changed", () => calculator!.Value == 10);
        }

        private void seek(double time) => AddStep($"Seek main clock to {time}ms", () => mainClock?.Seek(time));

        private void changeRate(double rate) => AddStep($"Change rate to x{rate}", () =>
            (mainClock?.UnderlyingClock as MockFrameBasedClock)!.Rate = rate);

        private void advanceForwards(int frames = 1) => AddStep($"Advance main clock {frames} frame(s) forward.", () =>
        {
            if (mainClock == null) return;

            MockFrameBasedClock underlyingClock = (MockFrameBasedClock)mainClock.UnderlyingClock;
            underlyingClock.Backwards = false;

            for (int i = 0; i < frames; i++)
            {
                underlyingClock.ProcessFrame();
            }
        });

        private void addInputs(IEnumerable<double> inputs)
        {
            Debug.Assert(mainClock != null && listener != null);
            if (!inputs.Any()) return;

            double baseTime = mainClock.CurrentTime;

            foreach (double timestamp in inputs)
            {
                mainClock.Seek(timestamp);
                listener.AddInput();
            }

            mainClock.Seek(baseTime);
        }

        private IEnumerable<double> generateGraduallyIncreasingKps()
        {
            IEnumerable<double>? final = null;

            for (int i = 1; i <= 10; i++)
            {
                var currentKps = generateConsistentKps(i, (i - 1) * 10000);

                if (i == 1)
                {
                    final = currentKps;
                    continue;
                }

                final = final!.Concat(currentKps);
            }

            return final!;
        }

        private IEnumerable<double> generateConsistentKps(double kps, double start = 0, double duration = 10)
        {
            double end = start + 1000 * duration;

            for (; start < end; start += 1000 / kps)
            {
                yield return start;
            }
        }

        protected override Ruleset CreateRuleset() => new ManiaRuleset();

        #region Mock classes

        private class ManualInputListener : KeysPerSecondCalculator.InputListener
        {
            public override event Action? OnNewInput;

            public void AddInput() => OnNewInput?.Invoke();
        }

        private class MockFrameBasedClock : ManualClock, IFrameBasedClock
        {
            public const double FRAME_INTERVAL = 1000;
            public bool Backwards;

            public MockFrameBasedClock()
            {
                Rate = 1;
                IsRunning = true;
            }

            public void ProcessFrame()
            {
                CurrentTime += FRAME_INTERVAL * Rate * (Backwards ? -1 : 1);
                TimeInfo = new FrameTimeInfo
                {
                    Current = CurrentTime,
                    Elapsed = FRAME_INTERVAL * Rate * (Backwards ? -1 : 1)
                };
            }

            public void Seek(double time)
            {
                TimeInfo = new FrameTimeInfo
                {
                    Elapsed = time - CurrentTime,
                    Current = CurrentTime = time
                };
            }

            public double ElapsedFrameTime => TimeInfo.Elapsed;
            public double FramesPerSecond => 1 / FRAME_INTERVAL;
            public FrameTimeInfo TimeInfo { get; private set; }
        }

        private class MockFrameStableClock : GameplayClock, IFrameStableClock
        {
            public MockFrameStableClock(MockFrameBasedClock underlyingClock)
                : base(underlyingClock)
            {
            }

            public void Seek(double time) => (UnderlyingClock as MockFrameBasedClock)?.Seek(time);

            public IBindable<bool> IsCatchingUp => new Bindable<bool>();
            public IBindable<bool> WaitingOnFrames => new Bindable<bool>();
        }

        private class MockDrawableRuleset : DrawableRuleset
        {
            public MockDrawableRuleset(Ruleset ruleset, IFrameStableClock clock)
                : base(ruleset)
            {
                FrameStableClock = clock;
            }

#pragma warning disable CS0067
            public override event Action<JudgementResult>? NewResult;
            public override event Action<JudgementResult>? RevertResult;
#pragma warning restore CS0067
            public override Playfield? Playfield => null;
            public override Container? Overlays => null;
            public override Container? FrameStableComponents => null;
            public override IFrameStableClock FrameStableClock { get; }

            internal override bool FrameStablePlayback { get; set; }
            public override IReadOnlyList<Mod> Mods => Array.Empty<Mod>();
            public override IEnumerable<HitObject> Objects => Array.Empty<HitObject>();
            public override double GameplayStartTime => 0;
            public override GameplayCursorContainer? Cursor => null;

            public override void SetReplayScore(Score replayScore)
            {
            }

            public override void SetRecordTarget(Score score)
            {
            }

            public override void RequestResume(Action continueResume)
            {
            }

            public override void CancelResume()
            {
            }
        }

        #endregion
    }
}
