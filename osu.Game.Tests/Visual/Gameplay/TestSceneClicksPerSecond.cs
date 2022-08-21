// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD.ClicksPerSecond;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneClicksPerSecond : OsuTestScene
    {
        private DependencyProvidingContainer? dependencyContainer;
        private ClicksPerSecondCalculator? calculator;
        private ManualInputListener? listener;
        private GameplayClockContainer? gameplayClockContainer;
        private ManualClock? manualClock;
        private DrawableRuleset? drawableRuleset;
        private IFrameStableClock? frameStableClock;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create components", () =>
            {
                var ruleset = CreateRuleset();

                Debug.Assert(ruleset != null);

                Child = gameplayClockContainer = new GameplayClockContainer(manualClock = new ManualClock());
                gameplayClockContainer.AddRange(new Drawable[]
                {
                    drawableRuleset = new TestDrawableRuleset(frameStableClock = new TestFrameStableClock(manualClock)),
                    dependencyContainer = new DependencyProvidingContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        CachedDependencies = new (Type, object)[]
                        {
                            (typeof(DrawableRuleset), drawableRuleset),
                            (typeof(IGameplayClock), gameplayClockContainer)
                        }
                    }
                });
            });
        }

        [Test]
        public void TestBasicConsistency()
        {
            createCalculator();
            startClock();

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
            startClock();

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
            startClock();

            AddStep("Create consistent KPS inputs", () => addInputs(generateConsistentKps(10)));
            seek(1000);

            AddAssert("KPS = 10", () => calculator!.Value == 10);

            AddStep("Create delayed inputs", () => addInputs(generateConsistentKps(10, 50)));
            seek(1000);
            AddAssert("KPS didn't changed", () => calculator!.Value == 10);
        }

        private void seekAllClocks(double time)
        {
            gameplayClockContainer?.Seek(time);
            manualClock!.CurrentTime = time;
        }

        protected override Ruleset CreateRuleset() => new OsuRuleset();

        #region Quick steps methods

        private void createCalculator()
        {
            AddStep("create calculator", () =>
            {
                Debug.Assert(dependencyContainer?.Dependencies.Get(typeof(DrawableRuleset)) is DrawableRuleset);
                dependencyContainer!.Children = new Drawable[]
                {
                    calculator = new ClicksPerSecondCalculator(),
                    new DependencyProvidingContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        CachedDependencies = new (Type, object)[] { (typeof(ClicksPerSecondCalculator), calculator) },
                        Child = new ClicksPerSecondCounter // For visual debugging, has no real purpose in the tests
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Scale = new Vector2(5),
                        }
                    }
                };
                calculator!.Listener = listener = new ManualInputListener(calculator!);
            });
        }

        private void seek(double time) => AddStep($"Seek clocks to {time}ms", () => seekAllClocks(time));

        private void changeRate(double rate) => AddStep($"Change rate to x{rate}", () => manualClock!.Rate = rate);

        private void advanceForwards(double time) =>
            AddStep($"Advance clocks {time} seconds forward.", () =>
            {
                gameplayClockContainer!.Seek(gameplayClockContainer.CurrentTime + time * manualClock!.Rate);

                for (int i = 0; i < time; i++)
                {
                    frameStableClock?.ProcessFrame();
                }
            });

        private void startClock() => AddStep("Start clocks", () =>
        {
            gameplayClockContainer?.Start();
            manualClock!.Rate = 1;
        });

        #endregion

        #region Input generation

        private void addInputs(IEnumerable<double> inputs)
        {
            Debug.Assert(manualClock != null && listener != null && gameplayClockContainer != null);
            if (!inputs.Any()) return;

            double baseTime = gameplayClockContainer.CurrentTime;

            foreach (double timestamp in inputs)
            {
                seekAllClocks(timestamp);
                listener.AddInput();
            }

            seekAllClocks(baseTime);
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

        #endregion

        #region Test classes

        private class TestFrameStableClock : IFrameStableClock
        {
            public TestFrameStableClock(IClock source, double startTime = 0)
            {
                this.source = source;
                StartTime = startTime;

                if (source is ManualClock manualClock)
                {
                    manualClock.CurrentTime = startTime;
                }
            }

            public double CurrentTime => source.CurrentTime;
            public double Rate => source.Rate;
            public bool IsRunning => source.IsRunning;

            private IClock source;

            public void ProcessFrame()
            {
                if (source is ManualClock manualClock)
                {
                    manualClock.CurrentTime += 1000 * Rate;
                }

                TimeInfo = new FrameTimeInfo
                {
                    Elapsed = 1000 * Rate,
                    Current = CurrentTime
                };
            }

            public double ElapsedFrameTime => TimeInfo.Elapsed;
            public double FramesPerSecond => 1 / ElapsedFrameTime * 1000;
            public FrameTimeInfo TimeInfo { get; private set; }

            public double? StartTime { get; }
            public IEnumerable<double> NonGameplayAdjustments => Enumerable.Empty<double>();
            public IBindable<bool> IsCatchingUp => new Bindable<bool>();
            public IBindable<bool> WaitingOnFrames => new Bindable<bool>();
        }

        private class ManualInputListener : ClicksPerSecondCalculator.InputListener
        {
            public void AddInput() => Calculator.AddTimestamp();

            public ManualInputListener(ClicksPerSecondCalculator calculator)
                : base(calculator)
            {
            }
        }

#nullable disable

        [SuppressMessage("ReSharper", "UnassignedGetOnlyAutoProperty")]
        private class TestDrawableRuleset : DrawableRuleset
        {
            public override IEnumerable<HitObject> Objects => Enumerable.Empty<HitObject>();

            public override event Action<JudgementResult> NewResult
            {
                add => throw new InvalidOperationException($"{nameof(NewResult)} operations not supported in test context");
                remove => throw new InvalidOperationException($"{nameof(NewResult)} operations not supported in test context");
            }

            public override event Action<JudgementResult> RevertResult
            {
                add => throw new InvalidOperationException($"{nameof(RevertResult)} operations not supported in test context");
                remove => throw new InvalidOperationException($"{nameof(RevertResult)} operations not supported in test context");
            }

            public override Playfield Playfield => null;
            public override Container Overlays => null;
            public override Container FrameStableComponents => null;
            public override IFrameStableClock FrameStableClock { get; }

            internal override bool FrameStablePlayback { get; set; }
            public override IReadOnlyList<Mod> Mods => Array.Empty<Mod>();

            public override double GameplayStartTime => 0;
            public override GameplayCursorContainer Cursor => null;

            public TestDrawableRuleset()
                : base(new OsuRuleset())
            {
            }

            public TestDrawableRuleset(IFrameStableClock frameStableClock)
                : this()
            {
                FrameStableClock = frameStableClock;
            }

            public override void SetReplayScore(Score replayScore) => throw new NotImplementedException();

            public override void SetRecordTarget(Score score) => throw new NotImplementedException();

            public override void RequestResume(Action continueResume) => throw new NotImplementedException();

            public override void CancelResume() => throw new NotImplementedException();
        }

        #endregion
    }
}
