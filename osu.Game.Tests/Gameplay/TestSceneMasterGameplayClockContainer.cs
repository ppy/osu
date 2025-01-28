// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Configuration;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Play;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Gameplay
{
    [HeadlessTest]
    public partial class TestSceneMasterGameplayClockContainer : OsuTestScene
    {
        private OsuConfigManager localConfig;

        [BackgroundDependencyLoader]
        private void load()
        {
            Dependencies.Cache(localConfig = new OsuConfigManager(LocalStorage));
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("reset audio offset", () => localConfig.SetValue(OsuSetting.AudioOffset, 0.0));
        }

        [Test]
        public void TestStartThenElapsedTime()
        {
            GameplayClockContainer gameplayClockContainer = null;

            AddStep("create container", () =>
            {
                var working = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo);
                Child = gameplayClockContainer = new MasterGameplayClockContainer(working, 0);
            });

            AddStep("start clock", () => gameplayClockContainer.Start());
            AddUntilStep("elapsed greater than zero", () => gameplayClockContainer.ElapsedFrameTime > 0);
        }

        [Test]
        public void TestElapseThenReset()
        {
            GameplayClockContainer gameplayClockContainer = null;

            AddStep("create container", () =>
            {
                var working = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo);
                Child = gameplayClockContainer = new MasterGameplayClockContainer(working, 0);
            });

            AddStep("start clock", () => gameplayClockContainer.Start());
            AddUntilStep("current time greater 2000", () => gameplayClockContainer.CurrentTime > 2000);

            double timeAtReset = 0;
            AddStep("reset clock", () =>
            {
                timeAtReset = gameplayClockContainer.CurrentTime;
                gameplayClockContainer.Reset();
            });

            AddAssert("current time < time at reset", () => gameplayClockContainer.CurrentTime < timeAtReset);
        }

        [Test]
        public void TestSeekPerformsInGameplayTime(
            [Values(1.0, 0.5, 2.0)] double clockRate,
            [Values(0.0, 200.0, -200.0)] double userOffset,
            [Values(false, true)] bool whileStopped,
            [Values(false, true)] bool setAudioOffsetBeforeConstruction)
        {
            ClockBackedTestWorkingBeatmap working = null;
            GameplayClockContainer gameplayClockContainer = null;

            // ReSharper disable once NotAccessedVariable
            BindableDouble trackAdjustment = null; // keeping a reference for track adjustment

            if (setAudioOffsetBeforeConstruction)
                AddStep($"preset audio offset to {userOffset}", () => localConfig.SetValue(OsuSetting.AudioOffset, userOffset));

            AddStep("create container", () =>
            {
                working = new ClockBackedTestWorkingBeatmap(new OsuRuleset().RulesetInfo, new FramedClock(new ManualClock()), Audio);
                Child = gameplayClockContainer = new MasterGameplayClockContainer(working, 0);

                gameplayClockContainer.Reset(startClock: !whileStopped);
            });

            AddStep($"set clock rate to {clockRate}", () => working.Track.AddAdjustment(AdjustableProperty.Frequency, trackAdjustment = new BindableDouble(clockRate)));

            if (!setAudioOffsetBeforeConstruction)
                AddStep($"set audio offset to {userOffset}", () => localConfig.SetValue(OsuSetting.AudioOffset, userOffset));

            AddStep("seek to 2500", () => gameplayClockContainer.Seek(2500));
            AddAssert("gameplay clock time = 2500", () => gameplayClockContainer.CurrentTime, () => Is.EqualTo(2500).Within(10f));

            AddStep("seek to 10000", () => gameplayClockContainer.Seek(10000));
            AddAssert("gameplay clock time = 10000", () => gameplayClockContainer.CurrentTime, () => Is.EqualTo(10000).Within(10f));
        }

        [Test]
        public void TestStopUsingBeatmapClock()
        {
            ClockBackedTestWorkingBeatmap working = null;
            MasterGameplayClockContainer gameplayClockContainer = null;
            BindableDouble frequencyAdjustment = new BindableDouble(2);

            AddStep("create container", () =>
            {
                working = new ClockBackedTestWorkingBeatmap(new OsuRuleset().RulesetInfo, new FramedClock(new ManualClock()), Audio);
                Child = gameplayClockContainer = new MasterGameplayClockContainer(working, 0);

                gameplayClockContainer.Reset(startClock: true);
            });

            AddStep("apply frequency adjustment", () => gameplayClockContainer.AdjustmentsFromMods.AddAdjustment(AdjustableProperty.Frequency, frequencyAdjustment));
            AddAssert("track frequency changed", () => working.Track.AggregateFrequency.Value, () => Is.EqualTo(2));

            AddStep("stop using beatmap clock", () => gameplayClockContainer.StopUsingBeatmapClock());
            AddAssert("frequency adjustment unapplied", () => working.Track.AggregateFrequency.Value, () => Is.EqualTo(1));
        }

        protected override void Dispose(bool isDisposing)
        {
            localConfig?.Dispose();
            base.Dispose(isDisposing);
        }
    }
}
