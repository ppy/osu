// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Play;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Gameplay
{
    [HeadlessTest]
    public class TestSceneMasterGameplayClockContainer : OsuTestScene
    {
        private OsuConfigManager localConfig;

        [BackgroundDependencyLoader]
        private void load()
        {
            Dependencies.Cache(localConfig = new OsuConfigManager(LocalStorage));
        }

        [Test]
        public void TestStartThenElapsedTime()
        {
            GameplayClockContainer gameplayClockContainer = null;

            AddStep("create container", () =>
            {
                var working = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo);
                working.LoadTrack();

                Add(gameplayClockContainer = new MasterGameplayClockContainer(working, 0));
            });

            AddStep("start clock", () => gameplayClockContainer.Start());
            AddUntilStep("elapsed greater than zero", () => gameplayClockContainer.GameplayClock.ElapsedFrameTime > 0);
        }

        [Test]
        public void TestElapseThenReset()
        {
            GameplayClockContainer gameplayClockContainer = null;

            AddStep("create container", () =>
            {
                var working = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo);
                working.LoadTrack();

                Add(gameplayClockContainer = new MasterGameplayClockContainer(working, 0));
            });

            AddStep("start clock", () => gameplayClockContainer.Start());
            AddUntilStep("current time greater 2000", () => gameplayClockContainer.GameplayClock.CurrentTime > 2000);

            double timeAtReset = 0;
            AddStep("reset clock", () =>
            {
                timeAtReset = gameplayClockContainer.GameplayClock.CurrentTime;
                gameplayClockContainer.Reset();
            });

            AddAssert("current time < time at reset", () => gameplayClockContainer.GameplayClock.CurrentTime < timeAtReset);
        }

        [Test]
        public void TestSeekPerformsInGameplayTime(
            [Values(1.0, 0.5, 2.0)] double clockRate,
            [Values(0.0, 200.0, -200.0)] double userOffset,
            [Values(false, true)] bool whileStopped)
        {
            ClockBackedTestWorkingBeatmap working = null;
            GameplayClockContainer gameplayClockContainer = null;

            AddStep("create container", () =>
            {
                working = new ClockBackedTestWorkingBeatmap(new OsuRuleset().RulesetInfo, new FramedClock(new ManualClock()), Audio);
                working.LoadTrack();

                Add(gameplayClockContainer = new MasterGameplayClockContainer(working, 0));

                if (whileStopped)
                    gameplayClockContainer.Stop();

                gameplayClockContainer.Reset();
            });

            AddStep($"set clock rate to {clockRate}", () => working.Track.AddAdjustment(AdjustableProperty.Frequency, new BindableDouble(clockRate)));
            AddStep($"set audio offset to {userOffset}", () => localConfig.SetValue(OsuSetting.AudioOffset, userOffset));

            AddStep("seek to 2500", () => gameplayClockContainer.Seek(2500));
            AddAssert("gameplay clock time = 2500", () => Precision.AlmostEquals(gameplayClockContainer.CurrentTime, 2500, 10f));

            AddStep("seek to 10000", () => gameplayClockContainer.Seek(10000));
            AddAssert("gameplay clock time = 10000", () => Precision.AlmostEquals(gameplayClockContainer.CurrentTime, 10000, 10f));
        }

        protected override void Dispose(bool isDisposing)
        {
            localConfig?.Dispose();
            base.Dispose(isDisposing);
        }
    }
}
