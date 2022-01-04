// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Storyboards;
using osu.Game.Storyboards.Drawables;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneStoryboardSamplePlayback : PlayerTestScene
    {
        private Storyboard storyboard;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            config.SetValue(OsuSetting.ShowStoryboard, true);

            storyboard = new Storyboard();
            var backgroundLayer = storyboard.GetLayer("Background");
            backgroundLayer.Add(new StoryboardSampleInfo("Intro/welcome.mp3", time: -7000, volume: 20));
            backgroundLayer.Add(new StoryboardSampleInfo("Intro/welcome.mp3", time: -5000, volume: 20));
            backgroundLayer.Add(new StoryboardSampleInfo("Intro/welcome.mp3", time: 0, volume: 20));
        }

        [Test]
        public void TestStoryboardSamplesStopDuringPause()
        {
            checkForFirstSamplePlayback();

            AddStep("player paused", () => Player.Pause());
            AddAssert("player is currently paused", () => Player.GameplayClockContainer.IsPaused.Value);
            AddAssert("all storyboard samples stopped immediately", () => allStoryboardSamples.All(sound => !sound.IsPlaying));

            AddStep("player resume", () => Player.Resume());
            AddUntilStep("any storyboard samples playing after resume", () => allStoryboardSamples.Any(sound => sound.IsPlaying));
        }

        [Test]
        public void TestStoryboardSamplesStopOnSkip()
        {
            checkForFirstSamplePlayback();

            AddStep("skip intro", () => InputManager.Key(osuTK.Input.Key.Space));
            AddAssert("all storyboard samples stopped immediately", () => allStoryboardSamples.All(sound => !sound.IsPlaying));

            AddUntilStep("any storyboard samples playing after skip", () => allStoryboardSamples.Any(sound => sound.IsPlaying));
        }

        private void checkForFirstSamplePlayback()
        {
            AddAssert("storyboard loaded", () => Player.Beatmap.Value.Storyboard != null);
            AddUntilStep("any storyboard samples playing", () => allStoryboardSamples.Any(sound => sound.IsPlaying));
        }

        private IEnumerable<DrawableStoryboardSample> allStoryboardSamples => Player.ChildrenOfType<DrawableStoryboardSample>();

        protected override bool AllowFail => false;

        protected override Ruleset CreatePlayerRuleset() => new OsuRuleset();
        protected override TestPlayer CreatePlayer(Ruleset ruleset) => new TestPlayer(true, false);

        protected override WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap, Storyboard storyboard = null) =>
            new ClockBackedTestWorkingBeatmap(beatmap, storyboard ?? this.storyboard, Clock, Audio);
    }
}
