// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics.Audio;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Storyboards;
using osu.Game.Storyboards.Drawables;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneStoryboardSamplePlayback : PlayerTestScene
    {
        private Storyboard storyboard;

        private IReadOnlyList<Mod> storyboardMods;

        protected override bool HasCustomSteps => true;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            config.SetValue(OsuSetting.ShowStoryboard, true);

            storyboard = new Storyboard();
            var backgroundLayer = storyboard.GetLayer("Background");
            backgroundLayer.Add(new StoryboardSampleInfo("Intro/welcome.mp3", time: -7000, volume: 20));
            backgroundLayer.Add(new StoryboardSampleInfo("Intro/welcome.mp3", time: -5000, volume: 20));
            backgroundLayer.Add(new StoryboardSampleInfo("Intro/welcome.mp3", time: 0, volume: 20));
            backgroundLayer.Add(new StoryboardSampleInfo("Intro/welcome.mp3", time: 2000, volume: 20));
        }

        [SetUp]
        public void SetUp() => storyboardMods = Array.Empty<Mod>();

        [Test]
        public void TestStoryboardSamplesStopDuringPause()
        {
            createPlayerTest();

            AddStep("player paused", () => Player.Pause());
            AddAssert("player is currently paused", () => Player.GameplayClockContainer.IsPaused.Value);
            allStoryboardSamplesStopped();

            AddStep("player resume", () => Player.Resume());
            waitUntilStoryboardSamplesPlay();
        }

        [Test]
        public void TestStoryboardSamplesStopOnSkip()
        {
            createPlayerTest();

            skipIntro();
            allStoryboardSamplesStopped();

            waitUntilStoryboardSamplesPlay();
        }

        [TestCase(typeof(OsuModDoubleTime), 1.5)]
        [TestCase(typeof(OsuModDoubleTime), 2)]
        [TestCase(typeof(OsuModHalfTime), 0.75)]
        [TestCase(typeof(OsuModHalfTime), 0.5)]
        public void TestStoryboardSamplesPlaybackWithRateAdjustMods(Type expectedMod, double expectedRate)
        {
            AddStep("setup mod", () =>
            {
                ModRateAdjust testedMod = (ModRateAdjust)Activator.CreateInstance(expectedMod).AsNonNull();
                testedMod.SpeedChange.Value = expectedRate;
                storyboardMods = new[] { testedMod };
            });

            createPlayerTest();
            skipIntro();

            AddAssert("sample playback rate matches mod rates", () => allStoryboardSamples.All(sound =>
                sound.ChildrenOfType<DrawableSample>().First().AggregateFrequency.Value == expectedRate));
        }

        [TestCase(typeof(ModWindUp), 0.5, 2)]
        [TestCase(typeof(ModWindUp), 1.51, 2)]
        [TestCase(typeof(ModWindDown), 2, 0.5)]
        [TestCase(typeof(ModWindDown), 0.99, 0.5)]
        public void TestStoryboardSamplesPlaybackWithTimeRampMods(Type expectedMod, double initialRate, double finalRate)
        {
            AddStep("setup mod", () =>
            {
                ModTimeRamp testedMod = (ModTimeRamp)Activator.CreateInstance(expectedMod).AsNonNull();
                testedMod.InitialRate.Value = initialRate;
                testedMod.FinalRate.Value = finalRate;
                storyboardMods = new[] { testedMod };
            });

            createPlayerTest();
            skipIntro();

            ModTimeRamp gameplayMod = null;

            AddUntilStep("mod speed change updated", () =>
            {
                gameplayMod = Player.GameplayState.Mods.OfType<ModTimeRamp>().Single();
                return gameplayMod.SpeedChange.Value != initialRate;
            });

            AddAssert("sample playback rate matches mod rates", () => allStoryboardSamples.All(sound =>
                sound.ChildrenOfType<DrawableSample>().First().AggregateFrequency.Value == gameplayMod.SpeedChange.Value));
        }

        private void createPlayerTest()
        {
            CreateTest();

            AddAssert("storyboard loaded", () => Player.Beatmap.Value.Storyboard != null);
            waitUntilStoryboardSamplesPlay();
        }

        private void waitUntilStoryboardSamplesPlay() => AddUntilStep("any storyboard samples playing", () => allStoryboardSamples.Any(sound => sound.IsActivelyPlaying));

        private void allStoryboardSamplesStopped() => AddAssert("all storyboard samples stopped immediately", () => allStoryboardSamples.All(sound => !sound.IsActivelyPlaying));

        private void skipIntro() => AddStep("skip intro", () => InputManager.Key(Key.Space));

        private IEnumerable<DrawableStoryboardSample> allStoryboardSamples => Player.ChildrenOfType<DrawableStoryboardSample>();

        protected override bool AllowFail => false;

        protected override TestPlayer CreatePlayer(Ruleset ruleset)
        {
            SelectedMods.Value = SelectedMods.Value.Concat(storyboardMods).ToArray();
            return new TestPlayer(true, false);
        }

        protected override Ruleset CreatePlayerRuleset() => new OsuRuleset();

        protected override WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap, Storyboard storyboard = null) =>
            new ClockBackedTestWorkingBeatmap(beatmap, storyboard ?? this.storyboard, Clock, Audio);
    }
}
